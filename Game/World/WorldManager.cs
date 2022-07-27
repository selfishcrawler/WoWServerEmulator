using Game.Entities;
using Game.Network;
using Shared.Database;
using Shared.RealmInfo;
using System.Net.Sockets;

namespace Game.World;

public static class WorldManager
{
    private static List<WorldSession> _sessions;
    public static ILoginDatabase LoginDatabase { get; private set; }
    public static byte RealmID { get; private set; }
    public static RealmTimeZone RealmTimeZone { get; private set; }
    public static RealmType RealmType { get; private set; }
    public const int StartingLevel = 1;
    public const int DKStartingLevel = 55;

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

    public static void RemoveSession(WorldSession session)
    {
        _sessions.Remove(session);
    }

    public static void AddPlayerToWorld(WorldSession self)
    {
        Player addingPlayer = self.ActiveCharacter;
        using MemoryStream broadcast = new(400);
        using MemoryStream surroundingPlayers = new(400);
        broadcast.Write((uint)1);
        addingPlayer.BuildUpdatePacket(broadcast, false);
        foreach (var session in _sessions)
        {
            if (session == self)
                continue;

            //check distance
            surroundingPlayers.Write((uint)1);
            session.ActiveCharacter.BuildUpdatePacket(surroundingPlayers, false);
            self.SendPacket(surroundingPlayers, Opcode.SMSG_UPDATE_OBJECT);
            session.SendPacket(broadcast, Opcode.SMSG_UPDATE_OBJECT);
        }
    }

    public static void BroadcastMovementPacket(WorldSession self, ReadOnlySpan<byte> packet, Opcode pktType)
    {
        foreach (var session in _sessions)
        {
            if (session == self)
                continue;

            //check distance

            session.SendPacket(packet, pktType);
        }
    }

    public static Player GetPlayerByGUID(ulong guid)
    {
        foreach (var session in _sessions)
            if (session.ActiveCharacter is not null)
                if (session.ActiveCharacter.Guid == guid)
                    return session.ActiveCharacter;
        return null;
    }

    // map, zone, coords
    public static (int, int, Position) GetStartingPosition(Race race, Class _class) => (race, _class) switch
    {
        (_, Class.DeathKnight) => (609, 4298, new Position() { X = 0.0f, Y = 0.0f, Z = 0.0f, Orientation = 0.0f }), //different position for each race?
        (Race.Human, _) => (0, 12, new Position() { X = -8949.95f, Y = -132.493f, Z = 83.5312f, Orientation = 0.0f }),
        (Race.Orc, _) or (Race.Troll, _) => (1, 14, new Position() { X = -618.518f, Y = -4251.67f, Z = 38.718f, Orientation = 0.0f }),
        (Race.Dwarf, _) => (0, 1, new Position() { X = -6240.32f, Y = 331.033f, Z = 382.758f, Orientation = 6.17716f }),
        (Race.NightElf, _) => (1, 141, new Position() { X = 10311.3f, Y = 831.463f, Z = 1326.41f, Orientation = 5.48033f }),
        (Race.Undead, _) => (0, 85, new Position() { X = 1676.35f, Y = 1677.45f, Z = 121.67f, Orientation = 2.70526f }),
        (Race.Tauren, _) => (1, 215, new Position() { X = -2917.58f, Y = -257.98f, Z = 52.9968f, Orientation = 0.0f }),
        (Race.Gnome, _) => (0, 1, new Position() { X = -6240.32f, Y = 331.033f, Z = 382.758f, Orientation = 0.0f }),
        (Race.BloodElf, _) => (530, 3431, new Position() { X = 10349.6f, Y = -6357.29f, Z = 33.4026f, Orientation = 5.31605f }),
        (Race.Draenei, _) => (530, 3526, new Position() { X = -3961.64f, Y = -13931.2f, Z = 100.615f, Orientation = 2.08364f }),
        _ => (0, 0, default(Position)),
    };
}
