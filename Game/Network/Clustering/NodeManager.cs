using Shared.Database;
using System.Net;
using System.Net.Sockets;

namespace Game.Network.Clustering;

public class NodeManager : INodeManager
{
    private readonly TcpClient _nodeSession;
    private readonly NetworkStream _stream;
    private readonly List<WorldSession> _sessions;
    private readonly Dictionary<int, uint> _pendingWorldEnter;
    private readonly Dictionary<int, int> _nodeMappings;
    private readonly Dictionary<int, IPEndPoint> _nodeAddresses;

    public NodeManager(int nodeId, IPAddress address, int port)
    {
        _sessions = new List<WorldSession>();
        _pendingWorldEnter = new Dictionary<int, uint>();
        _nodeMappings = new Dictionary<int, int>();
        _nodeAddresses = new Dictionary<int, IPEndPoint>();

        foreach (var mapping in Database.Cluster.ExecuteMultipleRaws(Database.Cluster.GetNodeMappings, null))
            _nodeMappings[(int)mapping[0]] = (int)mapping[1];

        foreach (var endpoint in Database.Cluster.ExecuteMultipleRaws(Database.Cluster.GetNodeEndpointsForRedirection, null))
            _nodeAddresses[(int)endpoint[0]] = new IPEndPoint(IPAddress.Parse(endpoint[1].ToString()), (int)endpoint[2]);

        _nodeSession = new TcpClient();
        try
        {
            _nodeSession.Connect(address, port);
            _stream = _nodeSession.GetStream();
            _stream.Write(BitConverter.GetBytes(nodeId));
            _ = ReceiveCommands();
        }
        catch
        {
            Log.Error("Cannot connect to cluster");
            throw;
        }
    }

    private async Task ReceiveCommands()
    {
        while (true)
        {
            ClusterPacket pkt = await ClusterPacket.ReceiveAsync(_stream);
            switch (pkt)
            {
                case EnterWorldPacket ewp:
                    var session = _sessions.FirstOrDefault(x => x.AccountID == ewp.AccountId, null);
                    if (session is null)
                        _pendingWorldEnter.Add(ewp.AccountId, ewp.CharacterId);
                    else
                        session.LoginAsCharacter(ewp.CharacterId);
                    break;
                default:
                    break;
            }
        }
    }

    public void AddSession(WorldSession session)
    {
        _sessions.Add(session);
        if (_pendingWorldEnter.ContainsKey(session.AccountID))
        {
            session.LoginAsCharacter(_pendingWorldEnter[session.AccountID]);
            _pendingWorldEnter.Remove(session.AccountID);
        }
    }

    public void RemoveSession(WorldSession session)
    {
        _sessions.Remove(session);
    }

    public void EnterWorld(WorldSession session, int map, ulong characterId)
    {
        
    }

    public void Logout(WorldSession session)
    {
        var clusterIPep = _nodeAddresses[0];
        session.SendRedirect(clusterIPep.Address, (ushort)clusterIPep.Port);
    }

    public void RedirectionFailed(WorldSession session)
    {
    }
}