using System.Net;
using Game.Network;
using Game.Network.Clustering;
using Game.World;
using Microsoft.Extensions.Configuration;
using Shared.Database;
using Shared.RealmInfo;

namespace EasternKingdomsNode;

internal class Program
{
    static int Main(string[] args)
    {
        var config = new ConfigurationBuilder().AddIniFile("EasternKingdomsNode.cfg").Build();
        string connString = config["DB:AuthConnectionString"];
        Database.Login = new SqlServerLoginDatabase(connString);
        byte ID = byte.Parse(config["Realm:ID"]);
        (var name, var realmType, var realmFlags, var address, var port, var timezone) =
            Database.Login.ExecuteSingleRaw<string, RealmType, RealmFlags, string, int, RealmTimeZone>(Database.Login.GetRealmInfo, new KeyValuePair<string, object>[]
        {
            new ("@Id", ID),
        });

        NodeManager node = new(1, IPAddress.Loopback, 3000);

        WorldManager.InitWorld(ID, node);
        WorldAcceptor acceptor = new(address, 8086)
        {
            Timeout = TimeSpan.FromSeconds(3),
            WriteTimeout = TimeSpan.FromMilliseconds(10),
        };
        _ = acceptor.Start();
        Console.ReadKey();
        acceptor.Stop();
        return 0;
    }
}