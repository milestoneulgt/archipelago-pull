using Microsoft.Data.SqlClient;
using ulgtArchipelagoPull.Interfaces;

namespace ulgtArchipelagoPull.Services;

public class SqlConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    private readonly string _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

    public SqlConnection CreateConnection(string? connectionString)
    {
        return new SqlConnection(connectionString ?? _connectionString);
    }
}