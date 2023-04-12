using System.Net;
using System.Net.Sockets;
using MockCamera.Models;

const int mainPortNumber = 9898;
const int udpPortNumber1 = 10897;
const int udpPortNumber2 = 10898;
var tokenSource = new CancellationTokenSource();
var tcpListener = new TcpListener(new IPAddress(new byte[]{0,0,0,0}), mainPortNumber);
tcpListener.Start();
//var udpWriter1 = new UdpClient(I)
Console.Clear();
Console.ForegroundColor = ConsoleColor.Green;

Console.WriteLine("App started");
Console.WriteLine($"Main port binded on: {mainPortNumber}");
Console.WriteLine($"UDP port #1 binded on: {udpPortNumber1}");
Console.WriteLine($"UDP port #2 binded on: {udpPortNumber2}");

Console.ForegroundColor = ConsoleColor.DarkCyan;

while (!tokenSource.IsCancellationRequested)
{
    var client = await tcpListener.AcceptTcpClientAsync();
    var session = new Session(client, udpPortNumber1, udpPortNumber2);
    await Task.Run(() => session.StartSession(tokenSource), tokenSource.Token);
}