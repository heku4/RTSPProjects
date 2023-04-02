
namespace MockCamera.Models;

public abstract class RtspMessage
{
    public string Protocol { get; init; } = null!;
    public  int SequenceNumber { get; init; }
    public RtspMethod Method { get; init; }
    public Dictionary<string, string> Headers { get; } = new ();

    public override string ToString()
    {
        var rtspMessage = this;

        var result = @$"
Protocol: {rtspMessage.Protocol}
Method: {rtspMessage.Method}
CSeq: {rtspMessage.SequenceNumber}";

        if (rtspMessage.Headers.Any())
        {
            result += $"{Environment.NewLine}Headers:\r\n{string.Join($"{Environment.NewLine}", rtspMessage.Headers)}{Environment.NewLine}";
        }
            
        return result;
    }
}