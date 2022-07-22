using Microsoft.Data.SqlClient;

namespace Shared.Database;

public abstract class SqlServerDatabase : IDatabase
{
    private readonly string _connectionString;

    protected SqlServerDatabase(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void ExecuteNonQuery(string statement, in KeyValuePair<string, object>[] parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        SqlCommand cmd = PrepareQuery(statement, connection, parameters);
        cmd.ExecuteNonQuery();
    }

    public (TOut1, TOut2) ExecuteSingleRaw<TOut1, TOut2>(string statement, in KeyValuePair<string, object>[] parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        SqlCommand cmd = PrepareQuery(statement, connection, parameters);
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

    public IEnumerable<object[]> ExecuteMultipleRaws(string statement, in KeyValuePair<string, object>[] parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        SqlCommand cmd = PrepareQuery(statement, connection, parameters);
        var reader = cmd.ExecuteReader();
        return IterateThroughResults(reader).ToList();
    }

    private IEnumerable<object[]> IterateThroughResults(SqlDataReader reader)
    {
        if (!reader.HasRows)
            yield break;
        object[] result = new object[reader.FieldCount];
        while (reader.Read())
        {
            reader.GetValues(result);
            yield return result;
        }
        reader.Close();
    }

    public TOut ExecuteSingleValue<TOut>(string statement, in KeyValuePair<string, object>[] parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        SqlCommand cmd = PrepareQuery(statement, connection, parameters);
        return (TOut)cmd.ExecuteScalar();
    }

    private SqlCommand PrepareQuery(string statement, SqlConnection conn, in KeyValuePair<string, object>[] parameters)
    {
        conn.Open();
        SqlCommand cmd = new(statement, conn);
        if (parameters is null)
            return cmd;
        foreach (var parameter in parameters)
            cmd.Parameters.AddWithValue(parameter.Key, parameter.Value);
        return cmd;
    }

    public (TOut1, TOut2, TOut3) ExecuteSingleRaw<TOut1, TOut2, TOut3>(string statement, in KeyValuePair<string, object>[] parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        SqlCommand cmd = PrepareQuery(statement, connection, parameters);
        var reader = cmd.ExecuteReader();
        if (reader.HasRows)
        {
            reader.Read();
            (TOut1, TOut2, TOut3) result = (reader.GetFieldValue<TOut1>(0),
                reader.GetFieldValue<TOut2>(1),
                reader.GetFieldValue<TOut3>(2));
            reader.Close();
            return result;
        }
        return (default(TOut1), default(TOut2), default(TOut3));
    }

    public (TOut1, TOut2, TOut3, TOut4) ExecuteSingleRaw<TOut1, TOut2, TOut3, TOut4>(string statement, in KeyValuePair<string, object>[] parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        SqlCommand cmd = PrepareQuery(statement, connection, parameters);
        var reader = cmd.ExecuteReader();
        if (reader.HasRows)
        {
            reader.Read();
            (TOut1, TOut2, TOut3, TOut4) result = (reader.GetFieldValue<TOut1>(0),
                reader.GetFieldValue<TOut2>(1),
                reader.GetFieldValue<TOut3>(2),
                reader.GetFieldValue<TOut4>(3));
            reader.Close();
            return result;
        }
        return (default(TOut1), default(TOut2), default(TOut3), default(TOut4));
    }

    public (TOut1, TOut2, TOut3, TOut4, TOut5) ExecuteSingleRaw<TOut1, TOut2, TOut3, TOut4, TOut5>(string statement, in KeyValuePair<string, object>[] parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        SqlCommand cmd = PrepareQuery(statement, connection, parameters);
        var reader = cmd.ExecuteReader();
        if (reader.HasRows)
        {
            reader.Read();
            (TOut1, TOut2, TOut3, TOut4, TOut5) result = (reader.GetFieldValue<TOut1>(0),
                reader.GetFieldValue<TOut2>(1),
                reader.GetFieldValue<TOut3>(2),
                reader.GetFieldValue<TOut4>(3),
                reader.GetFieldValue<TOut5>(4));
            reader.Close();
            return result;
        }
        return (default(TOut1), default(TOut2), default(TOut3), default(TOut4), default(TOut5));
    }

    public (TOut1, TOut2, TOut3, TOut4, TOut5, TOut6) ExecuteSingleRaw<TOut1, TOut2, TOut3, TOut4, TOut5, TOut6>(string statement, in KeyValuePair<string, object>[] parameters)
    {
        using var connection = new SqlConnection(_connectionString);
        SqlCommand cmd = PrepareQuery(statement, connection, parameters);
        var reader = cmd.ExecuteReader();
        if (reader.HasRows)
        {
            reader.Read();
            (TOut1, TOut2, TOut3, TOut4, TOut5, TOut6) result = (reader.GetFieldValue<TOut1>(0),
                reader.GetFieldValue<TOut2>(1),
                reader.GetFieldValue<TOut3>(2),
                reader.GetFieldValue<TOut4>(3),
                reader.GetFieldValue<TOut5>(4),
                reader.GetFieldValue<TOut6>(5));
            reader.Close();
            return result;
        }
        return (default(TOut1), default(TOut2), default(TOut3), default(TOut4), default(TOut5), default(TOut6));
    }
}
