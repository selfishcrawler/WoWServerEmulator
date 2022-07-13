using System.Net;
using System.Net.Sockets;
using AuthServer.Realms;
using Shared.Database;

namespace AuthServer.Network;

public class Server
{
    private const int DefaultAuthserverPort = 3724;
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
    public RealmList RealmList { get; private set; }
    public ILoginDatabase LoginDatabase { get; init; }

    public Server(string ip, int port = DefaultAuthserverPort) : this(IPAddress.TryParse(ip, out IPAddress _ip) ? _ip : null, port) { }

    public Server(IPAddress ip, int port = DefaultAuthserverPort)
    {
        ArgumentNullException.ThrowIfNull(ip, nameof(ip));
        if (port < 0 || port > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(port));

        IP = ip;
        Port = port;
        _listener = new TcpListener(IP, Port);
        RealmList = new();
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
            var client = new Client(tcpClient, this);
            _ = client.HandleConnection();
        }
    }

    public void Stop()
    {
        _cts.Cancel();
        _cts.Dispose();
        _listener.Stop();
    }
}