using Microsoft.Data.SqlClient;

namespace GunnyApi.Infrastructure.Database;

public class TankDbConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public TankDbConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("TankConnection")
            ?? throw new InvalidOperationException("Connection string 'TankConnection' not found.");
    }

    public SqlConnection CreateConnection()
    {
        return new SqlConnection(_connectionString);
    }
}
