namespace MockCamera.Models;

public class RtspRequest
{
    public string Protocol { get; init; }
    public string Url { get; init; }
    public  int SequenceNumber { get; init; }
    public RtspMethod Method { get; init; }
    public Dictionary<string, string> Headers { get; init; } = new Dictionary<string, string>();

    public RtspRequest(string rawRequestData)
    {
        // divide by rows
        if (string.IsNullOrWhiteSpace(rawRequestData))
        {
            throw new ArgumentException("Incoming request raw data is null or whitespace");
        }

        var startIndex = rawRequestData.IndexOf("\r\n\r\n", StringComparison.Ordinal);
        var rows = rawRequestData.Remove(startIndex).Split("\r\n");
        
        // rows[0] - METHOD url Protocol
        var mainRequestData = rows[0].Split(" ");
        if (mainRequestData.Length != 3)
        {
            throw new Exception($"Main request data is not valid: '{rows[0]}'");
        }
        
        var methodParsed = Enum.TryParse<RtspMethod>(mainRequestData[0], out var method);
        if (!methodParsed)
        {
            throw new Exception($"Can't parse request method: '{rows[0] ?? string.Empty}'");
        }

        Method = method;
        Protocol = mainRequestData[^1];
        Url = mainRequestData[1];
        
        foreach (var row in rows.Skip(1))
        {
            var header = row.Split(":", 2).Select(r => r.Trim()).ToArray();
            if (header.Length != 2)
            {
                throw new Exception($"Can't parse request header: '{row}'");
                // continue;
            }
            
            if (header[0] == "CSeq")
            {
                var seqParsed = int.TryParse(header[1], out var number);
                if (!seqParsed)
                {
                    throw new Exception($"Can't parse CSeq number: '{row}'");
                }
                
                SequenceNumber = number;
                continue;
            }
            
            Headers.Add(header[0], header[1]);
        }
    }
    
}