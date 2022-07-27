namespace Shared.Database;

public sealed class SqlServerLoginDatabase : SqlServerDatabase, ILoginDatabase
{
    public SqlServerLoginDatabase(string connectionString) : base(connectionString)
    {
    }

    public string CreateAccount => "INSERT INTO [Accounts] ([Name], [Verifier], [Salt], [SessionKey]) VALUES (@Username, @Verifier, @Salt, NULL);";
    public string GetSessionKey => "SELECT [SessionKey] FROM [Accounts] WHERE [Name]=@Name;";
    public string GetUserAuthData => "SELECT [Verifier], [Salt] From [Accounts] WHERE [Name]=@Name;";
    public string SetSessionKey => "UPDATE [Accounts] SET [SessionKey]=@SessionKey WHERE [Name]=@Name;";
    public string GetAccountInfoByUsername => "SELECT [Id], [SessionKey] FROM [Accounts] WHERE [Name]=@Name";
    public string GetRealmList => "SELECT [Id], [Name], [RealmType], [Locked], [RealmFlags], [Address], [Port], [Population], [Timezone], [Build] FROM [Realmlist];";
    public string GetRealmInfo => "SELECT [Name], [RealmType], [RealmFlags], [ListenAddress], [ListenPort], [Timezone] FROM [Realmlist] WHERE [Id]=@Id";
    public string GetCharacterCreationInfo => "WITH [UserChars] AS (SELECT * FROM [Characters] WHERE [Account] = @Account AND [Realm]=@Realm) SELECT" +
        "(SELECT CASE WHEN EXISTS(SELECT TOP 1 1 FROM [Characters] WHERE [Name]=@Name) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END)," +
        "(SELECT COUNT(*) FROM [UserChars])," +
        "(SELECT CASE WHEN EXISTS(SELECT TOP 1 1 FROM [UserChars] WHERE [Level] >= 55) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END)," +
        "(SELECT CASE WHEN EXISTS(SELECT TOP 1 1 FROM [UserChars] WHERE [Class] = 6) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END);";
    public string CreateCharacter => "INSERT INTO [Characters] ([Account], [Realm], [Name], [Level], [Race], [Class], [Gender]," +
        "[Map], [Zone], [X], [Y], [Z], [Orientation], [Skin], [Face], [HairStyle], [HairColor], [FacialStyle]) VALUES " +
        "(@Account, @Realm, @Name, @Level, @Race, @Class, @Gender, @Map, @Zone, @X, @Y, @Z, @Orientation, @Skin, @Face, @HairStyle, @HairColor, @FacialStyle);";
    public string GetCharacterList => "SELECT [Guid], [Name], [Race], [Class], [Gender]," +
        "[Skin], [Face], [HairStyle], [HairColor], [FacialStyle]," +
        "[Level], [Zone], [Map], [X], [Y], [Z] FROM [Characters] WHERE [Account]=@Account AND [Realm]=@Realm;";
    public string GetCharacterInfo => "SELECT [Name], [Level], [Race], [Class], [Gender], [Map], [Zone], [X], [Y], [Z], [Orientation]," +
        "[Skin], [Face], [HairStyle], [HairColor], [FacialStyle] FROM [Characters] WHERE [Guid]=@Guid;";
}