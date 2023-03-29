namespace MockCamera.Models;

public class RtspResponse: RtspMessage
{
    public int StatusCode { get; init; }
    public string ReasonPhrase { get; init; }
    public string? Body { get; init; }

    public RtspResponse(RtspRequest request, Dictionary<string, string> headers, string? bodyData, int statusCode)
    {
        Protocol = request.Protocol;
        Method = request.Method;
        SequenceNumber = request.SequenceNumber;

        foreach (var pair in headers)
        {
            Headers.Add(pair.Key, pair.Value);
        }

        StatusCode = statusCode;
        ReasonPhrase = statusCode == 200 ? "OK" : string.Empty;
    }

    public string Format()
    {
        var rtspResponse = this;

        var result = $"{rtspResponse.Protocol} {StatusCode} {ReasonPhrase}\r\n" +
            $"CSeq: {rtspResponse.SequenceNumber}\r\n";

        if (rtspResponse.Headers.Any())
        {
            foreach (var pair in rtspResponse.Headers)
            {
                result += $"{pair.Key}: {pair.Value}\r\n";   
            }
        }

        result += "\r\n";
        
        if (!string.IsNullOrWhiteSpace(rtspResponse.Body))
        {
            result += $"{rtspResponse.Body}";
        }
        
        return result;
    }
}