namespace MockCamera.Models;

public class RtspResponse : RtspMessage
{
    public int StatusCode { get; init; }
    public string ReasonPhrase { get; init; }
    public string? Body { get; init; }

    public RtspResponse(RtspRequest request, Dictionary<string, string>? headers, string? bodyData, int statusCode)
    {
        Protocol = request.Protocol;
        Method = request.Method;
        SequenceNumber = request.SequenceNumber;

        if (headers is not null && headers.Any())
        {
            foreach (var pair in headers)
            {
                Headers.Add(pair.Key, pair.Value);
            }
        }

        Body = bodyData;

        StatusCode = statusCode;

        ReasonPhrase = statusCode switch
        {
            200 => "OK",
            400 => "Bad Request",
            404 => "Not Found",
            _ => "Not Found"
        };
    }

    public string Format()
    {
        var rtspResponse = this;

        var result = $"{rtspResponse.Protocol} {StatusCode} {ReasonPhrase}\r\n" +
                     $"CSeq: {rtspResponse.SequenceNumber}\r\n";

        foreach (var pair in rtspResponse.Headers)
        {
            result += $"{pair.Key}: {pair.Value}\r\n";
        }

        if (!string.IsNullOrWhiteSpace(rtspResponse.Body))
        {
            result += $"Content-Length: {rtspResponse.Body.Length + 4}\r\n\r\n";
            result += $"{rtspResponse.Body}\r\n";
        }

        result += "\r\n";

        return result;
    }
}