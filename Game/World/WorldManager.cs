using Game.Entities;
using Game.Network;
using Game.Network.Clustering;
using Shared.RealmInfo;

namespace Game.World;

public static class WorldManager
{
    private static List<WorldSession> _sessions;
    public static INodeManager NodeManager { get; private set; }
    public static byte RealmID { get; private set; }
    public static RealmTimeZone RealmTimeZone { get; private set; }
    public static RealmType RealmType { get; private set; }
    public const int StartingLevel = 1;
    public const int DKStartingLevel = 55;
    static Creature c;

    public static void InitWorld(byte realmID, INodeManager nodeManager)
    {
        _sessions = new List<WorldSession>(100);
        RealmID = realmID;
        NodeManager = nodeManager;
    }

    public static void InitCreatures()
    {
        c = new Creature(1)
        {
            Entry = 1,
            Position = new Position() { X = -8949.95f, Y = -132.493f, Z = 83.5312f, Orientation = 0.0f },
            Alive = true,
            Name = "TestUnit",
            CurrentHealth = 50,
            MaxHealth = 150,
            Level = 5,
            DisplayID = 23006,
            NativeDisplayID = 23006,
            Race = Race.Human,
            Class = Class.Warrior,
            Gender = Gender.Male,
            PowerType = PowerType.Mana,
            Faction = 14,
        };
        c.SetCurrentPower(PowerType.Mana, 1);
        c.SetCurrentPower(PowerType.Mana, 100);
    }

    public static void AddSession(WorldSession session)
    {
        _sessions.Add(session);
        NodeManager.AddSession(session);
    }

    public static void RemoveSession(WorldSession session)
    {
        _sessions.Remove(session);
        NodeManager.RemoveSession(session);
    }

    public static void AddPlayerToWorld(WorldSession self)
    {
        Player addingPlayer = self.ActiveCharacter;
        using MemoryStream broadcast = new(400);
        using MemoryStream surroundingPlayers = new(400);
        broadcast.Write((uint)1);
        addingPlayer.BuildCreatePacket(broadcast, false);
        foreach (var session in _sessions)
        {
            if (session == self)
                continue;

            //check distance
            surroundingPlayers.Write((uint)1);
            session.ActiveCharacter.BuildCreatePacket(surroundingPlayers, false);
            self.SendPacket(surroundingPlayers, Opcode.SMSG_UPDATE_OBJECT);
            session.SendPacket(broadcast, Opcode.SMSG_UPDATE_OBJECT);
        }

        using MemoryStream units = new();
        units.Write((uint)1);
        c.BuildCreatePacket(units);
        self.SendPacket(units, Opcode.SMSG_UPDATE_OBJECT);
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

    public static uint GetCharacterDisplayId(Race race, Gender gender) => (race, gender) switch
    {
        (Race.Human, Gender.Male) => 49,
        (Race.Human, Gender.Female) => 50,
        (Race.Orc, Gender.Male) => 51,
        (Race.Orc, Gender.Female) => 52,
        (Race.Dwarf, Gender.Male) => 53,
        (Race.Dwarf, Gender.Female) => 54,
        (Race.NightElf, Gender.Male) => 55,
        (Race.NightElf, Gender.Female) => 56,
        (Race.Undead, Gender.Male) => 57,
        (Race.Undead, Gender.Female) => 58,
        (Race.Tauren, Gender.Male) => 59,
        (Race.Tauren, Gender.Female) => 60,
        (Race.Gnome, Gender.Male) => 1563,
        (Race.Gnome, Gender.Female) => 1564,
        (Race.Troll, Gender.Male) => 1478,
        (Race.Troll, Gender.Female) => 1479,
        (Race.BloodElf, Gender.Male) => 15476,
        (Race.BloodElf, Gender.Female) => 15475,
        (Race.Draenei, Gender.Male) => 16125,
        (Race.Draenei, Gender.Female) => 16126,
        (_,_) => 0,
    };
}