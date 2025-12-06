using Microsoft.Data.SqlClient;

namespace GunnyApi.Infrastructure.Database;

public interface IDbConnectionFactory
{
    SqlConnection CreateConnection();
}
