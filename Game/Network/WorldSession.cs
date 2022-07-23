using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

using Shared.Database;
using Game.Cryptography;
using Game.Entities;
using Game.Network.PacketStructs;
using Game.World;

namespace Game.Network;

using static Opcode;

public partial class WorldSession
{
    private delegate void PacketHandler(ClientPacketHeader header);

    private const int DefaultBufferSize = 1000;
    private const int ClientHeaderSize = 6;
    private CancellationTokenSource _cts;
    private readonly ILoginDatabase _loginDatabase;

    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly MemoryStream _smsg;
    private byte[] _cmsg;
    private ARC4 _encryptor;

    private readonly byte[] _seed;
    private byte[] _sessionKey;
    private int _accountId;
    private string _username;
    private uint _timeSyncSequenceIndex;
    private Timer _logoutTimer;
    public Player ActiveCharacter { get; private set; }

    public WorldSession(TcpClient client)
    {
        _client = client;
        _stream = client.GetStream();
        _smsg = new MemoryStream(DefaultBufferSize);
        _cmsg = new byte[DefaultBufferSize];
        _seed = RandomNumberGenerator.GetBytes(4);
        _loginDatabase = WorldManager.LoginDatabase;
        _timeSyncSequenceIndex = 0;
    }

    public async Task InitConnection(CancellationToken serverToken)
    {
        SendAuthChallenge();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(serverToken);
        int bytesRead = await _stream.ReadAsync(_cmsg, 0, ClientHeaderSize, _cts.Token);
        if (bytesRead == 0)
            Disconnect();
        ProcessAuthSession();
        await HandlePacketFlow();
    }

    public void SendRedirect(uint nodeID)
    {
        Span<byte> host = stackalloc byte[6];
        Span<byte> hash = stackalloc byte[20];
        IPAddress ip = IPAddress.Loopback;
        ip.TryWriteBytes(host, out _);
        BitConverter.GetBytes((ushort)8086).CopyTo(host[4..]);
        HMACSHA1.HashData(_sessionKey, host, hash);

        var header = new ServerPacketHeader(30, SMSG_REDIRECT_CLIENT);
        _encryptor.Encrypt(ref header);
        _smsg.Write(host);
        _smsg.Write((uint)0);
        _smsg.Write(hash);
        lock (_stream)
        {
            _stream.Write(header);
            _stream.PushPacket(_smsg);
        }
    }

    private async Task HandlePacketFlow()
    {
        while (!_cts.IsCancellationRequested)
        {
            int bytesRead = await _stream.ReadAsync(_cmsg, 0, ClientHeaderSize, _cts.Token);
            if (bytesRead == 0)
            {
                Disconnect();
                return;
            }
            FindHandler(_cmsg.AsSpan()[..6]);
        }

        void FindHandler(Span<byte> headerBytes)
        {
            var header = _encryptor.Decrypt(headerBytes);
            ReceiveFullPacket(header.Length);
            GetHandlerForPacket(header)(header);
        }
    }

    private unsafe void HandleCharCreate(ClientPacketHeader _)
    {
        var header = new ServerPacketHeader(1, SMSG_CHAR_CREATE);
        _encryptor.Encrypt(ref header);
        _smsg.Write(header);
        _smsg.Write(47); //result
        lock (_stream)
        {
            _stream.PushPacket(_smsg);
        }
    }

    private unsafe void HandleCharEnum(ClientPacketHeader _)
    {
        var charList = _loginDatabase.ExecuteMultipleRaws(_loginDatabase.GetCharacterList, new KeyValuePair<string, object>[]
        {
            new("@Account", _accountId),
            new("@Realm", 4),
        });

        _smsg.Write((byte)charList.Count);
        foreach (var character in charList)
        {
            _smsg.Write((ulong)(int)character[0]);
            _smsg.Write(character[1].ToString());
            _smsg.Write((byte)character[2]);
            _smsg.Write((byte)character[3]);
            _smsg.Write((byte)character[4]);
            _smsg.Write((byte)character[5]);
            _smsg.Write((byte)character[6]);
            _smsg.Write((byte)character[7]);
            _smsg.Write((byte)character[8]);
            _smsg.Write((byte)character[9]);
            int level = (int)character[10];
            _smsg.Write((byte)(level > 255 ? 255 : level));
            _smsg.Write((uint)(int)character[11]);
            _smsg.Write((uint)(int)character[12]);
            _smsg.Write((float)character[13]);
            _smsg.Write((float)character[14]);
            _smsg.Write((float)character[15]);



            _smsg.Write((uint)0); //guild id
            _smsg.Write((uint)0x02000000); // char flags declined names set
            _smsg.Write((uint)0); // at login flags 
            _smsg.Write(0); //first login
            _smsg.Write((uint)0); //pet display id
            _smsg.Write((uint)0); //pet level
            _smsg.Write((uint)0); //pet family
            for (int i = 0; i < 23; i++) //gear info
            {
                _smsg.Write((uint)0);
                _smsg.Write(0);
                _smsg.Write((uint)0);
            }
        }
        var header = new ServerPacketHeader((ushort)(_smsg.Position), SMSG_CHAR_ENUM);
        _encryptor.Encrypt(ref header);
        lock (_stream)
        {
            _stream.Write(header);
            _stream.PushPacket(_smsg);
        }
    }

