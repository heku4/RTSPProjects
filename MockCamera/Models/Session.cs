using System.Net.Sockets;
using System.Text;

namespace MockCamera.Models;

public class Session
{
    private readonly TcpClient _client;

    public Session(TcpClient client)
    {
        _client = client;
    }

    public async Task StartSession(CancellationToken stoppingToken)
    {
        await using var clientStream = _client.GetStream();
        var buffer = new byte[256];
        var requestData = string.Empty; 
        int read;
    
        while ((read = await clientStream.ReadAsync(buffer, stoppingToken)) != 0)
        {
            requestData += Encoding.UTF8.GetString(buffer, 0, read);
            Array.Clear(buffer, 0, buffer.Length);

            if (requestData.Contains("\r\n\r\n"))
            {
                var request = new RtspRequest(requestData);
                
                Console.WriteLine(request.ToString());

                var okResponse = $"RTSP/1.0 200 OK{Environment.NewLine}{Environment.NewLine}";
                var response = Encoding.UTF8.GetBytes(okResponse);
                await clientStream.WriteAsync(response, stoppingToken);
                break;
            }
        }
    }
}