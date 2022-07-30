using System.Net;
using System.Net.Sockets;

namespace Game.Network.Clustering;

public class NodeManager : INodeManager
{
    private readonly TcpClient _nodeSession;
    private readonly NetworkStream _stream;
    private readonly List<WorldSession> _sessions;

    public NodeManager(int nodeId, IPAddress address, int port)
    {
        _sessions = new List<WorldSession>();
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
            Thread.Sleep(2000);
            _sessions.First(x => x.AccountID == accId).LoginAsCharacter((uint)charId);
        }
    }

    public void AddSession(WorldSession session)
    {
        _sessions.Add(session);
    }

    public void EnterWorld(WorldSession session, int map, ulong characterId)
    {
        
    }
}