using Microsoft.Data.SqlClient;

namespace ulgtArchipelagoPull.Interfaces;

public interface ISqlConnectionFactory
{
    SqlConnection CreateConnection(string? connectionString);
}