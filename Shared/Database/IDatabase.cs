namespace Shared.Database;

public interface IDatabase
{
    public void ExecuteNonQuery(string statement, Dictionary<string, object> parameter);
    public TOut ExecuteSingleValue<TOut>(string statement, Dictionary<string, object> parameter);
    public (TOut1, TOut2) ExecuteSingleRaw<TOut1, TOut2>(string statement, Dictionary<string, object> parameter);
}