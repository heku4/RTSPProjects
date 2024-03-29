﻿using System.Net;
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
Console.WriteLine($"Main port binded on: {mainPortNumber}");
Console.WriteLine($"UDP port for RTP binded on: {udpPortNumber1}");
Console.WriteLine($"UDP port for RTCP binded on: {udpPortNumber2}");

Console.ForegroundColor = ConsoleColor.DarkCyan;

while (!tokenSource.IsCancellationRequested)
    try
    {
        var client = await tcpListener.AcceptTcpClientAsync();
        var session = new Session(client, udpPortNumber1, udpPortNumber2);

        Console.WriteLine(
            $"{Environment.NewLine}Session {session.GetSessionId()} started at: {DateTime.Now}{Environment.NewLine}");

        await Task.Run(() => session.StartSession(tokenSource), tokenSource.Token);
    }
    catch (Exception e)
    {
        Console.Error.WriteLine(e);
    }