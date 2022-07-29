using Game.Network;
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
        var LoginDatabase = new SqlServerLoginDatabase(connString);
        byte ID = byte.Parse(config["Realm:ID"]);
        (var name, var realmType, var realmFlags, var address, var port, var timezone) =
            LoginDatabase.ExecuteSingleRaw<string, RealmType, RealmFlags, string, int, RealmTimeZone>(LoginDatabase.GetRealmInfo, new KeyValuePair<string, object>[]
        {
            new ("@Id", ID),
        });

        WorldManager.InitWorld(ID, LoginDatabase);
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