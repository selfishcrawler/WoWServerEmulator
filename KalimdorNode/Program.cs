using System.Net;
using Game.Network;
using Game.Network.Clustering;
using Game.World;
using Microsoft.Extensions.Configuration;
using Shared.Database;
using Shared.RealmInfo;

namespace KalimdorNode;

internal class Program
{
    static int Main(string[] args)
    {
        var config = new ConfigurationBuilder().AddIniFile("KalimdorNode.cfg").Build();

        string connString = config["DB:AuthConnectionString"];
        Database.Login = new SqlServerLoginDatabase(connString);
        connString = config["DB:ClusterConnectionString"];
        Database.Cluster = new SqlServerClusterDatabase(connString);

        byte ID = byte.Parse(config["Realm:ID"]);
        int NodeID = int.Parse(config["Node:ID"]);
        (var name, var realmType, var realmFlags, var address, var port, var timezone) =
            Database.Login.ExecuteSingleRaw<string, RealmType, RealmFlags, string, int, RealmTimeZone>(Database.Login.GetRealmInfo, new KeyValuePair<string, object>[]
        {
            new ("@Id", ID),
        });

        (var clusterIp, var clusterPort) = Database.Cluster.ExecuteSingleRaw<string, int>(Database.Cluster.GetClusterConfiguration, null);
        NodeManager node = new(NodeID, IPAddress.Parse(clusterIp), clusterPort);

        (address, port) = Database.Cluster.ExecuteSingleRaw<string, int>(Database.Cluster.GetNodeEndpoint, new KeyValuePair<string, object>[]
        {
            new("@ID", NodeID),
        });

        WorldManager.InitWorld(ID, node);
        WorldManager.InitCreatures();
        WorldAcceptor acceptor = new(address, port)
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