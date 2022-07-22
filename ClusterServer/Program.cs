using Microsoft.Extensions.Configuration;
using Shared.Database;
using Shared.Network;
using Shared.RealmInfo;

namespace ClusterServer;

internal class Program
{
    static int Main(string[] args)
    {
        var config = new ConfigurationBuilder().AddIniFile("ClusterServer.cfg").Build();
        string connString = config["DB:AuthConnectionString"];
        ILoginDatabase loginDB = new SqlServerLoginDatabase(connString);
        int ID = int.Parse(config["Realm:ID"]);
        (var name, var realmType, var realmFlags, var address, var port, var timezone) =
            loginDB.ExecuteSingleRaw<string, RealmType, RealmFlags, string, int, RealmTimeZone>(loginDB.GetRealmInfo, new KeyValuePair<string, object>[]
        {
            new ("@Id", ID),
        });

        WorldAcceptor acceptor = new(address, port)
        {
            Timeout = TimeSpan.FromSeconds(3),
            WriteTimeout = TimeSpan.FromMilliseconds(10),
            LoginDatabase = loginDB,
        };
        _ = acceptor.Start();
        Console.ReadKey();
        acceptor.Stop();
        return 0;
    }
}