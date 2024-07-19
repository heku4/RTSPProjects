using System.Net;
using System.Net.Sockets;
using MockCamera.Models;

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

while (!tokenSource.IsCancellationRequested)
    try
    {
        var client = await tcpListener.AcceptTcpClientAsync();
        var session = new Session(client, udpPortNumber1, udpPortNumber2);

        Console.WriteLine($"Session {session.GetSessionId()} started at: {DateTime.Now}");

        var thread = new Thread(() =>
        {
            Task.Run(() => session.StartSession(tokenSource), tokenSource.Token);
        });

        thread.Start();
        thread.Join();
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e);
    }