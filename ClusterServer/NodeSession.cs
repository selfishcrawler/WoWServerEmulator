﻿using System.Net.Sockets;
using Game.Network.Clustering;

namespace ClusterServer;

public class NodeSession
{
    private readonly TcpClient _tcpClient;
    private readonly NetworkStream _stream;

    public NodeSession(TcpClient client)
    {
        _tcpClient = client;
        _stream = client.GetStream();
        _ = WaitCommands();
    }

    public async Task<int> GetNodeID()
    {
        byte[] nodeId = new byte[4];
        await _stream.ReadAsync(nodeId);
        return BitConverter.ToInt32(nodeId);
    }

    public async Task WaitCommands()
    {
    }

    public void SendCommand(ClusterPacket pkt)
    {
        try
        {
            pkt.Send(_stream);
        }
        catch
        {
            //node server is crashed, restart here maybe?
        }
    }
}