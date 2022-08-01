using System.Net;
using Microsoft.Extensions.Configuration;
using Game.Network;
using Game.World;
using Shared.Database;
using Shared.RealmInfo;

namespace ClusterServer;

public static class Program
{
    static int Main(string[] args)
    {
        var config = new ConfigurationBuilder().AddIniFile("ClusterServer.cfg").Build();
        string connString = config["DB:AuthConnectionString"];
        Database.Login = new SqlServerLoginDatabase(connString);
        byte ID = byte.Parse(config["Realm:ID"]);
        (var name, var realmType, var realmFlags, var address, var port, var timezone) =
            Database.Login.ExecuteSingleRaw<string, RealmType, RealmFlags, string, int, RealmTimeZone>(Database.Login.GetRealmInfo, new KeyValuePair<string, object>[]
        {
            new ("@Id", ID),
        });

        ClusterManager manager = new(IPAddress.Loopback, 3000);

        WorldManager.InitWorld(ID, manager);
        WorldAcceptor acceptor = new(address, port)
        {
            Timeout = TimeSpan.FromSeconds(3),
            WriteTimeout = TimeSpan.FromMilliseconds(10),
        };
        _ = acceptor.Start();
        Console.ReadKey();
        //acceptor.Stop();
        return 0;
    }
}