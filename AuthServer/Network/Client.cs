﻿using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

using AuthServer.AuthStructs;
using AuthServer.Cryptography;
using AuthServer.Enums;
using AuthServer.Extensions;

namespace AuthServer.Network;

public class Client
{
    private TcpClient _client;
    private NetworkStream _stream;
    private byte[] _buffer;
    private string _username;
    private SRP6 _srp;
    private Server _server;
    private MemoryStream _smsg;
    private bool _authed;

    public Client(TcpClient client, Server server)
    {
        _client = client;
        _stream = client.GetStream();
        _server = server;
        _stream.WriteTimeout = (int)_server.WriteTimeout.TotalMilliseconds;
        _stream.ReadTimeout = 1;

        _buffer = new byte[200];
        _smsg = new MemoryStream(200);
        _authed = false;
    }

    public async Task HandleConnection()
    {
        while (_server.Running)
        {
            try
            {
                var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(_server.CancellationToken);
                if (!_authed)
                    timeoutCts.CancelAfter(_server.Timeout);
                int bytesRead = await _stream.ReadAsync(_buffer, 0, 1, timeoutCts.Token).ConfigureAwait(false);
                if (bytesRead == 0)
                {
                    Log.Warning("Клиент разорвал соединение");
                    break;
                }
                AuthCommand cmd = (AuthCommand)_buffer[0];
                Log.Message($"Получена команда {cmd}");
                if (!Enum.IsDefined(cmd))
                    throw new InvalidDataException();

                GetHandlerForCommand(cmd)();
                timeoutCts.Dispose();
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case InvalidDataException:
                        Log.Error($"Ошибка при подключении: {ex.Message}");
                        break;
                    case IOException:
                        Log.Error("Превышено время ожидания на подключение клиента");
                        break;
                    default:
                        Log.Error($"Неизвестное исключение: {ex.Message}");
                        break;
                }
            }
        }
        _stream.Close();
        _client.Close();
        return;
    }

    private Action GetHandlerForCommand(AuthCommand cmd) => cmd switch
    {
        AuthCommand.AUTH_LOGON_CHALLENGE        => HandleAuthLogonChallenge,
        AuthCommand.AUTH_LOGON_PROOF            => HandleAuthLogonProof,
        AuthCommand.AUTH_RECONNECT_CHALLENGE    => HandleReconnectChallenge,
        AuthCommand.AUTH_RECONNECT_PROOF        => HandleReconnectProof,

        AuthCommand.REALMLIST                   => HandleRealmlist,
        _ => null,
    };

    private unsafe void HandleAuthLogonChallenge()
    {
        _stream.Read(_buffer, 0, _client.Available);

        AuthLogonChallengeStruct pkt;
        fixed (byte* ptr = &_buffer[0])
            pkt = *(AuthLogonChallengeStruct*)ptr;

        AuthLogonChallengeStruct.Reverse(pkt.OS);
        AuthLogonChallengeStruct.Reverse(pkt.Platform);
        AuthLogonChallengeStruct.Reverse(pkt.Locale);
        Console.WriteLine($"Protocol: {pkt.ProtocolVersion}");
        Console.WriteLine($"Size: {pkt.Size}");
        Console.WriteLine($"Gamename: {Marshal.PtrToStringAnsi((IntPtr)pkt.Gamename)}");
        Console.Write("Version: ");
        for (int i = 0; i < 3; i++)
            Console.Write($"{pkt.Version[i]}.");
        Console.WriteLine(pkt.Build);
        Console.WriteLine($"Platform: {Marshal.PtrToStringAnsi((IntPtr)pkt.Platform + 1, 3)}");
        Console.WriteLine($"OS: {Marshal.PtrToStringAnsi((IntPtr)pkt.OS + 1, 3)}");
        Console.WriteLine($"Locale: {Marshal.PtrToStringAnsi((IntPtr)pkt.Locale, 4)}");
        Console.WriteLine($"Timezone: GMT+{pkt.Timezone_bias / 60}");
        Console.WriteLine($"IP: {new IPAddress(stackalloc byte[] { pkt.IP.Octet1, pkt.IP.Octet2, pkt.IP.Octet3, pkt.IP.Octet4 })}");
        _username = Marshal.PtrToStringAnsi((IntPtr)pkt.AccountName);
        Console.WriteLine($"Account name: {_username}");

        // соль и верифаер хранятся в бд, вместо генерации написать получение
        var salt = SRP6.GenerateSalt().ToArray();
        var verifier = SRP6.CalculateVerifier(_username, "123", salt).ToArray();
        _srp = new(verifier, salt);

        _smsg.Reset();
        _smsg.Write(AuthCommand.AUTH_LOGON_CHALLENGE);
        _smsg.Write(0);
        _smsg.Write(AuthResult.WOW_SUCCESS);
        _smsg.Write(_srp.ServerPublicKey);
        _smsg.Write(1);
        _smsg.Write(SRP6.G);
        _smsg.Write(32);
        _smsg.Write(SRP6.N);
        _smsg.Write(salt);
        for (int i = 0; i < 16; i++)
            _smsg.Write(0);
        _smsg.Write(0);
        _stream.PushPacket(_smsg);
    }

    private unsafe void HandleAuthLogonProof()
    {
        _stream.Read(_buffer, 0, _client.Available);

        AuthLogonProofStruct pkt;
        fixed (byte* ptr = &_buffer[0])
            pkt = *(AuthLogonProofStruct*)ptr;

        ReadOnlySpan<byte> A = new(pkt.A, 32);
        ReadOnlySpan<byte> M1 = new(pkt.M1, 20);
        _smsg.Reset();
        _smsg.Write(AuthCommand.AUTH_LOGON_PROOF);
        var m2 = _srp.GetM2(A, M1, _username);
        if (m2.IsEmpty)
        {
            _smsg.Write(AuthResult.WOW_FAIL_INCORRECT_PASSWORD);
            _smsg.Write((ushort)0);
            return;
        }
        _smsg.Write(AuthResult.WOW_SUCCESS);
        _smsg.Write(m2);
        _smsg.Write(0x00800000);
        _smsg.Write((uint)0);
        _smsg.Write((ushort)0);
        _stream.PushPacket(_smsg);
    }

    private void HandleReconnectChallenge()
    {
        Log.Error("Еще не поддерживается");
    }

    private void HandleReconnectProof()
    {
        Log.Error("Еще не поддерживается");
    }

    private void HandleRealmlist()
    {
        _stream.Read(_buffer, 0, 4);
        if (_authed)
            return;

        var accountValues = new Dictionary<int, (bool, byte)>()
        {
            { 1, (false, 10) },
            { 2, (true, 10) },
        };

        _server.RealmList.SendRealmListToNetworkStream(_stream, accountValues);
        _stream.Write(stackalloc byte[] { 0x10, 0x00 });
        _authed = true;
    }
}