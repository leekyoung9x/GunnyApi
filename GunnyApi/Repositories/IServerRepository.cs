using GunnyApi.Models;

namespace GunnyApi.Repositories
{
    public interface IServerRepository
    {
        Task<List<ServerInfo>> GetServerListAsync();
    }
}
