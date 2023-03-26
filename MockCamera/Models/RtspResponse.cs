namespace MockCamera.Models;

public class RtspResponse: RtspMessage
{
    public int StatusCode { get; set; }
    public string ReasonPhrase { get; set; }

    public RtspResponse()
    {
        
    }
}