using System.Net;
using System.Net.Sockets;

var tcpListener = new TcpListener(new IPAddress(new byte[4]{0,0,0,0}), 9898);
tcpListener.Start();
while (true)
{
    var client = await tcpListener.AcceptTcpClientAsync();
    Task.Run(() => Echo(client));
    
}

async Task Echo(TcpClient client)
{
    await using var clientStream = client.GetStream();
    var buffer = new byte[1024];
    int readBytes = 0;
    while ((readBytes = await clientStream.ReadAsync(buffer)) != 0)
    {
        Console.WriteLine(System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length));
    } 
    client.Close();
    client.Dispose();
}