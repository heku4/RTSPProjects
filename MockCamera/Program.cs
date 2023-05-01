using System.Net;
using System.Net.Sockets;
using MockCamera.Models;
using MockCamera.Services;

const int mainPortNumber = 9898;
const int udpPortNumber1 = 10897;
const int udpPortNumber2 = 10898;
var tokenSource = new CancellationTokenSource();

var tcpListener = new TcpListener(new IPAddress(new byte[] { 0, 0, 0, 0 }), mainPortNumber);
tcpListener.Start();

Console.Clear();
Console.ForegroundColor = ConsoleColor.Green;

Console.WriteLine("App started");
Console.WriteLine($"Main port bound on: {mainPortNumber}");
Console.WriteLine($"UDP port for RTP bound on: {udpPortNumber1}");
Console.WriteLine($"UDP port for RTCP bound on: {udpPortNumber2}");

Console.ForegroundColor = ConsoleColor.DarkCyan;

var queueHandler = new PlayQueueHandler(1);
var packetSender = new UdpPacketSender(queueHandler);

Thread StartTheThread(CancellationToken param1) {
    var t = new Thread(() => packetSender.SendRtpPacket(param1));
    t.Start();
    return t;
}

StartTheThread(tokenSource.Token);

while (!tokenSource.IsCancellationRequested)
    try
    {
        var client = await tcpListener.AcceptTcpClientAsync();
        var session = new Session(client, mainPortNumber, udpPortNumber1, udpPortNumber2, queueHandler);

        Console.WriteLine(
            $"{Environment.NewLine}Session {session.GetSessionId()} started at: {DateTime.Now}{Environment.NewLine}");

        await Task.Run(() => session.HandSession(new CancellationTokenSource()), tokenSource.Token);
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e);
    }
    