    private unsafe void HandlePlayerLogin(ClientPacketHeader _)
    {
        //SendRedirect(0);
        CMSG_PLAYER_LOGIN pkt;
        fixed (byte* ptr = &_cmsg[0])
            pkt = *(CMSG_PLAYER_LOGIN*)ptr;

        ActiveCharacter = new Player((uint)pkt.Guid)
        {
            Alive = true,
            CurrentHealth = 100,
            MaxHealth = 200,
            Level = 80,
            Race = Race.Human,
            Class = Class.Warrior,
            Gender = Gender.Female,
            PowerType = PowerType.Happiness,
            DisplayID = 19724,
            NativeDisplayID = 19724,

            WalkSpeed = 4f,
            RunSpeed = 20f,
            BackwardsRunSpeed = 5f
        };

        var header = new ServerPacketHeader(20, SMSG_LOGIN_VERIFY_WORLD);
        _encryptor.Encrypt(ref header);
        _smsg.Write((uint)0);
        _smsg.Write(-8949.95F);
        _smsg.Write(-132.493F);
        _smsg.Write(83.5312F);
        _smsg.Write(0.0F);
        lock (_stream)
        {
            _stream.Write(header);
            _stream.PushPacket(_smsg);
        }

        header = new ServerPacketHeader(32, SMSG_TUTORIAL_FLAGS);
        _encryptor.Encrypt(ref header);
        for (int i = 0; i < 8; i++)
            _smsg.Write((uint)0xFF);
        lock (_stream)
        {
            _stream.Write(header);
            _stream.PushPacket(_smsg);
        }
        ActiveCharacter.SetPosition(-8949.95F, -132.493F, 83.5312F, 0f);
        ActiveCharacter.SetCurrentPower(PowerType.Happiness, 1000000);
        ActiveCharacter.SetMaxPower(PowerType.Happiness, 2000000);
        _smsg.Write((uint)1); // block count
        ActiveCharacter.BuildUpdatePacket(_smsg);

        header = new ServerPacketHeader((ushort)_smsg.Position, SMSG_UPDATE_OBJECT);
        _encryptor.Encrypt(ref header);
        lock (_stream)
        {
            _stream.Write(header);
            _stream.PushPacket(_smsg);
        }

        header = new ServerPacketHeader(4, SMSG_TIME_SYNC_REQ);
        _encryptor.Encrypt(ref header);
        _smsg.Write(_timeSyncSequenceIndex++);

        lock (_stream)
        {
            _stream.Write(header);
            _stream.PushPacket(_smsg);
        }

        header = new ServerPacketHeader(2, SMSG_FEATURE_SYSTEM_STATUS);
        _encryptor.Encrypt(ref header);
        _smsg.Write(2);
        _smsg.Write(0); //voice chat disable
        lock (_stream)
        {
            _stream.Write(header);
            _stream.PushPacket(_smsg);
        }
    }

    private void HandleLogoutRequest(ClientPacketHeader _)
    {
        bool instantLogout = true;
        var header = new ServerPacketHeader(5, SMSG_LOGOUT_RESPONSE);
        _encryptor.Encrypt(ref header);
        _smsg.Write(header);
        _smsg.Write((uint)0);
        _smsg.Write(instantLogout);
        lock (_stream)
        {
            _stream.PushPacket(_smsg);
        }
        _logoutTimer = new Timer((_) =>
        {
            var header = new ServerPacketHeader(0, SMSG_LOGOUT_COMPLETE);
            _encryptor.Encrypt(ref header);
            lock (_stream)
            {
                _stream.Write(header);
            }
        }, null, TimeSpan.FromSeconds(instantLogout ? 0 : 20), TimeSpan.Zero);
    }

