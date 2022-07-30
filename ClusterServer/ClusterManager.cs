using System.Net;
using System.Net.Sockets;
using Game.Network;
using Game.Network.Clustering;

namespace ClusterServer;

public class ClusterManager : INodeManager
{
    private readonly TcpListener _listener;
    private readonly Dictionary<int, NodeSession> _nodeSessions;

    public ClusterManager(IPAddress ip, int port)
    {
        _nodeSessions = new Dictionary<int, NodeSession>();
        _listener = new TcpListener(ip, port);
        try
        {
            _listener.Start();
            _ = WaitNodes();
        }
        catch
        {
            Log.Error("Cannot start cluster listener");
            throw;
        }
    }

    private async Task WaitNodes()
    {
        while (true)
        {
            var tcpclient = await _listener.AcceptTcpClientAsync();
            var nodeSession = new NodeSession(tcpclient);
            int nodeId = await nodeSession.GetNodeID();
            Log.Warning($"Node ID:{nodeId} joined");
            _nodeSessions[nodeId] = nodeSession;
        }
    }

    public void AddSession(WorldSession session)
    {
    }

    public void EnterWorld(WorldSession session, int map, ulong characterId)
    {
        int nodeId = 1; //detect this by map->node mapping
        if (map == 0)
        {
            session.SendRedirect(IPAddress.Loopback, 8086);
        }
        _nodeSessions[nodeId].SendCommand(session.AccountID, characterId);
    }
}