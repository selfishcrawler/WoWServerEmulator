using Microsoft.Data.SqlClient;

namespace Shared.Database;

public abstract class SqlServerDatabase : IDatabase
{
    private readonly SqlConnection _connection;

    public SqlServerDatabase(string connectionString, string dbName)
    {
        _connection = new SqlConnection(connectionString);
        _connection.Open();
        if (!string.IsNullOrEmpty(dbName))
            _connection.ChangeDatabase(dbName);
    }

    public void ExecuteNonQuery(string statement, Dictionary<string, object> parameters)
    {
        SqlCommand cmd = PrepareQuery(statement, parameters);
        cmd.ExecuteNonQuery();
    }

    public (TOut1, TOut2) ExecuteSingleRaw<TOut1, TOut2>(string statement, Dictionary<string, object> parameters)
    {
        SqlCommand cmd = PrepareQuery(statement, parameters);
        var reader = cmd.ExecuteReader();
        if (reader.HasRows)
        {
            reader.Read();
            (TOut1, TOut2) result = (reader.GetFieldValue<TOut1>(0), reader.GetFieldValue<TOut2>(1));
            reader.Close();
            return result;
        }
        return (default(TOut1), default(TOut2));
    }

    public TOut ExecuteSingleValue<TOut>(string statement, Dictionary<string, object> parameters)
    {
        SqlCommand cmd = PrepareQuery(statement, parameters);
        return (TOut)cmd.ExecuteScalar();
    }

    private SqlCommand PrepareQuery(string statement, Dictionary<string, object> parameters)
    {
        SqlCommand cmd = new(statement, _connection);
        foreach (var parameter in parameters)
            cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
        return cmd;
    }
}
