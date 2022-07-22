﻿using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

using Shared.Cryptography;
using Shared.Database;
using Shared.Entities;
using Shared.Extensions;
using Shared.Network.PacketStructs;

namespace Shared.Network;

public class WorldSession
{
    private delegate void PacketHandler(ClientPacketHeader header);

    private const int DefaultBufferSize = 1000;
    private const int ClientHeaderSize = 6;
    private readonly CancellationToken _serverCancellationToken;
    private CancellationTokenSource _cts;
    private readonly WorldAcceptor _acceptor;
    private readonly ILoginDatabase _loginDatabase;

    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly MemoryStream _smsg;
    private byte[] _cmsg;
    private ARC4 _encryptor;

    private readonly byte[] _seed;
    private byte[] _sessionKey;
    private string _username;
    private uint _timeSyncSequenceIndex;

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
        _timeSyncSequenceIndex = 0;
    }

    public async Task InitConnection()
    {
        SendAuthChallenge();
        _cts = CancellationTokenSource.CreateLinkedTokenSource(_serverCancellationToken);
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

        var header = new ServerPacketHeader(30, Opcode.SMSG_REDIRECT_CLIENT);
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
            Log.Warning($"Received {header.Opcode}");
            ReceiveFullPacket(header.Length);
            GetHandlerForPacket(header)(header);
        }
    }

    private PacketHandler GetHandlerForPacket(ClientPacketHeader header) => header.Opcode switch
    {
        Opcode.CMSG_CHAR_CREATE                         => HandleCharCreate,
        Opcode.CMSG_CHAR_ENUM                           => HandleCharEnum,
        Opcode.CMSG_PLAYER_LOGIN                        => HandlePlayerLogin,
        Opcode.CMSG_NAME_QUERY                          => HandleNameQuery,
        Opcode.CMSG_PING                                => HandlePing,
        Opcode.CMSG_SET_ACTIVE_MOVER                    => HandleSetActiveMover,
        Opcode.CMSG_REALM_SPLIT                         => HandleRealmSplit,
        Opcode.CMSG_TIME_SYNC_RESP                      => HandleTimeSyncResponce,
        Opcode.CMSG_SET_PLAYER_DECLINED_NAMES           => HandleSetPlayerDeclinedNames,
        Opcode.CMSG_READY_FOR_ACCOUNT_DATA_TIMES        => HandleReadyForAccountDataTimes,
        _ => UnhandledPacket,
    };

    private unsafe void HandleCharCreate(ClientPacketHeader _)
    {
        var header = new ServerPacketHeader(1, Opcode.SMSG_CHAR_CREATE);
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
        _smsg.Write(1); //char count
        _smsg.Write((ulong)1); // character guid
        _smsg.Write("Тестчар");
        _smsg.Write(1); // race
        _smsg.Write(1); //class
        _smsg.Write(1); //gender
        _smsg.Write(8); //skin
        _smsg.Write(2); //face
        _smsg.Write(8); //hairstyle
        _smsg.Write(5); //haircolor
        _smsg.Write(6); //facialstyle
        _smsg.Write(80); //level
        _smsg.Write((uint)12); //zone
        _smsg.Write((uint)0); //map
        _smsg.Write(-8949.95F); //x
        _smsg.Write(-132.493F); //y
        _smsg.Write(83.5312F); //z
        _smsg.Write((uint)0); //guild id
        _smsg.Write((uint)0); // char flags
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
        var header = new ServerPacketHeader((ushort)(_smsg.Position), Opcode.SMSG_CHAR_ENUM);
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
        var header = new ServerPacketHeader(20, Opcode.SMSG_LOGIN_VERIFY_WORLD);
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

        header = new ServerPacketHeader(32, Opcode.SMSG_TUTORIAL_FLAGS);
        _encryptor.Encrypt(ref header);
        for (int i = 0; i < 8; i++)
            _smsg.Write((uint)0xFF);
        lock (_stream)
        {
            _stream.Write(header);
            _stream.PushPacket(_smsg);
        }

        var player = new Player(1)
        {
            CurrentHealth = 100,
            MaxHealth = 200,
            Level = 8000000,
        };
        player.BuildUpdatePacket(_smsg);
        header = new ServerPacketHeader((ushort)_smsg.Position, Opcode.SMSG_UPDATE_OBJECT);
        _encryptor.Encrypt(ref header);
        lock (_stream)
        {
            _stream.Write(header);
            _stream.PushPacket(_smsg);
        }

        header = new ServerPacketHeader(4, Opcode.SMSG_TIME_SYNC_REQ);
        _encryptor.Encrypt(ref header);
        _smsg.Write(_timeSyncSequenceIndex++);

        lock (_stream)
        {
            _stream.Write(header);
            _stream.PushPacket(_smsg);
        }

        header = new ServerPacketHeader(2, Opcode.SMSG_FEATURE_SYSTEM_STATUS);
        _encryptor.Encrypt(ref header);
        _smsg.Write(2);
        _smsg.Write(0); //voice chat disable
        lock (_stream)
        {
            _stream.Write(header);
            _stream.PushPacket(_smsg);
        }
    }

    private void HandleNameQuery(ClientPacketHeader _)
    {
        _smsg.Write(new byte[] { 1, 1}); //packed guid
        _smsg.Write(0);
        _smsg.Write("Тестчар");
        _smsg.Write(0); // realm name
        _smsg.Write(1);
        _smsg.Write(1);
        _smsg.Write(1);
        _smsg.Write(0);

        var header = new ServerPacketHeader((ushort)_smsg.Position, Opcode.SMSG_NAME_QUERY_RESPONSE);
        _encryptor.Encrypt(ref header);
        lock (_stream)
        {
            _stream.Write(header);
            _stream.PushPacket(_smsg);
        }
    }

    private unsafe void HandlePing(ClientPacketHeader _)
    {
        CMSG_PING pkt;
        var header = new ServerPacketHeader(4, Opcode.SMSG_PONG);
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
        var header = new ServerPacketHeader((ushort)(8 + split_date.Length), Opcode.SMSG_REALM_SPLIT);
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
        var header = new ServerPacketHeader(12, Opcode.SMSG_SET_PLAYER_DECLINED_NAMES_RESULT);
        _encryptor.Encrypt(ref header);
        _stream.Write(header);
        _smsg.Write((uint)0); //result code 0/1
        _smsg.Write((ulong)1); //character guid
        lock (_stream)
        {
            _stream.PushPacket(_smsg);
        }
    }

    private void HandleReadyForAccountDataTimes(ClientPacketHeader header)
    {
        var sheader = new ServerPacketHeader(4 + 1 + 4 + 8 * 4, Opcode.SMSG_ACCOUNT_DATA_TIMES);
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
        if (header.Opcode != Opcode.CMSG_AUTH_SESSION &&
            header.Opcode != Opcode.CMSG_REDIRECTION_AUTH_PROOF)
        {
            Log.Error("Wrong opcode");
            Disconnect();
            return;
        }

        ReceiveFullPacket(header.Length);
        int index;
        switch (header.Opcode)
        {
            case Opcode.CMSG_AUTH_SESSION:
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

                    _sessionKey = _loginDatabase.ExecuteSingleValue<byte[]>(_loginDatabase.GetSessionKey, new KeyValuePair<string, object>[]
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
                var serverHeader = new ServerPacketHeader(11, Opcode.SMSG_AUTH_RESPONSE);
                _encryptor.Encrypt(ref serverHeader);
                _smsg.Write(serverHeader);
                _smsg.Write(12); //todo: auth result enum
                _smsg.Write((uint)0);
                _smsg.Write(0);
                _smsg.Write((uint)0);
                _smsg.Write(2); //expansion
                _stream.PushPacket(_smsg);
                break;
            case Opcode.CMSG_REDIRECTION_AUTH_PROOF:
                index = Array.IndexOf<byte>(_cmsg, 0, 0) + 1;
                _username = Encoding.UTF8.GetString(_cmsg, 0, index - 1);
                Span<byte> hash = _cmsg.AsSpan()[(index + 8)..(index+28)];

                _sessionKey = _loginDatabase.ExecuteSingleValue<byte[]>(_loginDatabase.GetSessionKey, new KeyValuePair<string, object>[]
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
        var header = new ServerPacketHeader(40, Opcode.SMSG_AUTH_CHALLENGE);
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
        _acceptor.RemoveSession(this);
        _cts.Cancel();
        _stream.Close();
        _client.Close();
    }
}