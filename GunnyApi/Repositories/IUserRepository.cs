using GunnyApi.Infrastructure.Repositories;
using GunnyApi.Models;

namespace GunnyApi.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
}
