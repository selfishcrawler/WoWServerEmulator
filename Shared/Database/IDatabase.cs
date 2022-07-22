namespace Shared.Database;

public interface IDatabase
{
    public void ExecuteNonQuery(string statement, in KeyValuePair<string, object>[] parameters);
    public TOut ExecuteSingleValue<TOut>(string statement, in KeyValuePair<string, object>[] parameters);
    public (TOut1, TOut2) ExecuteSingleRaw<TOut1, TOut2>(string statement, in KeyValuePair<string, object>[] parameters);
    public (TOut1, TOut2, TOut3) ExecuteSingleRaw<TOut1, TOut2, TOut3>(string statement, in KeyValuePair<string, object>[] parameters);
    public (TOut1, TOut2, TOut3, TOut4) ExecuteSingleRaw<TOut1, TOut2, TOut3, TOut4>(string statement, in KeyValuePair<string, object>[] parameters);
    public (TOut1, TOut2, TOut3, TOut4, TOut5) ExecuteSingleRaw<TOut1, TOut2, TOut3, TOut4, TOut5>(string statement, in KeyValuePair<string, object>[] parameters);
    public (TOut1, TOut2, TOut3, TOut4, TOut5, TOut6) ExecuteSingleRaw<TOut1, TOut2, TOut3, TOut4, TOut5, TOut6>(string statement, in KeyValuePair<string, object>[] parameters);
    public IEnumerable<object[]> ExecuteMultipleRaws(string statement, in KeyValuePair<string, object>[] parameters);
}