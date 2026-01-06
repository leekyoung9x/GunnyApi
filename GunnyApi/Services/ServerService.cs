using GunnyApi.Models;
using GunnyApi.Repositories;

namespace GunnyApi.Services;

public class ServerService : IServerService
{
    private readonly IServerRepository _serverRepository;

    public ServerService(IServerRepository serverRepository)
    {
        _serverRepository = serverRepository;
    }

    public async Task<ServerListResponse> GetServerListAsync(int version)
    {
        var servers = await _serverRepository.GetServerListAsync();
        
        return new ServerListResponse
        {
            Servers = servers
        };
    }
}
