using System.Threading.Channels;

namespace MockCamera.Services;

public class PlayQueueHandler
{
    private readonly Channel<(ulong, int)> _toPlayChannel;
    private readonly Channel<ulong> _teardownChannel;

    public PlayQueueHandler(int bound)
    {
        _teardownChannel = Channel.CreateBounded<ulong>(bound);
        _toPlayChannel = Channel.CreateBounded<(ulong, int)>(bound);
    }

    public bool TryPeekNewSession(out (ulong SessionId, int Port) sessionData)
    {
        if (_toPlayChannel.Reader.TryPeek(out sessionData))
        {
            _toPlayChannel.Reader.TryRead(out sessionData);
            return true;
        }

        return false;
    }
    
    public bool TryPeekSessionToTeardown(out ulong sessionId)
    {
        if (_teardownChannel.Reader.TryPeek(out sessionId))
        {
            _teardownChannel.Reader.TryRead(out sessionId);
            return true;
        }

        return false;
    }

    public async Task AddNewSessionAsync(ulong sessionId, int port)
    {
        await _toPlayChannel.Writer.WriteAsync((sessionId, port));
    }
    
    public async Task AddSessionToTeardownQueueAsync(ulong sessionId)
    {
        await _teardownChannel.Writer.WriteAsync(sessionId);
    }
}