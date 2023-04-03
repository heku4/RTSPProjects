using System.Net;
using System.Net.Sockets;
using MockCamera.Models;

const int mainPortNumber = 9898;
const int udpNumber1 = 10897;
const int udpNumber2 = 10898;
var tokenSource = new CancellationTokenSource();
var tcpListener = new TcpListener(new IPAddress(new byte[]{0,0,0,0}), mainPortNumber);
tcpListener.Start();

Console.Clear();
Console.ForegroundColor = ConsoleColor.Green;

Console.WriteLine("App started");
Console.WriteLine($"Main port binded on: {mainPortNumber}");
Console.WriteLine($"UDP port #1 binded on: {udpNumber1}");
Console.WriteLine($"UDP port #2 binded on: {udpNumber2}");

Console.ForegroundColor = ConsoleColor.DarkCyan;

while (!tokenSource.IsCancellationRequested)
{
    var client = await tcpListener.AcceptTcpClientAsync();
    var session = new Session(client, udpNumber1, udpNumber2);
    await Task.Run(() => session.StartSession(tokenSource), tokenSource.Token);
}