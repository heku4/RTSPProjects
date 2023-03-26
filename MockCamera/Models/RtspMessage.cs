
namespace MockCamera.Models;

public abstract class RtspMessage
{
    public string Protocol { get; init; }
    public  int SequenceNumber { get; init; }
    public RtspMethod Method { get; init; }
    public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();

    public override string ToString()
    {
        var rtspMessage = this;

        var result = @$"
Protocol: {rtspMessage.Protocol}
Method:{rtspMessage.Method}
CSeq:{rtspMessage.SequenceNumber}";

        if (rtspMessage.Headers.Any())
        {
            result += $"\r\nHeaders:\r\n{string.Join("\r\n", rtspMessage.Headers)}\r\n";
        }
            
        return result;
    }
}