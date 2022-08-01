namespace Shared.Database;

public sealed class SqlServerClusterDatabase : SqlServerDatabase, IClusterDatabase
{
    public SqlServerClusterDatabase(string connectionString) : base(connectionString)
    {
    }
}
