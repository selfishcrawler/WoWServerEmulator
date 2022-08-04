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

enum AccountDataType
{
    GlobalConfigCache,
    PerCharacterConfigCache,
    GlobalBindingsCache,
    PerCharacterBindingsCache,
    GlobalMacrosCache,
    PerCharacterMacrosCache,
    PerCharacterLayourCache,
    PerCharacterChatCache,
}

class AccountData
{
    private readonly AccountDataType _dataType;
    public uint Time { get; private set; } = 0;
    public string Data { get; private set; } = string.Empty;

    public AccountData(AccountDataType type)
    {
        _dataType = type;
    }

    public void Write(MemoryStream ms)
    {
        ms.Write((ulong)0);
        ms.Write(_dataType);
        ms.Write(Time);

        var data = Encoding.UTF8.GetBytes(Data);
        ms.Write((uint)data.Length);

        DeflateStream compress = new(ms, CompressionLevel.Optimal, true);
        compress.Write(data);
    }
}

public partial class WorldSession
{
    private delegate void PacketHandler(ClientPacketHeader header);

    private const int DefaultBufferSize = 1000;
    private const int ClientHeaderSize = 6;
    private CancellationTokenSource _cts;

    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly MemoryStream _smsg;
    private byte[] _cmsg;
    private ARC4 _encryptor;

    private readonly byte[] _seed;
    private byte[] _sessionKey;
    private string _username;
    private uint _timeSyncSequenceIndex;
    private Timer _logoutTimer;

    public Player ActiveCharacter { get; private set; }
    public int AccountID { get; private set; }

    public WorldSession(TcpClient client)
    {
        _client = client;
        _stream = client.GetStream();
        _smsg = new MemoryStream(DefaultBufferSize);
        _cmsg = new byte[DefaultBufferSize];
        _seed = RandomNumberGenerator.GetBytes(4);
        _timeSyncSequenceIndex = 0;
    }

