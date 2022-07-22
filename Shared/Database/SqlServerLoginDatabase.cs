namespace Shared.Database;

public sealed class SqlServerLoginDatabase : SqlServerDatabase, ILoginDatabase
{
    public SqlServerLoginDatabase(string connectionString) : base(connectionString)
    {
    }

    public string GetSessionKey => "SELECT [SessionKey] FROM [Accounts] WHERE [Name]=@Name;";
    public string GetUserAuthData => "SELECT [Verifier], [Salt] From [Accounts] WHERE [Name]=@Name;";
    public string SetSessionKey => "UPDATE [Accounts] SET [SessionKey]=@SessionKey WHERE [Name]=@Name;";
    public string GetRealmList => "SELECT * FROM [Realmlist];";
    public string GetRealmInfo => "SELECT [Name], [RealmType], [RealmFlags], [Address], [Port], [Timezone] FROM [Realmlist] WHERE [Id]=@Id";
    public string GetCharacterList => throw new NotImplementedException();
    public string GetCharacterInfo => throw new NotImplementedException();
}
