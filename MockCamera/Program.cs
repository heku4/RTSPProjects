using System.Net;
using System.Net.Sockets;
using System.Text;

var tcpListener = new TcpListener(new IPAddress(new byte[]{0,0,0,0}), 9898);
tcpListener.Start();
while (true)
{
    var client = await tcpListener.AcceptTcpClientAsync();
    if (client is null)
    {
        continue;
    }
    await Task.Run(() => Echo(client));
}

async Task Echo(TcpClient client)
{
    await using var clientStream = client.GetStream();
    var buffer = new byte[50];
    var request = new StringBuilder();
    var read = 0;
    while ((read = await clientStream.ReadAsync(buffer)) != 0)
    {
        var reqPart = Encoding.UTF8.GetString(buffer, 0, read);
        request.Append(reqPart);
        Array.Clear(buffer, 0, buffer.Length);
        Console.Write(reqPart);

        //change to request
        if (reqPart.Contains($"{Environment.NewLine}{Environment.NewLine}"))
        {
            var okResponse = $"HTTP/1.1 200 OK{Environment.NewLine}{Environment.NewLine}";
            var response = Encoding.UTF8.GetBytes(okResponse);
            await clientStream.WriteAsync(response);
            break;
        }
    }
    
    client.Close();
    client.Dispose();
}