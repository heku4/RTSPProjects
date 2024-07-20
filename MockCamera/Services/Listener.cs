using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using MockCamera.Options;
using MockCamera.Models;

public class RtspListener
{
    private readonly ServerOptions _serverOptions;
    private readonly ILogger<RtspListener> _logger;

    public RtspListener(ServerOptions serverOptions, ILogger<RtspListener> logger)
    {
        _serverOptions = serverOptions;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var tcpListener = new TcpListener(new IPAddress(new byte[] { 0, 0, 0, 0 }), _serverOptions.RtspPort);
        tcpListener.Start();

        while (cancellationToken.IsCancellationRequested is false)
        {
            try
            {
                var client = await tcpListener.AcceptTcpClientAsync(cancellationToken);
                var session = new Session(client, _serverOptions.RtspPort);

                var thread = new Thread(() =>
                {
                    Task.Run(() => session.StartSession(new CancellationTokenSource()), cancellationToken);
                });

                thread.Start();
                thread.Join();
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
            }
        }
    }
}