namespace Shared.Database;

public class SqlServerLoginDatabase : SqlServerDatabase, ILoginDatabase
{
    public SqlServerLoginDatabase(string connectionString, string dbName) : base(connectionString, dbName)
    {
    }

    public string GetSessionKey => "SELECT [SessionKey] FROM [Accounts] WHERE [Name]=@Name;";
    public string GetUserAuthData => "SELECT [Verifier], [Salt] From [Accounts] WHERE [Name]=@Name;";
    public string SetSessionKey => "UPDATE [Accounts] SET [SessionKey]=@SessionKey WHERE [Name]=@Name;";

}
