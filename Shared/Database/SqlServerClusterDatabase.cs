namespace Shared.Database;

public sealed class SqlServerClusterDatabase : SqlServerDatabase, IClusterDatabase
{
    public SqlServerClusterDatabase(string connectionString) : base(connectionString)
    {
    }

    public string GetClusterConfiguration => "SELECT [IntercomIP], [IntercomPort] From [ClusterConfiguration];";
    public string GetNodeEndpoint => "SELECT [IP], [Port] FROM [Nodes] WHERE [NodeID]=@ID;";
    public string GetNodeEndpoints => "SELECT [NodeID], [IP], [PORT] FROM [Nodes];";
    public string GetNodeMappings => "SELECT [MapID], [NodeID] FROM [NodesMapping];";
}
