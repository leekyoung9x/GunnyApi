using GunnyApi.Infrastructure.Database;
using GunnyApi.Models;
using System.Data;

namespace GunnyApi.Repositories;

public class ServerRepository : IServerRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ServerRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<List<ServerInfo>> GetServerListAsync()
    {
        var query = @"
            SELECT 
                AreaID as AreaId,
                ServerName,
                IsNew,
                LoadGameUrl,
                ConfigUrl,
                RequestUrl
            FROM Server_List
            ORDER BY AreaID";

        using var connection = _connectionFactory.CreateConnection();
        using var command = connection.CreateCommand();
        command.CommandText = query;
        command.CommandType = CommandType.Text;

        await connection.OpenAsync();

        var servers = new List<ServerInfo>();
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            servers.Add(new ServerInfo
            {
                AreaId = reader["AreaId"]?.ToString() ?? string.Empty,
                ServerName = reader["ServerName"]?.ToString() ?? string.Empty,
                IsNew = reader["IsNew"] != DBNull.Value && Convert.ToBoolean(reader["IsNew"]),
                LoadGameUrl = reader["LoadGameUrl"]?.ToString() ?? string.Empty,
                ConfigUrl = reader["ConfigUrl"]?.ToString() ?? string.Empty,
                RequestUrl = reader["RequestUrl"]?.ToString() ?? string.Empty
            });
        }

        return servers;
    }
}
