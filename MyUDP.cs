using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class MyUdpConfig
{
    static public int port = 11006; // 端口
    static public string serverAddress = "127.0.0.1"; // 服务器地址
}

class MyUdpServer
{
    private static UdpClient udpClient = new UdpClient();

    static public async Task Send(string str)
    {
        await SendMessageAsync(str, MyUdpConfig.serverAddress); // 此处发送数据
    }

    private static async Task SendMessageAsync(string message, string serverAddress)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        await udpClient.SendAsync(data, data.Length, serverAddress, MyUdpConfig.port);
    }
}

class MyUdpClient
{
    static public async Task Init(Action<string> callback)
    {
        UdpClient udpServer = new UdpClient(MyUdpConfig.port);
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

        while (true)
        {
            UdpReceiveResult receivedResult = await udpServer.ReceiveAsync(); // 此处接收数据
            string receivedMessage = Encoding.UTF8.GetString(receivedResult.Buffer);
            callback(receivedMessage);
        }
    }
}