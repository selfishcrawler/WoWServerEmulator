using System.Net;
using System.Net.Sockets;
using Game.Network;
using Game.Network.Clustering;
using Shared.Database;

namespace ClusterServer;

public class ClusterManager : INodeManager
{
    private readonly TcpListener _listener;
    private readonly Dictionary<int, NodeSession> _nodeSessions;
    private readonly Dictionary<int, int> _nodeMappings;
    private readonly Dictionary<int, IPEndPoint> _nodeAddresses;

    public ClusterManager(IPAddress ip, int port)
    {
        _nodeSessions = new Dictionary<int, NodeSession>();
        _nodeMappings = new Dictionary<int, int>();
        _nodeAddresses = new Dictionary<int, IPEndPoint>();

        foreach (var mapping in Database.Cluster.ExecuteMultipleRaws(Database.Cluster.GetNodeMappings, null))
            _nodeMappings[(int)mapping[0]] = (int)mapping[1];

        foreach (var mapping in Database.Cluster.ExecuteMultipleRaws(Database.Cluster.GetNodeEndpoints, null))
            _nodeAddresses[(int)mapping[0]] = new IPEndPoint(IPAddress.Parse(mapping[1].ToString()), (int)mapping[2]);

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

    public void RemoveSession(WorldSession session)
    {
    }

    public void EnterWorld(WorldSession session, int map, ulong characterId)
    {
        var nodeId = _nodeMappings[map];
        var nodeAddress = _nodeAddresses[nodeId];
        session.SendRedirect(nodeAddress.Address, (ushort)nodeAddress.Port);
        EnterWorldPacket pkt = new() { AccountId = session.AccountID, CharacterId = (uint)characterId };
        _nodeSessions[nodeId].SendCommand(pkt);
    }

    public void Logout(WorldSession session)
    {
    }

    public void RedirectionFailed(WorldSession session)
    {
        session.SendLoginError(LoginError.ServerUnavailable);
    }
}