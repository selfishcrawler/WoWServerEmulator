using AuthServer.Network;
using AuthServer.Cryptography;
using AuthServer.Enums;

namespace AuthServer;

class Program
{
    static int Main()
    {
        Server s;
        try
        {
            //explicit init of all SRP6 values before everything
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(SRP6).TypeHandle);
            s = new Server("0.0.0.0")
            {
                Timeout = TimeSpan.FromSeconds(3),
                WriteTimeout = TimeSpan.FromMilliseconds(10),
            };

            var realm1 = new Realms.Realm(1, "Для гладиаторов", "127.0.0.1:8085")
            {
                RealmType = RealmType.PVP,
                Locked = false,
                Flags = RealmFlags.NONE,
                Population = 200,
                TimeZone = RealmTimeZone.Russian,
                ID = 1,
                Version = new byte[] { 3, 3, 5 },
                Build = 12340,
            };


            var realm2 = new Realms.Realm(2, "Четкий реалм", "127.0.0.1:8086")
            {
                RealmType = RealmType.NORMAL,
                Locked = false,
                Flags = RealmFlags.NONE,
                Population = 400,
                TimeZone = RealmTimeZone.Russian,
                ID = 2,
                Version = new byte[] { 3, 3, 5 },
                Build = 12340,
            };

            s.RealmList.Add(realm1);
            s.RealmList.Add(realm2);
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