using System.Net;
using System.Net.Sockets;
using System.Text;
using MockCamera.Models;

var tcpListener = new TcpListener(new IPAddress(new byte[]{0,0,0,0}), 9898);
tcpListener.Start();
while (true)
{
    var client = await tcpListener.AcceptTcpClientAsync();
    var session = new Session(client);
    await Task.Run(() => session.StartSession());
}