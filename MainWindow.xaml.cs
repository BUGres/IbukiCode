using System.Configuration;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using System.Linq;
using System.IO;
using System.Diagnostics;

namespace IbukiCode
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string fileType = "";
        public static string filePath = "";
        public static Dictionary<Rectangle, double> CodeLightHeightRem = new Dictionary<Rectangle, double>();

        public MainWindow()
        {
            InitializeComponent();
            #region Window设置
            // 置顶本窗口
            this.Topmost = true;

            // 通过WindowChrome设置标题栏高度
            WindowChrome.SetWindowChrome(this, new WindowChrome() { CaptionHeight = 30 });

            #endregion

            #region 配置文件读取
            FromConfigDynamicColor("BackgroundColor");
            FromConfigDynamicColor("TitleColor");
            FromConfigDynamicColor("CodeColor");
            #endregion

            #region 初始化一些文本
            string CodeLeftLineCountText = string.Empty;
            for (int i = 1; i < 1000; i++)
            {
                CodeLeftLineCountText += i + "\n";
            }
            CodeLeftLineCount.Text = CodeLeftLineCountText.Substring(0, CodeLeftLineCountText.Length - 1);

            // override 关键字
            if (File.Exists("Edit/shader_type.txt"))
            {
                KeyWords.shader.Clear();
                var lines = File.ReadAllLines("Edit/shader_type.txt");
                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    KeyWords.shader.Add(line);
                }
            }
            if (File.Exists("Edit/shader_valueType.txt"))
            {
                KeyWords.shader_valueType.Clear();
                var lines = File.ReadAllLines("Edit/shader_valueType.txt");
                foreach (var line in lines)
                {
                    if (string.IsNullOrEmpty(line)) continue;
                    KeyWords.shader_valueType.Add(line);
                }
            }
            #endregion
        }

        #region UI交互

        static public void FromConfigDynamicColor(string str)
        { 
            var color = ConfigurationManager.AppSettings[str];
            Application.Current.Resources[str] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
        }

        // 代码变更
        private void LeftCode_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 获取当前的文本
            var str = LeftCode.Text;
            if (str[^1] != '\n') str = str + "\n";
            // 清理之前的标注
            CodeLightHeightRem.Clear();
            CodeLightCanvas.Children.Clear();
            if (fileType == "shader" || fileType == "hlsl")
            {
                for (int i = 0; i < KeyWords.shader.Count; i++)
                { 
                    string keyword = KeyWords.shader[i];
                    List<int> keys = FindAllOccurrences(str, keyword);
                    foreach (var index in keys)
                    {
                        if (index > 0 && 
                            str[index - 1] != ' ' &&
                            str[index - 1] != '\r' &&
                            str[index - 1] != '\n' && 
                            str[index - 1] != '\t')
                        {
                            continue;
                        }
                        else
                        if (str[index + keyword.Length] != ' ' &&
                            str[index + keyword.Length] != '\r' &&
                            str[index + keyword.Length] != '\n' && 
                            str[index + keyword.Length] != '\t')
                        {
                            continue;
                        }

                        var r = LeftCode.GetRectFromCharacterIndex(index);
                        var r2 = LeftCode.GetRectFromCharacterIndex(index + keyword.Length);
                        // 如果是关键字中的值类型
                        var lightcolor = KeyWords.shader_valueType.IndexOf(keyword) == -1 ? "CodeLightColor" : "CodeLightColor_ValueType";
                        var area = new Rectangle() {
                            Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ConfigurationManager.AppSettings[lightcolor])),
                            RadiusX = 5,
                            RadiusY = 5,
                        };
                        area.Width = r2.X - r.X;
                        area.Height = r.Height;
                        CodeLightCanvas.Children.Add(area);
                        Canvas.SetLeft(area, r.X + 30);
                        Canvas.SetTop(area, r.Y + 90);
                        CodeLightHeightRem.Add(area, r.Y + 90);
                    }
                }

                List<string> paragraphKeywards = new List<string>() { "Properties", "struct", "Pass", "maxvertexcount" };
                foreach (var paragraphKeyword in paragraphKeywards)
                {
                    // Properties关键字
                    List<int> Propertieskeys = FindAllOccurrences(str, paragraphKeyword);
                    foreach (var index in Propertieskeys)
                    {
                        var r = LeftCode.GetRectFromCharacterIndex(index);
                        int endlineCount = 1; // 行数量
                        int leftICount = 0; // 左括号计数
                                            // bool isFirstLeftICountZero = true; // 第一次归零
                        for (int i = 0; i < str.Length - index; i++)
                        {
                            if (str[i + index] == '\n') endlineCount++;
                            else if (str[i + index] == '{') leftICount++;
                            else if (str[i + index] == '}') leftICount--;

                            if (leftICount == 0 && str[i + index] == '}')
                            {
                                break;
                            }
                        }
                        var area = new Rectangle()
                        {
                            Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ConfigurationManager.AppSettings["BackgroundColor_deepGround"])),
                            RadiusX = 1,
                            RadiusY = 1,
                        };
                        area.Width = 1000000;
                        area.Height = r.Height * endlineCount;
                        CodeLightCanvas.Children.Add(area);
                        Canvas.SetLeft(area, 0);
                        Canvas.SetTop(area, r.Y + 90);
                        CodeLightHeightRem.Add(area, r.Y + 90);
                    }
                }
            }

            // 触发一次重新layout
            CodeViewChange(null, null);
        }

        // 查询函数input中出现的substring
        static List<int> FindAllOccurrences(string input, string substring)
        {
            List<int> positions = new List<int>();
            int index = input.IndexOf(substring);

            while (index != -1)
            {
                positions.Add(index);
                index = input.IndexOf(substring, index + substring.Length);
            }

            return positions;
        }

        // 代码翻动
        private void CodeViewChange(object sender, ScrollChangedEventArgs e)
        {
            for (int i = 0; i < CodeLightCanvas.Children.Count; i++)
            {
                Canvas.SetTop(CodeLightCanvas.Children[i], CodeLightHeightRem[CodeLightCanvas.Children[i] as Rectangle] - CodeView.VerticalOffset);
            }
        }

        // 当文件拖动进入窗口时的事件
        private void DragHang(object sender, DragEventArgs e)
        {
            // 检查拖动的数据类型是否为文件
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;  // 显示可复制的效果
            }
            else
            {
                e.Effects = DragDropEffects.Scroll;  // 不允许拖放
            }
        }

        // 当文件被放下时的事件
        private void DropEnter(object sender, DragEventArgs e)
        {
            // 获取拖入的文件
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Length > 0)
                {
                    // 处理第一个文件
                    var file = files[0];
                    LeftCode.Text = File.ReadAllText(file);
                    fileType = file.Split('.')[^1];
                    filePath = file;
                    LeftCode_TextChanged(null, null);
                    TitleText.Text = "IbukiCode Edit as TXT";
                    if (KeyWords.supportLevelOne.IndexOf(fileType) != -1)
                    {
                        TitleText.Text = "IbukiCode " + "Edit as " + fileType + " (Support)";
                    }
                    else if (KeyWords.supportLevelTwo.IndexOf(fileType) != -1)
                    {
                        TitleText.Text = "IbukiCode " + "Edit as " + fileType + " (Low-Level Support)";
                    }
                }
            }

            edit.tool.WakeUp();
            EditType_Title.IsEnabled = false;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {

            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                this.Close();
            }

            e.Handled = true; // 禁止在消息队列上继续传递
        }

        // 鼠标双击背景板
        private void Window_MouseDouble(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void OpenWithExplor_Click(object sender, RoutedEventArgs e)
        {
            var paths = filePath.Split('\\');
            if (paths.Length <= 1) return;
            string path = string.Empty;
            for (int i = 0; i < paths.Length - 1; i++)
            {
                path += paths[i] + '\\';
            }
            path = path.Substring(0, path.Length - 1);
            Process.Start("Explorer.exe", path);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (filePath == string.Empty) return;
            try
            {
                File.WriteAllText(filePath, LeftCode.Text);
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void Link_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LeftCode.Text)) return;
            MyUdpServer.Send(LeftCode.Text);
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NewWindow_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(Environment.CurrentDirectory + "\\IbukiCode.exe");
            }
            catch (Exception ee)
            { 
                MessageBox.Show(ee.Message);
            }
        }

        private void EditType_Main_SelectChanged(object sender, SelectionChangedEventArgs e)
        {
            if (KeyWords.supportLevelOne.IndexOf(fileType) == -1)
            {
                EditType_Title.IsEnabled = false;
                e.Handled = true;
                return;
            }

            // 检查是否有选中项
            if (EditType_Main.SelectedItem is ComboBoxItem selectedItem)
            {
                EditType_Main_Text.Text = ((e.AddedItems[0] as ComboBoxItem).Content as string);
                if (EditType_Main_Text.Text == "插入宏")
                {
                    EditType_Title.Items.Clear();
                    foreach (var line in edit.tool.hong)
                    {
                        EditType_Title.Items.Add(new ComboBoxItem
                        {
                            Content = line.Key,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ConfigurationManager.AppSettings["CodeColor"])),
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ConfigurationManager.AppSettings["BackgroundColor"])),
                            BorderThickness = new Thickness(0, 0, 0, 0),
                        });
                    }
                }
                else if (EditType_Main_Text.Text == "插入变量")
                {
                    EditType_Title.Items.Clear();
                    foreach (var line in edit.tool.bianliang)
                    {
                        EditType_Title.Items.Add(new ComboBoxItem
                        {
                            Content = line.Key,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ConfigurationManager.AppSettings["CodeColor"])),
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ConfigurationManager.AppSettings["BackgroundColor"])),
                            BorderThickness = new Thickness(0, 0, 0, 0),
                        });
                    }
                }
                else if (EditType_Main_Text.Text == "插入函数")
                {
                    EditType_Title.Items.Clear();
                    foreach (var line in edit.tool.hanshu)
                    {
                        EditType_Title.Items.Add(new ComboBoxItem
                        {
                            Content = line.Key,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ConfigurationManager.AppSettings["CodeColor"])),
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ConfigurationManager.AppSettings["BackgroundColor"])),
                            BorderThickness = new Thickness(0, 0, 0, 0),
                        });
                    }
                }
                else if (EditType_Main_Text.Text == "插入类")
                {
                    EditType_Title.Items.Clear();
                    foreach (var line in edit.tool.lei)
                    {
                        EditType_Title.Items.Add(new ComboBoxItem
                        {
                            Content = line.Key,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ConfigurationManager.AppSettings["CodeColor"])),
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ConfigurationManager.AppSettings["BackgroundColor"])),
                            BorderThickness = new Thickness(0, 0, 0, 0),
                        });
                    }
                }
                else if (EditType_Main_Text.Text == "从模板复制")
                {
                    EditType_Title.Items.Clear();
                    foreach (var line in edit.tool.moban)
                    {
                        EditType_Title.Items.Add(new ComboBoxItem
                        {
                            Content = line.Key,
                            Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ConfigurationManager.AppSettings["CodeColor"])),
                            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ConfigurationManager.AppSettings["BackgroundColor"])),
                            BorderThickness = new Thickness(0, 0, 0, 0),
                        });
                    }
                }
                EditType_Title.IsEnabled = true;
            }
        }

        private void EditType_Title_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 检查是否有选中项
            if (EditType_Title.SelectedItem is ComboBoxItem selectedItem)
            {
                EditType_Title.Text = ((e.AddedItems[0] as ComboBoxItem).Content as string);
                string tocpy = "";
                if (EditType_Main_Text.Text == "插入宏") tocpy = (edit.tool.hong[EditType_Title.Text]);
                if (EditType_Main_Text.Text == "插入变量") tocpy = (edit.tool.bianliang[EditType_Title.Text]);
                if (EditType_Main_Text.Text == "插入函数") tocpy = (edit.tool.hanshu[EditType_Title.Text]);
                if (EditType_Main_Text.Text == "插入类") tocpy = (edit.tool.lei[EditType_Title.Text]);
                if (EditType_Main_Text.Text == "从模板复制") tocpy = (edit.tool.moban[EditType_Title.Text]);

                try
                {
                    Clipboard.SetDataObject(tocpy);
                }
                catch (Exception ee)
                {
                    MessageBox.Show(tocpy + "\n" + ee.Message);
                }
            }
        }

        #endregion
    }

    #region 内联

    internal class KeyWords
    {
        // 一级支持：可以启用编辑工具，可以高亮标注
        public static List<string> supportLevelOne = new List<string>
        {
            "shader",
            "lua",
            "hlsl",
        };
        // 二级支持：可以高亮标注
        // 不支持：仅按照txt打开
        public static List<string> supportLevelTwo = new List<string>
        {
            "cs",
            "cpp",
            "py",

        };
        public static List<string> shader = new List<string>
        {
            "CGPROGRAM",
            "ENDCG",
            "POSITION",
            "#pragma",
            "vertex",
            "fragment",
            "TEXCOORD0",
            "TEXCOORD1",
            "TEXCOORD2",
            "TEXCOORD3",

            "flaot2",
            "float3",
            "float4",
            "float",
        };
        public static List<string> shader_valueType = new List<string>
        {
            "flaot2",
            "float3",
            "float4",
            "float",
        };
    }

    internal class edit
    {
        static edit _tool = null;
        public static edit tool
        {
            get
            {
                if (_tool == null)
                {
                    _tool = new edit();

                    return _tool;
                }
                else
                {
                    if (_tool.type != MainWindow.fileType)
                    {
                        _tool = new edit();
                    }
                    return _tool;
                }
            }
        }

        edit()
        {
            type = MainWindow.fileType;
            try
            {
                var lines = File.ReadAllLines("Edit/" + type + ".txt");
                foreach (var line in lines)
                {
                    var parts = line.Split('\t');
                    if (parts[0] == "插入宏") hong.Add(parts[1], parts[2]);
                    else if (parts[0] == "插入变量") bianliang.Add(parts[1], parts[2]);
                    else if (parts[0] == "插入函数") hanshu.Add(parts[1], parts[2]);
                    else if (parts[0] == "插入类") lei.Add(parts[1], parts[2]);
                    else if (parts[0] == "从模板复制")
                    {
                        if (File.Exists("Edit/" + parts[2] + ".txt"))
                        {
                            moban.Add(parts[1], File.ReadAllText("Edit/" + parts[2] + ".txt"));
                        }
                        else
                        {
                            MessageBox.Show("无法打开文件" + "Edit/" + parts[2] + ".txt", "出现了一些意外");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                // MessageBox.Show(e.Message, "出现了一些意外");
            }
        }

        public void WakeUp()
        { 
            
        }

        string type = string.Empty;

        public Dictionary<string, string> hong = new Dictionary<string, string>();
        public Dictionary<string, string> bianliang = new Dictionary<string, string>();
        public Dictionary<string, string> hanshu = new Dictionary<string, string>();
        public Dictionary<string, string> lei = new Dictionary<string, string>();
        public Dictionary<string, string> moban = new Dictionary<string, string>();
    }
    #endregion
}