using System.Net.Sockets;
using System.Text;

namespace MockCamera.Models;

public class Session
{
    private int _sequenceNumber;
    private readonly TcpClient _client;

    public Session(TcpClient client)
    {
        _client = client;
        _sequenceNumber = 0;
    }

    public void SetSequence(int number)
    {
        _sequenceNumber = number;
    }

    public int GetSequence()
    {
        return _sequenceNumber;
    }

    public async Task StartSession()
    {
        await using var clientStream = _client.GetStream();
        var buffer = new byte[256];
        var requestData = string.Empty; 
        int read;
    
        while ((read = await clientStream.ReadAsync(buffer)) != 0)
        {
            requestData += Encoding.UTF8.GetString(buffer, 0, read);
            Array.Clear(buffer, 0, buffer.Length);

            if (requestData.Contains("\r\n\r\n"))
            {
                var request = new RtspRequest(requestData);
                
                Console.WriteLine($"OriginalUrl: {request.Url}");
                Console.WriteLine($"Protocol: {request.Protocol}");
                Console.WriteLine($"Method:{request.Method}");
                Console.WriteLine($"CSeq:{request.SequenceNumber}");
                Console.WriteLine(string.Join("\r\n", request.Headers));
                
                var okResponse = $"RTSP/1.0 200 OK{Environment.NewLine}{Environment.NewLine}";
                var response = Encoding.UTF8.GetBytes(okResponse);
                await clientStream.WriteAsync(response);
                break;
            }
        }
    }
}