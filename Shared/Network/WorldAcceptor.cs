using System.Net;
using System.Net.Sockets;
using Shared.Database;

namespace Shared.Network;

public class WorldAcceptor
{
    private const int DefaultWorldPort = 8085;
    private TcpListener _listener;
    private CancellationTokenSource _cts;
    public IPAddress IP { get; private set; }
    public int Port { get; private set; }
    public TimeSpan Timeout { get; set; }
    public TimeSpan WriteTimeout { get; set; }
    public CancellationToken CancellationToken
    {
        get => _cts.Token;
    }
    public bool Running
    {
        get => _cts is not null && !_cts.IsCancellationRequested;
    }

    private readonly List<WorldSession> _sessions;
    public ILoginDatabase LoginDatabase { get; init; }

    public WorldAcceptor(string ip, int port = DefaultWorldPort) : this(IPAddress.TryParse(ip, out IPAddress _ip) ? _ip : null, port) { }

    public WorldAcceptor(IPAddress ip, int port = DefaultWorldPort)
    {
        ArgumentNullException.ThrowIfNull(ip, nameof(ip));
        if (port < 0 || port > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(port));

        IP = ip;
        Port = port;
        _listener = new TcpListener(IP, Port);
        _sessions = new List<WorldSession>(100);
    }

    public async Task Start()
    {
        if (Running)
        {
            Log.Warning("Сервер уже запущен");
            return;
        }
        _cts = new CancellationTokenSource();
        try
        {
            _listener.Start();
            Log.Message($"Сервер запущен на {IP}:{Port}");
        }
        catch (SocketException se)
        {
            Log.Error($"Ошибка запуска сервера: {se.Message}");
            _cts.Cancel();
            return;
        }
        while (!_cts.IsCancellationRequested)
        {
            var tcpClient = await _listener.AcceptTcpClientAsync(_cts.Token);
            tcpClient.SendTimeout = (int)WriteTimeout.TotalMilliseconds;
            tcpClient.ReceiveTimeout = 3;
            var client = new WorldSession(tcpClient, this);
            _sessions.Add(client);
            _ = client.InitConnection();
        }
    }

    public void RemoveSession(WorldSession session)
    {
        _sessions.Remove(session);
    }

    public void Stop()
    {
        _cts.Cancel();
        _cts.Dispose();
        _listener.Stop();
    }
}