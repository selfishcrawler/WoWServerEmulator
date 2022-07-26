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
}
