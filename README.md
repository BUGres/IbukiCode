![运行截图](https://github.com/BUGres/IbukiCode/blob/main/运行时.png)

### 使用方法

+ 将受支持的文件拖入窗口即可

+ 目前一级支持`有高亮，有代码小抄`的语言：`CG`
+ 目前二级支持`有高亮`的语言：
+ 三级支持：会当作`TXT`打开并编辑

+ 如果希望支持你喜好语言，需要自己修改一些文件，还需要自己读代码改一些（代码很短）

  您可能关注的文件（以CG语言写的UnityShader为例）：

  `App.config` 包含五个**配色**

  ```config
  <?xml version="1.0" encoding="utf-8" ?>
  <!-- App.config -->
  <configuration>
    <appSettings>
      <add key="BackgroundColor" value="#FF333333"/>
      <add key="TitleColor" value="#FF1E1E20"/>
      <add key="CodeColor" value="#FFACACAC"/>
      <add key="CodeLightColor" value="#ff126e82"/>
      <add key="CodeLightColor_ValueType" value="#ff485b4d"/>
    </appSettings>
  </configuration>
  ```

  `Edit/shader.txt` 用于定制你自己的代码小抄，下面的每行**三部分必须用Tab隔开**

  ```
  插入宏	透明混合	Blend SrcAlpha One
  插入宏	透明批声明	Tags { "RenderType"="Opaque" "Queue"="Transparent" }
  ```

  `插入宏`是程序内第一个选项，`透明混合`是程序内第二个选项，都选好了会自动给`Blend SrcAlpha One`复制剪切板

  `Edit/shader_type.txt` 所有高亮的文本

  ```
  CGPROGRAM
  ENDCG
  POSITION
  #pragma
  vertex
  fragment
  TEXCOORD0
  TEXCOORD1
  TEXCOORD2
  TEXCOORD3
  flaot2
  float3
  float4
  float
  ```

  `Edit/shader_valueType.txt` 值类型，高亮颜色与上面不同

  ```
  flaot2
  float3
  float4
  float
  ```

  

