namespace Shared.Database;

public sealed class SqlServerClusterDatabase : SqlServerDatabase, IClusterDatabase
{
    public SqlServerClusterDatabase(string connectionString) : base(connectionString)
    {
    }

    public string GetClusterConfiguration => "SELECT [IntercomIP], [IntercomPort] From [ClusterConfiguration];";
    public string GetNodeEndpoint => "SELECT [ListenIP], [ListenPort] FROM [Nodes] WHERE [NodeID]=@ID;";
    public string GetNodeEndpointsForRedirection => "SELECT [NodeID], [IP], [PORT] FROM [Nodes];";
    public string GetNodeMappings => "SELECT [MapID], [NodeID] FROM [NodesMapping];";
}