    private void HandleNameQuery(ClientPacketHeader _)
    {
        //TODO: read guid then responce
        _smsg.Write(ActiveCharacter.PackedGuid); //packed guid
        _smsg.Write(0);
        _smsg.Write("Тестчарбд");
        _smsg.Write(0); // realm name
        _smsg.Write(ActiveCharacter.Race);
        _smsg.Write(ActiveCharacter.Gender);
        _smsg.Write(ActiveCharacter.Class);
        _smsg.Write(0);

        var header = new ServerPacketHeader((ushort)_smsg.Position, SMSG_NAME_QUERY_RESPONSE);
        _encryptor.Encrypt(ref header);
        lock (_stream)
        {
            _stream.Write(header);
            _stream.PushPacket(_smsg);
        }
    }

    private void HandleMovementPacket(ClientPacketHeader header)
    {

    }

    private unsafe void HandlePing(ClientPacketHeader _)
    {
        CMSG_PING pkt;
        var header = new ServerPacketHeader(4, SMSG_PONG);
        _encryptor.Encrypt(ref header);
        _smsg.Write(header);
        fixed (byte* ptr = &_cmsg[0])
            pkt = *(CMSG_PING*)ptr;
        _smsg.Write(pkt.Ping);

        lock (_stream)
        {
            _stream.PushPacket(_smsg);
        }
    }

    private void HandleSetActiveMover(ClientPacketHeader _)
    {
        ulong guid = BitConverter.ToUInt64(_cmsg);
        Log.Message($"Active mover: {guid}");
    }

    private unsafe void HandleRealmSplit(ClientPacketHeader _)
    {
        const uint RealmNormal = 0x0;
        //const uint RealmSplit = 0x1;
        //const uint RealmSplitPending = 0x2;
        CMSG_REALM_SPLIT pkt;
        fixed (byte* ptr = &_cmsg[0])
            pkt = *(CMSG_REALM_SPLIT*)ptr;

        byte[] split_date = Encoding.UTF8.GetBytes("01/01/01\0");
        var header = new ServerPacketHeader((ushort)(8 + split_date.Length), SMSG_REALM_SPLIT);
        _encryptor.Encrypt(ref header);
        _smsg.Write(header);
        _smsg.Write(pkt.Unk);
        _smsg.Write(RealmNormal);
        _smsg.Write(split_date);
        lock (_stream)
        {
            _stream.PushPacket(_smsg);
        }
    }

    private void HandleTimeSyncResponce(ClientPacketHeader _)
    {

    }

    private void HandleSetPlayerDeclinedNames(ClientPacketHeader _)
    {
        //receive guid here
        var header = new ServerPacketHeader(12, SMSG_SET_PLAYER_DECLINED_NAMES_RESULT);
        _encryptor.Encrypt(ref header);
        _stream.Write(header);
        _smsg.Write((uint)0); //result code 0/1
        _smsg.Write((ulong)2); //character guid
        lock (_stream)
        {
            _stream.PushPacket(_smsg);
        }
    }

    private void HandleReadyForAccountDataTimes(ClientPacketHeader header)
    {
        var sheader = new ServerPacketHeader(4 + 1 + 4 + 8 * 4, SMSG_ACCOUNT_DATA_TIMES);
        _encryptor.Encrypt(ref sheader);
        _smsg.Write(sheader);
        var time = (uint)DateTimeOffset.Now.ToUnixTimeSeconds();
        _smsg.Write(time);
        _smsg.Write(1);
        _smsg.Write((uint)0x15);
        for (int i = 0; i < 8; i++)
        {
            _smsg.Write((uint)0);
        }
        lock (_stream)
        {
            _stream.PushPacket(_smsg);
        }
    }

    private void UnhandledPacket(ClientPacketHeader header)
    {
        uint code = (uint)header.Opcode;
        Log.Warning($"Received unhandled packet: 0x{code:X3}");
    }

