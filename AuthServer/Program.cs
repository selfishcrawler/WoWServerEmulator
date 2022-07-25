using Microsoft.Extensions.Configuration;
using AuthServer.Network;
using AuthServer.Cryptography;
using Shared.Database;

namespace AuthServer;

class Program
{
    static int Main()
    {
        //explicit init of all SRP6 values before everything
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(SRP6).TypeHandle);

        Server s;
        var config = new ConfigurationBuilder().AddIniFile("AuthServer.cfg").Build();
        ILoginDatabase loginDatabase;
        try
        {
            string connString = config["DB:AuthConnectionString"];
            loginDatabase = new SqlServerLoginDatabase(connString);
            s = new Server(loginDatabase, "0.0.0.0")
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

        AuthServerCommandHandler commandHandler = new(s, loginDatabase);
        commandHandler.Handle();
        return 0;
    }
}