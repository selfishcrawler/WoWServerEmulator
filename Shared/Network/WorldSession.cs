using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

using Shared.Database;
using Shared.Extensions;
using Shared.Network.PacketStructs;

namespace Shared.Network;

public class WorldSession
{
    private const int DefaultBufferSize = 1000;
    private const int ClientHeaderSize = 6;
    private readonly CancellationToken _serverCancellationToken;
    private CancellationTokenSource _cts;
    private readonly WorldAcceptor _acceptor;
    private readonly ILoginDatabase _loginDatabase;

    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly MemoryStream _smsg;
    private readonly byte[] _cmsg;

    private readonly byte[] _seed;
    private byte[] _sessionKey;
    private string _username;

    public WorldSession(TcpClient client, WorldAcceptor acceptor)
    {
        _client = client;
        _stream = client.GetStream();
        _smsg = new MemoryStream(DefaultBufferSize);
        _cmsg = new byte[DefaultBufferSize];
        _seed = RandomNumberGenerator.GetBytes(4);
        _acceptor = acceptor;
        _serverCancellationToken = acceptor.CancellationToken;
        _loginDatabase = acceptor.LoginDatabase;
    }

    public async Task InitConnection()
    {
        SendAuthChallenge();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(_serverCancellationToken);
        int bytesRead = await _stream.ReadAsync(_cmsg, 0, ClientHeaderSize, _cts.Token);
        if (bytesRead == 0)
            Disconnect();
        ProcessAuthSession();
    }

    private unsafe void ProcessAuthSession()
    {
        var header = new ClientPacketHeader(BitConverter.ToUInt16(_cmsg, 0), (Opcode)BitConverter.ToUInt32(_cmsg, 2));
        if (header.Opcode != Opcode.CMSG_AUTH_SESSION)
        {
            Log.Error("Wrong opcode");
            Log.Warning($"Header size: {header.Length}");
            Log.Warning($"Client available: {_client.Available}");
            Disconnect();
            return;
        }

        ReceiveFullPacket(header.Length);

        switch (header.Opcode)
        {
            case Opcode.CMSG_AUTH_SESSION:
                int index = Array.IndexOf<byte>(_cmsg, 0, 8) + 1;
                CMSG_AUTH_SESSION pkt;
                fixed (byte* ptr = &_cmsg[0])
                {
                    pkt = *(CMSG_AUTH_SESSION*)ptr;
                    _username = Encoding.UTF8.GetString(_cmsg, 8, index - 9);
                    pkt.LoginServerType = *(uint*)(ptr + index);
                    pkt.Seed = *(uint*)(ptr + index + 4);
                    pkt.RegionID = *(uint*)(ptr + index + 8);
                    pkt.BattlegroupID = *(uint*)(ptr + index + 12);
                    pkt.RealmID = *(uint*)(ptr + index + 16);
                    pkt.DOS = *(uint*)(ptr + index + 20);
                    pkt.AddonData = (ptr + index + 28);
                }
                _sessionKey = _loginDatabase.ExecuteSingleValue<byte[]>(_loginDatabase.GetSessionKey, new KeyValuePair<string, object>[]
                {
                    new ("@Name", _username),
                });
                Log.Warning(_username);
                Log.Warning(pkt.RealmID);
                break;
        }
    }

    private void SendAuthChallenge()
    {
        var header = new ServerPacketHeader(40, Opcode.SMSG_AUTH_CHALLENGE);
        _smsg.Write(header);
        _smsg.Write((uint)1);
        _smsg.Write(_seed);
        _smsg.Write(RandomNumberGenerator.GetBytes(32));
        _stream.PushPacket(_smsg);
        Log.Message("SMSG_AUTH_CHALLENGE sent");
    }

    private void ReceiveFullPacket(uint length)
    {
        if (_client.Available == length)
        {
            _stream.Read(_cmsg, 0, (int)length);
            return;
        }
        int bytesRead = 0;
        while (bytesRead < length)
        {
            bytesRead += _stream.Read(_cmsg, bytesRead, (int)length - bytesRead);
        }
    }

    public void Disconnect()
    {
        Log.Warning("Disconnecting");
        _acceptor.RemoveSession(this);
        _cts.Cancel();
        _stream.Close();
        _client.Close();
    }
}