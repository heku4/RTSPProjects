using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MockCamera.Models;

public class Session
{
    private readonly TcpClient _client;
    private static readonly Random Rand = new();
    private readonly ulong _sessionId;
    private bool _isStreaming;
    private int _clientRtpPort;
    private int _clientRtcpPort;

    private const string ContentData = @"v=0
m=video 0 RTP/AVP 26
a=control:1";

    public Session(TcpClient client, int udpPort1, int udpPort2)
    {
        _client = client;
        _sessionId = (ulong)Rand.NextInt64(1, long.MaxValue);
        _clientRtpPort = udpPort1;
        _clientRtcpPort = udpPort2;
    }

    public ulong GetSessionId()
    {
        return _sessionId;
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

                requestData = string.Empty;


                var rtspResponse = HandleRequest(request);


                var response = Encoding.UTF8.GetBytes(rtspResponse.Format());

                Console.WriteLine(rtspResponse.Format());
                await clientStream.WriteAsync(response, tokenSource.Token);

                if (_isStreaming)
                {
                    await SendRtpPacket();
                }

                if (rtspResponse.Method == RtspMethod.TEARDOWN)
                {
                    tokenSource.Cancel();
                    break;
                }
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
            case RtspMethod.PLAY:
                rtspResponse = HandlePlayRequest(request);
                break;
            case RtspMethod.TEARDOWN:
                rtspResponse = HandleTeardownRequest(request);
                break;
            default:
                rtspResponse = new RtspResponse(request, new Dictionary<string, string>(), null, 404);
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

        var headers = new Dictionary<string, string> { { "Public", string.Join(", ", supportedMethods) } };

        var response = new RtspResponse(request, headers, null, 200);

        return response;
    }

    private RtspResponse HandleDescribeRequest(RtspRequest request)
    {
        var headers = new Dictionary<string, string>
        {
            { "Content-Base", "rtsp://localhost:9898" },
            { "Content-type", "application/sdp" }
        };

        var response = new RtspResponse(request, headers, ContentData, 200);

        return response;
    }

    private RtspResponse HandleSetupRequest(RtspRequest request)
    {
        var statusCode = 200;
        var headers = new Dictionary<string, string>();

        var clientPortsHeaderData = request.Headers["Transport"].Trim().Split("=");
        if (clientPortsHeaderData.Length != 2)
        {
            return new RtspResponse(request, headers, null, 400);
        }

        var ports = clientPortsHeaderData[^1].Split("-");

        if (ports.Length != 2)
        {
            return new RtspResponse(request, headers, null, 400);
        }

        try
        {
            _clientRtpPort = Convert.ToInt32(ports[0]);
            _clientRtcpPort = Convert.ToInt32(ports[1]);
        }
        catch
        {
            return new RtspResponse(request, headers, null, 400);
        }

        headers.Add("Session", _sessionId.ToString());
        headers.Add("Transport", $"{request.Headers["Transport"]};server_port:{_clientRtpPort}-{_clientRtcpPort}");

        var response = new RtspResponse(request, headers, null, statusCode);

        return response;
    }

    private RtspResponse HandlePlayRequest(RtspRequest request)
    {
        var headers = new Dictionary<string, string>
        {
            { "Session", _sessionId.ToString() }
        };

        var response = new RtspResponse(request, headers, null, 200);

        _isStreaming = true;

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

        _isStreaming = false;

        return response;
    }

    private async Task SendRtpPacket()
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);

        var serverAddr = new IPAddress(new byte[] { 127, 0, 0, 1 });
        var endPoint = new IPEndPoint(serverAddr, _clientRtpPort);

        uint packNumber = 0;
        uint timeStamp = 0;
        var changeFlag = false;

        Console.WriteLine($"Starting send packets on {_sessionId}");

        while (_isStreaming)
        {
             timeStamp += 3600;

            var dataToSend = RtpPacket.Prepare(timeStamp, packNumber, changeFlag);
            await socket.SendToAsync(dataToSend, endPoint);

            changeFlag = !changeFlag;
            packNumber++;

            await Task.Delay(40);
        }

        Console.WriteLine($"Stopping send packets on {_sessionId}");
    }
}