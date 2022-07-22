using Microsoft.Extensions.Configuration;
using AuthServer.Network;
using AuthServer.Cryptography;
using Shared.Database;

namespace AuthServer;

class Program
{
    static int Main()
    {
        Server s;
        var config = new ConfigurationBuilder().AddIniFile("AuthServer.cfg").Build();
        try
        {
            //explicit init of all SRP6 values before everything
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(SRP6).TypeHandle);
            string connString = config["DB:AuthConnectionString"];
            s = new Server(new SqlServerLoginDatabase(connString), "0.0.0.0")
            {
                Timeout = TimeSpan.FromSeconds(3),
                WriteTimeout = TimeSpan.FromMilliseconds(10),
            };

            _ = s.Start();
        }
        catch (Exception ex)
        {
            Log.Error($"Ошибка при задании конфигурации сервера: {ex.Message}");
            return 1;
        }
        Console.ReadKey();
        s.Stop();
        return 0;
    }
}