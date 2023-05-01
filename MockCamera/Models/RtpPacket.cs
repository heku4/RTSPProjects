namespace MockCamera.Models;

public static class RtpPacket
{
    private const byte VersionRtpPacket = 0x80;
    private const byte PayloadTypeRtpPacket = 0x9a;
    private const byte SsrcRtpPacket1 = 0x13;
    private const byte SsrcRtpPacket2 = 0xf9;
    private const byte SsrcRtpPacket3 = 0x7e;
    private const byte SsrcRtpPacket4 = 0x67;

    private const byte PayloadTypeSpecific = 0;
    private const byte PayloadFragmentOffset = 0;
    private const byte PayloadQData = 1;
    private const byte PayloadType = 0x5e;
    private const byte PayloadWidth = 6;
    private const byte PayloadHeight = 4;

    private static readonly byte[] SampleChannelA =
    {
        0xf8, 0xbe, 0x8a, 0x28, 0xaf, 0xe5, 0x33, 0xfd,
        0xfc, 0x0a, 0x28, 0xa2, 0x80, 0x0a, 0x28, 0xa2,
        0x80, 0x0a, 0x28, 0xa2, 0x80, 0x0a, 0x28, 0xa2,
        0x80, 0x0a, 0x28, 0xa2, 0x80, 0x3f, 0xff, 0xd9
    };

    private static readonly byte[] SampleChannelB =
    {
        0xf5, 0x8a, 0x28, 0xa2, 0xbf, 0xca, 0xf3, 0xfc,
        0x53, 0x0a, 0x28, 0xa2, 0x80, 0x0a, 0x28, 0xa2,
        0x80, 0x0a, 0x28, 0xa2, 0x80, 0x0a, 0x28, 0xa2,
        0x80, 0x0a, 0x28, 0xa2, 0x80, 0x3f, 0xff, 0xd9
    };

    public static byte[] Prepare(uint timeStamp, uint packetSequenceNumber, bool chanelFlag)
    {
        var rtpDataBuffer = new byte[20];
        // RTP headers https://www.rfc-editor.org/rfc/rfc3550.html
        // version (V): 2 + bits padding (P): 1 bit + extension (X): 1 bit + CSRC count (CC): 4 bits
        rtpDataBuffer[0] = VersionRtpPacket;

        // payload type (PT): 7 bits + marker (M): 1 bit
        rtpDataBuffer[1] = PayloadTypeRtpPacket;

        // sequence number: 2 bytes should be randomized for safety
        var sNumberInBytes = BitConverter.GetBytes(packetSequenceNumber);
        
        if (BitConverter.IsLittleEndian)
        {
            sNumberInBytes = sNumberInBytes.Reverse().ToArray();
        }
        rtpDataBuffer[2] = sNumberInBytes[2];
        rtpDataBuffer[3] = sNumberInBytes[3];

        //  timestamp: 4 bytes
        var tsInBytes = BitConverter.GetBytes(timeStamp);

        if (BitConverter.IsLittleEndian)
        {
            tsInBytes = tsInBytes.Reverse().ToArray();
        }
        
        rtpDataBuffer[4] = tsInBytes[0];
        rtpDataBuffer[5] = tsInBytes[1];
        rtpDataBuffer[6] = tsInBytes[2];
        rtpDataBuffer[7] = tsInBytes[3];
        
        // SSRC - 4 bytes
        rtpDataBuffer[8] = SsrcRtpPacket1;
        rtpDataBuffer[9] = SsrcRtpPacket2;
        rtpDataBuffer[10] = SsrcRtpPacket3;
        rtpDataBuffer[11] = SsrcRtpPacket4;

        // https://datatracker.ietf.org/doc/html/rfc2435 // https://datatracker.ietf.org/doc/html/rfc2035
        // RTP Payload Format for JPEG-compressed Video
        // TypeSpecific - 1 byte
        rtpDataBuffer[12] = PayloadTypeSpecific;
        // FragmentOffset - 3 bytes
        rtpDataBuffer[13] = PayloadFragmentOffset;
        rtpDataBuffer[14] = PayloadFragmentOffset;
        rtpDataBuffer[15] = PayloadFragmentOffset;
        // Q - 1 byte
        rtpDataBuffer[16] = PayloadQData;
        // Type - 1 byte
        rtpDataBuffer[17] = PayloadType;
        // Width - 1 byte
        rtpDataBuffer[18] = PayloadWidth; // 48
        // Height - 1 byte
        rtpDataBuffer[19] = PayloadHeight; // 32 = 4 x 8
        // the rest is jpeg payload

        // RGB JPEG images as RTP payload - 48x32 pixel.

        if (chanelFlag)
        {
            var resultArr = new byte[rtpDataBuffer.Length + SampleChannelA.Length];
            Buffer.BlockCopy(rtpDataBuffer, 0, resultArr, 0, rtpDataBuffer.Length);
            Buffer.BlockCopy(SampleChannelA, 0, resultArr, rtpDataBuffer.Length, SampleChannelA.Length);
            return resultArr;
        }
        else
        {
            var resultArr = new byte[rtpDataBuffer.Length + SampleChannelB.Length];
            Buffer.BlockCopy(rtpDataBuffer, 0, resultArr, 0, rtpDataBuffer.Length);
            Buffer.BlockCopy(SampleChannelB, 0, resultArr, rtpDataBuffer.Length, SampleChannelB.Length);
            return resultArr;
        }
    }
}