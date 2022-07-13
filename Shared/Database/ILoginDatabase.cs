namespace Shared.Database;

public interface ILoginDatabase : IDatabase
{
    public string GetSessionKey { get; }
    public string GetUserAuthData { get; }
    public string SetSessionKey { get; }
}
