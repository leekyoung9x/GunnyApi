using GunnyApi.Infrastructure.Repositories;
using GunnyApi.Models;

namespace GunnyApi.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<int?> LoginAsync(string applicationName, string userName, string password);
    Task<TransferMoneyResponse> TransferMoneyAsync(int userId, int amount);
    Task<PlayerInfo?> GetPlayerByNickNameAsync(string nickName);
    Task<bool> SendMailAsync(MailInfo mail);
}
