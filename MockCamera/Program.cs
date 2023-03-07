using System.Net;
using System.Net.Sockets;
using System.Text;

var tcpListener = new TcpListener(new IPAddress(new byte[]{0,0,0,0}), 9898);
tcpListener.Start();
while (true)
{
    var client = await tcpListener.AcceptTcpClientAsync();
    
    await Task.Run(() => Echo(client));
}

async Task Echo(TcpClient client)
{
    await using var clientStream = client.GetStream();
    var buffer = new byte[256];
    var request = string.Empty; 
    int read;
    while ((read = await clientStream.ReadAsync(buffer)) != 0)
    {
        request += Encoding.UTF8.GetString(buffer, 0, read);
        Array.Clear(buffer, 0, buffer.Length);

        if (request.Contains("\r\n\r\n"))
        {
            Console.Write(request);
            var okResponse = $"HTTP/1.1 200 OK{Environment.NewLine}{Environment.NewLine}";
            var response = Encoding.UTF8.GetBytes(okResponse);
            await clientStream.WriteAsync(response);
            break;
        }
    }
    
    client.Close();
    client.Dispose();
}