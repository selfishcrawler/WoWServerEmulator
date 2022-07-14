namespace Shared.Database;

public interface IDatabase
{
    public void ExecuteNonQuery(string statement, in KeyValuePair<string, object>[] parameters);
    public TOut ExecuteSingleValue<TOut>(string statement, in KeyValuePair<string, object>[] parameters);
    public (TOut1, TOut2) ExecuteSingleRaw<TOut1, TOut2>(string statement, in KeyValuePair<string, object>[] parameters);
}