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

        Body = bodyData;
        
        StatusCode = statusCode;
        ReasonPhrase = statusCode == 200 ? "OK" : string.Empty;
    }

    public string Format()
    {
        var rtspResponse = this;

        var result = $"{rtspResponse.Protocol} {StatusCode} {ReasonPhrase}{Environment.NewLine}" +
            $"CSeq: {rtspResponse.SequenceNumber}{Environment.NewLine}";

        if (rtspResponse.Headers.Any())
        {
            foreach (var pair in rtspResponse.Headers)
            {
                result += $"{pair.Key}: {pair.Value}{Environment.NewLine}";   
            }
        }
        
        result += $"{Environment.NewLine}";

        if (!string.IsNullOrWhiteSpace(rtspResponse.Body))
        {
            result += $"{rtspResponse.Body}{Environment.NewLine}{Environment.NewLine}";
        }
        
        return result;
    }
}