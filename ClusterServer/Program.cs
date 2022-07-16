using Microsoft.Extensions.Configuration;
using Shared.Database;
using Shared.Network;

namespace ClusterServer;

internal class Program
{
    static int Main(string[] args)
    {
        var config = new ConfigurationBuilder().AddIniFile("ClusterServer.cfg").Build();
        string connString = config["DB:AuthConnectionString"];
        WorldAcceptor acceptor = new("0.0.0.0")
        {
            Timeout = TimeSpan.FromSeconds(3),
            WriteTimeout = TimeSpan.FromMilliseconds(10),
            LoginDatabase = new SqlServerLoginDatabase(connString, ""),
        };
        _ = acceptor.Start();
        Console.ReadKey();
        acceptor.Stop();
        return 0;
    }
}