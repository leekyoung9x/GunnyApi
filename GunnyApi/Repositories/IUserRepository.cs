using GunnyApi.Infrastructure.Repositories;
using GunnyApi.Models;

namespace GunnyApi.Repositories;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<bool> UsernameExistsAsync(string username);
    Task<bool> EmailExistsAsync(string email);
    Task<bool> NicknameExistsAsync(string nickname);
    Task<int?> LoginAsync(string applicationName, string userName, string password);
    Task<int?> RegisterUserAsync(string username, string password, string fullname);
    Task<bool> CallInsertUserDetailProcAsync(int userId, string username, string nickname, int exp, int gold, int money, bool sex);
    Task<TransferMoneyResponse> TransferMoneyAsync(int userId, int amount);
    Task<PlayerInfo?> GetPlayerByNickNameAsync(string nickName);
    Task<PlayerInfo?> GetPlayerByUserNameAsync(string userName);
    Task<bool> SendMailAsync(MailInfo mail);
}
