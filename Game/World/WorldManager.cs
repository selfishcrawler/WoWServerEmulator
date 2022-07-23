using Game.Network;
using Shared.Database;

namespace Game.World;

public static class WorldManager
{
    private static List<WorldSession> _sessions;
    public static ILoginDatabase LoginDatabase { get; private set; }
    public static byte RealmID { get; private set; }

    public static void InitWorld(byte realmID, ILoginDatabase ldb)
    {
        _sessions = new List<WorldSession>(100);
        LoginDatabase = ldb;
        RealmID = realmID;
    }

    public static void AddSession(WorldSession session)
    {
        _sessions.Add(session);
    }

    public static void AddPlayerToWorld()
    {
        foreach (var session in _sessions)
        {
            //check distance and send packets
        }
    }
}