    public async Task InitConnection(CancellationToken serverToken)
    {
        (var serverSeed, var clientSeed) = SendAuthChallenge();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(serverToken);
        int bytesRead = await _stream.ReadAsync(_cmsg, 0, ClientHeaderSize, _cts.Token);
        if (bytesRead == 0)
            Disconnect();
        ProcessAuthSession(serverSeed, clientSeed);
        await HandlePacketFlow();
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

    public void SendRedirect(IPAddress ip, ushort port)
    {
        Span<byte> host = stackalloc byte[6];
        Span<byte> hash = stackalloc byte[20];
        ip.TryWriteBytes(host, out _);
        BitConverter.GetBytes(port).CopyTo(host[4..]);
        HMACSHA1.HashData(_sessionKey, host, hash);

        _smsg.Write(host);
        _smsg.Write((uint)0);
        _smsg.Write(hash);
        SendPacket(SMSG_REDIRECT_CLIENT);
    }

    public void SendLoginError(LoginError code)
    {
        _smsg.Write(code);
        SendPacket(SMSG_CHARACTER_LOGIN_FAILED);
    }

    private unsafe void HandleCharCreate(ClientPacketHeader header)
    {
        CMSG_CHAR_CREATE pkt;
        int index = Array.IndexOf<byte>(_cmsg, 0);

        if (index == -1 || header.Length - index - 1 != sizeof(CMSG_CHAR_CREATE))
        {
            Disconnect();
            return;
        }

        fixed (byte* ptr = &_cmsg[index + 1])
            pkt = *(CMSG_CHAR_CREATE*)ptr;
        string name = Encoding.UTF8.GetString(_cmsg, 0, index);

        if (name.Length < 2)
        {
            SendResponce(ResponseCode.CHAR_NAME_TOO_SHORT);
            return;
        }
        if (name.Length > 16)
        {
            SendResponce(ResponseCode.CHAR_NAME_TOO_LONG);
            return;
        }

        foreach (char c in name)
        {
            if (!char.IsLetter(c) || c is not '\'')
            {
                SendResponce(ResponseCode.CHAR_NAME_INVALID_CHARACTER);
                return;
            }
        }

        char first = char.ToLower(name[0]);
        if (first is 'ъ' or 'ь')
        {
            SendResponce(ResponseCode.CHAR_NAME_RUSSIAN_SILENT_CHARACTER_AT_BEGINNING_OR_END);
            return;
        }

        var nameIsUsed = Database.Login.ExecuteSingleValue<bool>(Database.Login.IsNameInUse, new KeyValuePair<string, object>[]
        {
            new("@Name", name),
            new("@Realm", WorldManager.RealmID),
        });

        if (nameIsUsed)
        {
            SendResponce(ResponseCode.CHAR_CREATE_NAME_IN_USE);
            return;
        }

        if (!Enum.IsDefined(pkt.Race) ||
            !Enum.IsDefined(pkt.Class) ||
            !Enum.IsDefined(pkt.Gender))
        {
            SendResponce(ResponseCode.CHAR_CREATE_FAILED);
            return;
        }

        (var nameUsed, var charCount, var canCreateDK, var hasDK) =
            Database.Login.ExecuteSingleRaw<bool, int, bool, bool>(Database.Login.GetCharacterCreationInfo, new KeyValuePair<string, object>[]
        {
            new("@Account", AccountID),
            new("@Name", name),
            new("@Realm", WorldManager.RealmID),
        });

        if (nameUsed)
        {
            SendResponce(ResponseCode.CHAR_CREATE_NAME_IN_USE);
            return;
        }

        if (charCount > 10)
        {
            SendResponce(ResponseCode.CHAR_CREATE_SERVER_LIMIT);
            return;
        }

        if (pkt.Class == Class.DeathKnight)
        {
            if (!canCreateDK)
            {
                SendResponce(ResponseCode.CHAR_CREATE_LEVEL_REQUIREMENT);
                return;
            }
            if (hasDK)
            {
                SendResponce(ResponseCode.CHAR_CREATE_UNIQUE_CLASS_LIMIT);
                return;
            }
        }

        (var map, var zone, var pos) = WorldManager.GetStartingPosition(pkt.Race, pkt.Class);

        Database.Login.ExecuteNonQuery(Database.Login.CreateCharacter, new KeyValuePair<string, object>[]
        {
            new("@Account", AccountID),
            new("@Realm", WorldManager.RealmID),
            new("@Name", name),
            new("@Level", pkt.Class == Class.DeathKnight ? WorldManager.DKStartingLevel : WorldManager.StartingLevel),
            new("@Race", pkt.Race),
            new("@Class", pkt.Class),
            new("@Gender", pkt.Gender),
            new("@Map", map),
            new("@Zone", zone),
            new("@X", pos.X),
            new("@Y", pos.Y),
            new("@Z", pos.Z),
            new("@Orientation", pos.Orientation),
            new("@Skin", pkt.Skin),
            new("@Face", pkt.Face),
            new("@HairStyle", pkt.HairStyle),
            new("@HairColor", pkt.HairColor),
            new("@FacialStyle", pkt.FacialStyle),
        });
        SendResponce(ResponseCode.CHAR_CREATE_SUCCESS);

        void SendResponce(ResponseCode code)
        {
            _smsg.Write(code);
            SendPacket(SMSG_CHAR_CREATE);
        }
    }

    private unsafe void HandleCharEnum(ClientPacketHeader _)
    {
        var charList = Database.Login.ExecuteMultipleRaws(Database.Login.GetCharacterList, new KeyValuePair<string, object>[]
        {
            new("@Account", AccountID),
            new("@Realm", WorldManager.RealmID),
        });

        _smsg.Write((byte)charList.Count());
        foreach (var character in charList)
        {
            _smsg.Write((ulong)(long)character[0]);
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
        SendPacket(SMSG_CHAR_ENUM);
    }

    private void HandleCharDelete(ClientPacketHeader _)
    {

    }

    public void LoginAsCharacter(uint guid)
    {
        var charInfo = Database.Login.ExecuteSingleRaw(Database.Login.GetCharacterInfo, new KeyValuePair<string, object>[]
        {
            new("@Guid", (long)guid),
            new("@Account", AccountID),
        });

        if (charInfo is null)
        {
            Disconnect();
            return;
        }

        Race race = (Race)charInfo[2];
        Gender gender = (Gender)charInfo[4];

        ActiveCharacter = new Player()
        {
            Guid = guid,
            Name = charInfo[0].ToString(),
            Alive = true,
            CurrentHealth = 100,
            MaxHealth = 200,
            Level = (uint)(int)charInfo[1],
            Race = race,
            Class = (Class)charInfo[3],
            Gender = gender,
            PowerType = PowerType.Happiness,
            DisplayID = WorldManager.GetCharacterDisplayId(race, gender),
            NativeDisplayID = WorldManager.GetCharacterDisplayId(race, gender),

            WalkSpeed = 1f,
            RunSpeed = 3f,
            BackwardsRunSpeed = 1f
        };

        ActiveCharacter.SetPosition((float)charInfo[7], (float)charInfo[8], (float)charInfo[9], (float)charInfo[10]);
        ActiveCharacter.SetCurrentPower(PowerType.Happiness, 1000000);
        ActiveCharacter.SetMaxPower(PowerType.Happiness, 2000000);


        _smsg.Write((uint)(int)charInfo[5]);
        _smsg.Write(ActiveCharacter.Position.X);
        _smsg.Write(ActiveCharacter.Position.Y);
        _smsg.Write(ActiveCharacter.Position.Z);
        _smsg.Write(ActiveCharacter.Position.Orientation);
        SendPacket(SMSG_LOGIN_VERIFY_WORLD);

        for (int i = 0; i < 8; i++)
            _smsg.Write((uint)0xFF);
        SendPacket(SMSG_TUTORIAL_FLAGS);

        _smsg.Write((uint)1); // block count
        ActiveCharacter.BuildCreatePacket(_smsg);

        SendPacket(SMSG_UPDATE_OBJECT);

        WorldManager.AddPlayerToWorld(this);

        _smsg.Write(_timeSyncSequenceIndex++);
        SendPacket(SMSG_TIME_SYNC_REQ);

        _smsg.Write(2);
        _smsg.Write(0); //voice chat disable
        SendPacket(SMSG_FEATURE_SYSTEM_STATUS);
    }

    private unsafe void HandlePlayerLogin(ClientPacketHeader header)
    {
        if (header.Length != sizeof(CMSG_PLAYER_LOGIN))
        {
            Disconnect();
            return;
        }
        CMSG_PLAYER_LOGIN pkt;
        fixed (byte* ptr = &_cmsg[0])
            pkt = *(CMSG_PLAYER_LOGIN*)ptr;
        int map = Database.Login.ExecuteSingleValue<int>(Database.Login.GetCharacterMap, new KeyValuePair<string, object>[]
        {
            new("@Guid", (long)pkt.Guid),
        });

        WorldManager.NodeManager.EnterWorld(this, map, pkt.Guid);
    }

    private void HandleLogoutRequest(ClientPacketHeader _)
    {
        bool instantLogout = true;
        _smsg.Write((uint)0);
        _smsg.Write(instantLogout);
        SendPacket(SMSG_LOGOUT_RESPONSE);
        _logoutTimer = new Timer((_) =>
        {
            SendPacket(SMSG_LOGOUT_COMPLETE);
            WorldManager.NodeManager.Logout(this);
        }, null, TimeSpan.FromSeconds(instantLogout ? 0 : 20), TimeSpan.Zero);
    }

    private void HandleNameQuery(ClientPacketHeader header)
    {
        if (header.Length != sizeof(ulong))
        {
            Disconnect();
            return;
        }
        ulong guid = BitConverter.ToUInt64(_cmsg);
        var player = WorldManager.GetPlayerByGUID(guid);
        if (player is null)
            return;
        _smsg.Write(player.PackedGuid);
        _smsg.Write(0);
        _smsg.Write(player.Name);
        _smsg.Write(0); // realm name
        _smsg.Write(player.Race);
        _smsg.Write(player.Gender);
        _smsg.Write(player.Class);
        _smsg.Write(0);
        SendPacket(SMSG_NAME_QUERY_RESPONSE);
    }

    private void HandleMovementPacket(ClientPacketHeader header)
    {
        WorldManager.BroadcastMovementPacket(this, _cmsg.ToArray(), header.Opcode);
    }

    private unsafe void HandlePing(ClientPacketHeader _)
    {
        CMSG_PING pkt;
        fixed (byte* ptr = &_cmsg[0])
            pkt = *(CMSG_PING*)ptr;
        _smsg.Write(pkt.Ping);
        SendPacket(SMSG_PONG);
    }

    private void HandleUpdateAccountData(ClientPacketHeader _)
    {
        var type = (AccountDataType)BitConverter.ToUInt32(_cmsg);
        Log.Warning(type);
    }

    private void HandleRequestAccountData(ClientPacketHeader header)
    {
        uint dataType = BitConverter.ToUInt32(_cmsg);
        if (!Enum.IsDefined((AccountDataType)dataType))
            return;

        var data = new AccountData((AccountDataType)dataType);
        data.Write(_smsg);
        SendPacket(SMSG_UPDATE_ACCOUNT_DATA);
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
        _smsg.Write(pkt.Unk);
        _smsg.Write(RealmNormal);
        _smsg.Write(split_date);
        SendPacket(SMSG_REALM_SPLIT);
    }

    private void HandleTimeSyncResponce(ClientPacketHeader _)
    {

    }

    private void HandleSetPlayerDeclinedNames(ClientPacketHeader _)
    {
        _smsg.Write((uint)0); //result code 0/1
        _smsg.Write((ulong)2); //character guid
        SendPacket(SMSG_SET_PLAYER_DECLINED_NAMES_RESULT);
    }

    private void HandleReadyForAccountDataTimes(ClientPacketHeader header)
    {
        var time = (uint)DateTimeOffset.Now.ToUnixTimeSeconds();
        _smsg.Write(time);
        _smsg.Write(1);
        _smsg.Write((uint)0x15);
        for (int i = 0; i < 3; i++)
        {
            _smsg.Write((uint)time);
        }
        SendPacket(SMSG_ACCOUNT_DATA_TIMES);
    }

    private void HandleRedirectionFailed(ClientPacketHeader header)
    {
        WorldManager.NodeManager.RedirectionFailed(this);
    }

    private void UnhandledPacket(ClientPacketHeader header)
    {
        uint code = (uint)header.Opcode;
        Log.Warning($"Received unhandled packet: 0x{code:X3}");
    }

    private void ProcessAuthSession(byte[] serverSeed, byte[] clientSeed)
    {
        var header = new ClientPacketHeader(BitConverter.ToUInt16(_cmsg, 0), (Opcode)BitConverter.ToUInt32(_cmsg, 2));
        if (header.Opcode is not CMSG_AUTH_SESSION &&
            header.Opcode is not CMSG_REDIRECTION_AUTH_PROOF)
        {
            Log.Error("Wrong opcode");
            Disconnect();
            return;
        }

        ReceiveFullPacket(header.Length);
        switch (header.Opcode)
        {
            case CMSG_AUTH_SESSION:
                HandleAuthSession();
                break;
            case CMSG_REDIRECTION_AUTH_PROOF:
                HandleRedirectionAuthProof(serverSeed, clientSeed);
                break;
        }
        WorldManager.AddSession(this);
    }

    private unsafe void HandleAuthSession()
{
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
            Span<byte> proof = new(ptr + index + 28, 20);
            pkt.AddonData = (ptr + index + 48);

            byte[] login = Encoding.UTF8.GetBytes(_username);
            Span<byte> concat = stackalloc byte[login.Length + 52];
            login.CopyTo(concat);
            (stackalloc byte[] { 0, 0, 0, 0 }).CopyTo(concat[login.Length..]);
            BitConverter.GetBytes(pkt.Seed).CopyTo(concat[(login.Length + 4)..]);
            _seed.CopyTo(concat[(login.Length + 8)..]);

            if (!QueryAccountInfo())
            {
                Disconnect();
                return;
            }

            _sessionKey.CopyTo(concat[(login.Length + 12)..]);

            if (!IsCorrectClientChallengeResponce(proof, concat))
            {
                Disconnect();
                return;
            }
        }

        _encryptor = new ARC4(_sessionKey);
        _smsg.Write(ResponseCode.AUTH_OK);
        _smsg.Write((uint)0);
        _smsg.Write(0);
        _smsg.Write((uint)0);
        _smsg.Write(2); //expansion
        SendPacket(SMSG_AUTH_RESPONSE);
    }

    private void HandleRedirectionAuthProof(byte[] serverSeed, byte[] clientSeed)
    {
        int index = Array.IndexOf<byte>(_cmsg, 0, 0) + 1;
        _username = Encoding.UTF8.GetString(_cmsg, 0, index - 1);
        Span<byte> hash = _cmsg.AsSpan()[(index + 8)..(index + 28)];

        if (!QueryAccountInfo())
        {
            Disconnect();
            return;
        }

        byte[] loginBytes = Encoding.UTF8.GetBytes(_username);
        Span<byte> concat = stackalloc byte[loginBytes.Length + 44];
        loginBytes.CopyTo(concat);
        _sessionKey.CopyTo(concat[(loginBytes.Length..)]);
        _seed.CopyTo(concat[(loginBytes.Length + 40)..]);
        _encryptor = new ARC4(_sessionKey, serverSeed, clientSeed);

        if (!IsCorrectClientChallengeResponce(hash, concat))
        {
            Disconnect();
            return;
        }
        SendPacket(SMSG_RESUME_COMMS);
    }

    private bool QueryAccountInfo()
    {
        (AccountID, _sessionKey) = Database.Login.ExecuteSingleRaw<int, byte[]>(Database.Login.GetAccountInfoByUsername, new KeyValuePair<string, object>[]
        {
            new ("@Name", _username),
        });
        return _sessionKey is not default(byte[]);
    }

    private bool IsCorrectClientChallengeResponce(ReadOnlySpan<byte> clientData, ReadOnlySpan<byte> serverConcat)
    {
        Span<byte> hash = stackalloc byte[20];
        SHA1.HashData(serverConcat, hash);
        return hash.SequenceEqual(clientData);
    }

    private (byte[], byte[]) SendAuthChallenge()
    {
        var header = new ServerPacketHeader(40, SMSG_AUTH_CHALLENGE);
        _smsg.Write(header);
        _smsg.Write((uint)1);
        _smsg.Write(_seed);
        byte[] serverSeed = RandomNumberGenerator.GetBytes(16);
        byte[] clientSeed = RandomNumberGenerator.GetBytes(16);
        _smsg.Write(serverSeed);
        _smsg.Write(clientSeed);
        _stream.PushPacket(_smsg);
        return (serverSeed, clientSeed);
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
            _stream.Read(_cmsg, 0, length);
            return;
        }
        int bytesRead = 0;
        while (bytesRead < length)
        {
            bytesRead += _stream.Read(_cmsg, bytesRead, length - bytesRead);
        }
    }

    public void SendPacket(ReadOnlySpan<byte> packet, Opcode opcode)
    {
        lock (_stream)
        {
            var header = new ServerPacketHeader((ushort)packet.Length, opcode);
            _encryptor.Encrypt(ref header);
            _stream.Write(header);
            _stream.Write(packet);
        }
    }
    public void SendPacket(MemoryStream packet, Opcode opcode)
    {
        lock (_stream)
        {
            var header = new ServerPacketHeader((ushort)packet.Position, opcode);
            _encryptor.Encrypt(ref header);
            _stream.Write(header);
            _stream.PushPacket(packet);
        }
    }

    private void SendPacket(Opcode opcode)
    {
        SendPacket(_smsg, opcode);
    }

    public void Disconnect()
    {
        Log.Warning("Disconnecting");
        WorldManager.RemoveSession(this);
        _cts.Cancel();
        _stream.Close();
        _client.Close();
        _smsg.Dispose();
    }
}