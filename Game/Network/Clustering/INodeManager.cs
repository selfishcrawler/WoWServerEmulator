namespace Game.Network.Clustering;

public interface INodeManager
{
    public void AddSession(WorldSession session);
    public void EnterWorld(WorldSession session, int map, ulong characterId);
}