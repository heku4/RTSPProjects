using System.Net;
using System.Net.Sockets;
using MockCamera.Models;

const int portNumber = 9898;
var tokenSource = new CancellationTokenSource();
var tcpListener = new TcpListener(new IPAddress(new byte[]{0,0,0,0}), portNumber);
tcpListener.Start();

Console.WriteLine("App started");
Console.WriteLine($"Main port binded on: {portNumber}");

while (!tokenSource.IsCancellationRequested)
{
    var client = await tcpListener.AcceptTcpClientAsync();
    var session = new Session(client);
    await Task.Run(() => session.StartSession(tokenSource.Token));
}

Console.WriteLine("App main loop ended. Good bye!");
