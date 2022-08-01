namespace Shared.Database;

public interface IClusterDatabase : IDatabase
{
    public string GetClusterConfiguration { get; }
    public string GetNodeEndpoint { get; }
    public string GetNodeEndpointsForRedirection { get; }
    public string GetNodeMappings { get; }
}
