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
    public ILoginDatabase LoginDatabase { get; private init; }

    public Server(ILoginDatabase loginDB, string ip, int port = DefaultAuthserverPort) : this(loginDB, IPAddress.TryParse(ip, out IPAddress _ip) ? _ip : null, port) { }

    public Server(ILoginDatabase loginDB, IPAddress ip, int port = DefaultAuthserverPort)
    {
        ArgumentNullException.ThrowIfNull(ip, nameof(ip));
        if (port < 0 || port > ushort.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(port));

        IP = ip;
        Port = port;
        _listener = new TcpListener(IP, Port);
        LoginDatabase = loginDB;
        RealmList = new(LoginDatabase);
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
            tcpClient.ReceiveTimeout = 1;
            var client = new Client(tcpClient, this);
            _ = client.HandleConnection().ContinueWith((t, c) =>
            {
                if (t.IsFaulted)
                {
                    Log.Error("Unhandled exception in auth session");
                    (c as Client).CloseConnection();
                }
            }, client);
        }
    }

    public void Stop()
    {
        _cts.Cancel();
        _cts.Dispose();
        _listener.Stop();
    }
}