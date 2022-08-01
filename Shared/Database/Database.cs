namespace Shared.Database;

public static class Database
{
    public static ILoginDatabase Login { get; set; }
    public static IClusterDatabase Cluster { get; set; }
}
