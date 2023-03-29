using System.Net.Sockets;
using System.Text;

namespace MockCamera.Models;

public class Session
{
    private readonly TcpClient _client;
    private uint _sessionCounter;

    private const string ContentData = @"v=0
m=video 0 RTP/AVP 26
a=control:1";

    public Session(TcpClient client)
    {
        _client = client;
    }

    public async Task StartSession(CancellationTokenSource tokenSource)
    {
        await using var clientStream = _client.GetStream();
        var buffer = new byte[256];
        var requestData = string.Empty; 
        int read;
    
        while ((read = await clientStream.ReadAsync(buffer, tokenSource.Token)) != 0)
        {
            requestData += Encoding.UTF8.GetString(buffer, 0, read);
            Array.Clear(buffer, 0, buffer.Length);

            if (requestData.Contains("\r\n\r\n"))
            {
                var request = new RtspRequest(requestData);

                Console.WriteLine(requestData);

                var rtspResponse = HandleRequest(request);
                
                Console.WriteLine(rtspResponse.Format());
                
                var response = Encoding.UTF8.GetBytes(rtspResponse.Format());
                
                await clientStream.WriteAsync(response, tokenSource.Token);

                if (rtspResponse.Method == RtspMethod.TEARDOWN)
                {
                    tokenSource.Cancel();
                }
                
                break;
            }
        }
    }

    private RtspResponse HandleRequest(RtspRequest request)
    {
        RtspResponse rtspResponse;
        
        switch (request.Method)
        {
            case RtspMethod.OPTIONS:
                rtspResponse = HandleOptionsRequest(request);
                break;
            case RtspMethod.DESCRIBE:
                rtspResponse = HandleDescribeRequest(request);
                break;
            case RtspMethod.SETUP:
                rtspResponse = HandleSetupRequest(request);
                break;
            case RtspMethod.TEARDOWN:
                rtspResponse = HandleTeardownRequest(request);
                break;
            default:
                rtspResponse = new RtspResponse(request, new Dictionary<string, string>(), null, 200);
                break;
        }
        
        return rtspResponse;
    }

    private RtspResponse HandleOptionsRequest(RtspRequest request)
    {
        var supportedMethods = new[]
        {
            RtspMethod.DESCRIBE.ToString(), RtspMethod.SETUP.ToString(),
            RtspMethod.PLAY.ToString(), RtspMethod.TEARDOWN.ToString()
        };
        
        var headers = new Dictionary<string, string> { { "Public", String.Join(", ", supportedMethods) } };

        var response = new RtspResponse(request, headers, ContentData,200);

        return response;
    }
    
    private RtspResponse HandleDescribeRequest(RtspRequest request)
    {
        var contentLength = ContentData.Length;

        var headers = new Dictionary<string, string>
        {
            { "Content-Base", "rtsp://localhost:9898" },
            { "Content-type", "application/sdp" },
            { "Content-Length", contentLength.ToString() }
        };

        var response = new RtspResponse(request, headers, null, 200);

        return response;
    }
    
    private RtspResponse HandleSetupRequest(RtspRequest request)
    {
        var headers = new Dictionary<string, string>
        {
            { "Session", GetSessionNumber().ToString() },
            { "Transport", $"{request.Headers["Transport"]};server_port:{7057}-{7058}" }
        };
        
        var response = new RtspResponse(request, headers, null, 200);

        return response;
    }
    
    private RtspResponse HandleTeardownRequest(RtspRequest request)
    {
        var headers = new Dictionary<string, string>();

        if (request.Headers.Any(h => h.Key == "Session"))
        {
            headers.Add("Session", request.Headers["Session"]);
        }

        var response = new RtspResponse(request, headers, null, 200);

        return response;
    }

    private uint GetSessionNumber()
    {
        return _sessionCounter++;//4346886530419407805
    }
}