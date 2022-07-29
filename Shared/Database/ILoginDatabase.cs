namespace Shared.Database;

public interface ILoginDatabase : IDatabase
{
    public string CreateAccount { get; }
    public string GetSessionKey { get; }
    public string GetUserAuthData { get; }
    public string SetSessionKey { get; }
    public string GetAccountInfoByUsername { get; }
    public string GetRealmList { get; }
    public string GetRealmInfo { get; }
    public string GetCharacterCreationInfo { get; }
    public string CreateCharacter { get; }
    public string GetCharacterList { get; }
    public string GetCharacterMap { get; }
    public string GetCharacterInfo { get; }
}
