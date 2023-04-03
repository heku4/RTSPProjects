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
        ReasonPhrase = statusCode == 200 ? "OK" : "Bad Request";
    }

    public string Format()
    {
        var rtspResponse = this;

        var result = $"{rtspResponse.Protocol} {StatusCode} {ReasonPhrase}\n" +
            $"CSeq: {rtspResponse.SequenceNumber}\n";
        if (rtspResponse.Headers.Any())
        {
            foreach (var pair in rtspResponse.Headers)
            {
                result += $"{pair.Key}: {pair.Value}\n";   
            }
        }

        if (!string.IsNullOrWhiteSpace(rtspResponse.Body))
        {
            result += $"Content-Length: {rtspResponse.Body.Length + 4}\n\n";
            result += $"{rtspResponse.Body}\n";
        }
        
        result += "\n";

        return result;
    }
}