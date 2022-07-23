namespace Shared.Database;

public sealed class SqlServerLoginDatabase : SqlServerDatabase, ILoginDatabase
{
    public SqlServerLoginDatabase(string connectionString) : base(connectionString)
    {
    }

    public string GetSessionKey => "SELECT [SessionKey] FROM [Accounts] WHERE [Name]=@Name;";
    public string GetUserAuthData => "SELECT [Verifier], [Salt] From [Accounts] WHERE [Name]=@Name;";
    public string SetSessionKey => "UPDATE [Accounts] SET [SessionKey]=@SessionKey WHERE [Name]=@Name;";
    public string GetAccountInfoByUsername => "SELECT [Id], [SessionKey] FROM [Accounts] WHERE [Name]=@Name";
    public string GetRealmList => "SELECT * FROM [Realmlist];";
    public string GetRealmInfo => "SELECT [Name], [RealmType], [RealmFlags], [Address], [Port], [Timezone] FROM [Realmlist] WHERE [Id]=@Id";
    public string GetCharacterList => "SELECT [Guid], [Name], [Race], [Class], [Gender]," +
        "[Skin], [Face], [HairStyle], [HairColor], [FacialStyle]," +
        "[Level], [Zone], [Map], [X], [Y], [Z] FROM [Characters] WHERE [Account]=@Account AND [Realm]=@Realm;";
    public string GetCharacterInfo => throw new NotImplementedException();
}
