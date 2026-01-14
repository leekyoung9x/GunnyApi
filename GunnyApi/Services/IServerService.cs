using GunnyApi.Models;

namespace GunnyApi.Services
{
    public interface IServerService
    {
        Task<ServerListResponse> GetServerListAsync(int version);
    }
}
