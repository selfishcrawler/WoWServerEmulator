using System.Net;
using System.Net.Sockets;

namespace Game.Network.Clustering;

public class NodeManager : INodeManager
{
    private readonly TcpClient _nodeSession;
    private readonly NetworkStream _stream;
    private readonly List<WorldSession> _sessions;
    private readonly Dictionary<int, uint> _pendingWorldEnter;

    public NodeManager(int nodeId, IPAddress address, int port)
    {
        _sessions = new List<WorldSession>();
        _pendingWorldEnter = new Dictionary<int, uint>();
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
        Log.Warning("Connected to cluster");
        byte[] buf = new byte[12];
        while (true)
        {
            await _stream.ReadAsync(buf, 0, 4);
            int accId = BitConverter.ToInt32(buf);
            await _stream.ReadAsync(buf, 0, 8);
            ulong charId = BitConverter.ToUInt32(buf);

            var session = _sessions.FirstOrDefault(x => x.AccountID == accId, null);
            if (session is null)
                _pendingWorldEnter.Add(accId, (uint)charId);
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
        session.SendRedirect(IPAddress.Loopback, 8085);
    }

    public void RedirectionFailed(WorldSession session)
    {
    }
}