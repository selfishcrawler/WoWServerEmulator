namespace Game.Network.Clustering;

public interface INodeManager
{
    public void AddSession(WorldSession session);
    public void RemoveSession(WorldSession session);
    public void EnterWorld(WorldSession session, int map, ulong characterId);
    public void Logout(WorldSession session);
    public void RedirectionFailed(WorldSession session);
}