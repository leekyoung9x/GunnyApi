using GunnyApi.Infrastructure.Services;
using GunnyApi.Models;

namespace GunnyApi.Services;

public interface IUserService : IBaseService<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<User?> GetCurrentUserAsync(int userId);
}
