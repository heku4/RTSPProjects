using System.Net;
using System.Net.Sockets;
using MockCamera.Models;

namespace MockCamera.Services;

public class UdpPacketSender
{
    private async Task SendRtpPacket(Dictionary<int, int> sessions, CancellationToken stoppingToken)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);

        var serverAddr = new IPAddress(new byte[] { 0, 0, 0, 0 });
        //var endPoint = new IPEndPoint(serverAddr, _clientRtpPort);

        uint packNumber = 0;
        uint timeStamp = 0;
        var changeFlag = false;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                timeStamp += 3600;

                foreach (var pair in sessions)
                {
                    
                    var endPoint = new IPEndPoint(serverAddr, pair.Key);
                    var dataToSend = RtpPacket.Prepare(timeStamp, packNumber, changeFlag);
                    await socket.SendToAsync(dataToSend, endPoint);

                    changeFlag = !changeFlag;
                    packNumber++;
                }

                
                await Task.Delay(40, stoppingToken);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
    }
}