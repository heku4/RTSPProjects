using System.Net;
using System.Net.Sockets;
using MockCamera.Models;

namespace MockCamera.Services;

public class UdpPacketSender
{
    private readonly PlayQueueHandler _playQueueHandler;
    private readonly Dictionary<ulong, PacketMetrics> _sessions;

    public UdpPacketSender(PlayQueueHandler playQueueHandler)
    {
        _playQueueHandler = playQueueHandler;
        _sessions = new Dictionary<ulong, PacketMetrics>();
    }

    public void SendRtpPacket(CancellationToken stoppingToken)
    {
        var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
            ProtocolType.Udp);

        var serverAddr = new IPAddress(new byte[] { 127, 0, 0, 1 });

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (_playQueueHandler.TryPeekSessionToTeardown(out var sessionToTeardown))
                {
                    if (!_sessions.TryGetValue(sessionToTeardown, out var sessionValue))
                    {
                        continue;
                    }
                    sessionValue.Socket.Close();
                    
                    _sessions.Remove(sessionToTeardown);
                }

                if (_playQueueHandler.TryPeekNewSession(out var sessionData))
                {
                    if (sessionData.SessionId == sessionToTeardown)
                    {
                        continue;
                    }
                    var endPoint = new IPEndPoint(serverAddr, sessionData.Port);
                    socket.Connect(endPoint);

                    _sessions.Add(sessionData.SessionId, new PacketMetrics
                    {
                        TimeStamp = 0,
                        PacketNumber = 0,
                        SessionId = sessionData.SessionId,
                        Port = sessionData.Port,
                        ChannelFlag = false,
                        Socket = socket
                    });
                }

                foreach (var session in _sessions)
                {
                    Console.WriteLine(session.Key);
                    var dataToSend = RtpPacket.Prepare(session.Value.TimeStamp,  session.Value.PacketNumber, session.Value.ChannelFlag);
                    session.Value.Socket.Send(dataToSend, SocketFlags.None);

                    var metrics = new PacketMetrics
                    {
                        TimeStamp = session.Value.TimeStamp + session.Value.TimeStamp,
                        PacketNumber = session.Value.PacketNumber + session.Value.PacketNumber,
                        SessionId = session.Value.SessionId,
                        Port = session.Value.Port,
                        ChannelFlag = !session.Value.ChannelFlag,
                        Socket = socket
                    };
                    
                    _sessions[session.Value.SessionId] = metrics;
                }

                
                Task.Delay(40, stoppingToken);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                throw;
            }
        }
    }
}

public struct PacketMetrics
{
    public uint TimeStamp;
    public uint PacketNumber;
    public ulong SessionId;
    public int Port;
    public bool ChannelFlag;
    public Socket Socket;
}