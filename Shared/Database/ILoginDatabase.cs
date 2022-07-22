namespace Shared.Database;

public interface ILoginDatabase : IDatabase
{
    public string GetSessionKey { get; }
    public string GetUserAuthData { get; }
    public string SetSessionKey { get; }
    public string GetRealmList { get; }
    public string GetRealmInfo { get; }
    public string GetCharacterList { get; }
    public string GetCharacterInfo { get; }
}
