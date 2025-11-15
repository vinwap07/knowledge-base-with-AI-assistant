using System.Data;
using Npgsql;

namespace knowledgeBase.DataBase;

public class PostgresDbConnection : IDatabaseConnection, IDisposable
{
    private readonly string _connectionString;
    private bool _disposed = false;

    public PostgresDbConnection(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public Task OpenAsync()
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _disposed = true;
    }

    public async Task<IDataReader> ExecuteReader(string sql, Dictionary<string, object>? parameters = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PostgresDbConnection));

        var connection = new NpgsqlConnection(_connectionString);
        try
        {
            await connection.OpenAsync();
            var command = CreateCommand(sql, parameters, connection);
            try
            {
                return await command.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            }
            catch
            {
                command.Dispose();
                throw;
            }
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    public async Task<int> ExecuteNonQuery(string sql, Dictionary<string, object>? parameters = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PostgresDbConnection));

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = CreateCommand(sql, parameters, connection);
        return await command.ExecuteNonQueryAsync();
    }

    public async Task<object?> ExecuteScalar(string sql, Dictionary<string, object>? parameters = null)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(PostgresDbConnection));

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        
        await using var command = CreateCommand(sql, parameters, connection);
        return await command.ExecuteScalarAsync();
    }

    private NpgsqlCommand CreateCommand(string sql, Dictionary<string, object>? parameters, NpgsqlConnection connection)
    {
        var cmd = new NpgsqlCommand(sql, connection);
        
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                cmd.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
        }
        
        return cmd;
    }
}