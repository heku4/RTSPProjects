using System.Net;
using System.Net.Sockets;
using MockCamera.Models;

const int portNumber = 9898;
var tokenSource = new CancellationTokenSource();
var tcpListener = new TcpListener(new IPAddress(new byte[]{0,0,0,0}), portNumber);
tcpListener.Start();

Console.Clear();
Console.ForegroundColor = ConsoleColor.Green;

Console.WriteLine("App started");
Console.WriteLine($"Main port binded on: {portNumber}");

Console.ForegroundColor = ConsoleColor.DarkCyan;

while (!tokenSource.IsCancellationRequested)
{
    var client = await tcpListener.AcceptTcpClientAsync();
    var session = new Session(client);
    await Task.Run(() => session.StartSession(tokenSource.Token), tokenSource.Token);
}