    private unsafe void ProcessAuthSession()
    {
        var header = new ClientPacketHeader(BitConverter.ToUInt16(_cmsg, 0), (Opcode)BitConverter.ToUInt32(_cmsg, 2));
        if (header.Opcode != CMSG_AUTH_SESSION &&
            header.Opcode != CMSG_REDIRECTION_AUTH_PROOF)
        {
            Log.Error("Wrong opcode");
            Disconnect();
            return;
        }

        ReceiveFullPacket(header.Length);
        int index;
        switch (header.Opcode)
        {
            case CMSG_AUTH_SESSION:
                index = Array.IndexOf<byte>(_cmsg, 0, 8) + 1;
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
                    Span<byte> proof = new(ptr + index + 28, 20);
                    pkt.AddonData = (ptr + index + 48);

                    byte[] login = Encoding.UTF8.GetBytes(_username);
                    Span<byte> concat = stackalloc byte[login.Length + 52];
                    login.CopyTo(concat);
                    (stackalloc byte[] { 0, 0, 0, 0 }).CopyTo(concat[(login.Length)..]);
                    BitConverter.GetBytes(pkt.Seed).CopyTo(concat[(login.Length + 4)..]);
                    _seed.CopyTo(concat[(login.Length + 8)..]);

                    (_accountId, _sessionKey) = _loginDatabase.ExecuteSingleRaw<int, byte[]>(_loginDatabase.GetAccountInfoByUsername, new KeyValuePair<string, object>[]
                    {
                        new ("@Name", _username),
                    });

                    if (_sessionKey is default(byte[]))
                    {
                        Disconnect();
                        return;
                    }

                    _sessionKey.CopyTo(concat[(login.Length + 12)..]);
                    if (!SHA1.HashData(concat).SequenceEqual(proof.ToArray()))
                    {
                        Disconnect();
                        return;
                    }
                }


                _encryptor = new ARC4(_sessionKey);
                var serverHeader = new ServerPacketHeader(11, SMSG_AUTH_RESPONSE);
                _encryptor.Encrypt(ref serverHeader);
                _smsg.Write(serverHeader);
                _smsg.Write(12); //todo: auth result enum
                _smsg.Write((uint)0);
                _smsg.Write(0);
                _smsg.Write((uint)0);
                _smsg.Write(2); //expansion
                _stream.PushPacket(_smsg);
                break;
            case CMSG_REDIRECTION_AUTH_PROOF:
                index = Array.IndexOf<byte>(_cmsg, 0, 0) + 1;
                _username = Encoding.UTF8.GetString(_cmsg, 0, index - 1);
                Span<byte> hash = _cmsg.AsSpan()[(index + 8)..(index+28)];

                (_accountId,_sessionKey) = _loginDatabase.ExecuteSingleRaw<int, byte[]>(_loginDatabase.GetAccountInfoByUsername, new KeyValuePair<string, object>[]
                {
                    new ("@Name", _username),
                });

                byte[] loginBytes = Encoding.UTF8.GetBytes(_username);
                Span<byte> c = stackalloc byte[loginBytes.Length + 44];
                loginBytes.CopyTo(c);
                _sessionKey.CopyTo(c[(loginBytes.Length..)]);
                _seed.CopyTo(c[(loginBytes.Length + 40)..]);

                if (SHA1.HashData(c).SequenceEqual(hash.ToArray()))
                {
                    Log.Message("Successful redirect");
                }
                else
                {
                    Disconnect();
                    return;
                }

                break;
        }
    }

    private void SendAuthChallenge()
    {
        var header = new ServerPacketHeader(40, SMSG_AUTH_CHALLENGE);
        _smsg.Write(header);
        _smsg.Write((uint)1);
        _smsg.Write(_seed);
        _smsg.Write(RandomNumberGenerator.GetBytes(32));
        _stream.PushPacket(_smsg);
    }

    private void ReceiveFullPacket(ushort length)
    {
        if (length == 0)
            return;
        if (length > _cmsg.Length)
        {
            _cmsg = new byte[length];
        }
        if (_client.Available == length)
        {
            _stream.Read(_cmsg, 0, (int)length);
            return;
        }
        int bytesRead = 0;
        while (bytesRead < length)
        {
            bytesRead += _stream.Read(_cmsg, bytesRead, length - bytesRead);
        }
    }

    public void Disconnect()
    {
        Log.Warning("Disconnecting");
        _cts.Cancel();
        _stream.Close();
        _client.Close();
        _smsg.Dispose();
    }
}