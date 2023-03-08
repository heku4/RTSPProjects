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

        var rows = rawRequestData.Split("\r\n");
        
        // rows[0] - METHOD url Protocol
        var mainRequestData = rows[0].Split(" ");
        if (mainRequestData.Length != 3)
        {
            throw new Exception($"Main request data is not valid: '{rows[0]}'");
        }
        
        var methodParsed = Enum.TryParse<RtspMethod>(rows[0], out var method);
        if (!methodParsed)
        {
            // exception
        }

        Method = method;
        Protocol = mainRequestData[^1];
        Url = mainRequestData[1];

        // last el == \r\n. First el is parsed already
        foreach (var row in rows.Skip(1).Take(rows.Length - 2))
        {
            var header = row.Split(":").Select(r => r.Trim()).ToArray();
            if (header.Length != 2)
            {
                // exception
            }
            
            if (header[0] == "CSeq")
            {
                var seqParsed = int.TryParse(header[1], out var number);
                if (!seqParsed)
                {
                    //exception
                }
                
                SequenceNumber = number;
                continue;
            }
            
            Headers.Add(header[0], header[1]);
        }
    }
    
}