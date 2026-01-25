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
    Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword);
    Task<bool> UpdateUsernameAsync(int userId, string oldUsername, string newUsername);
    Task<bool> UpdateNicknameAsync(string username, string newNickname);
    Task<bool> CheckUsernameExistsExcludingUserAsync(int userId, string username);
    Task<bool> CheckNicknameExistsExcludingUserAsync(string currentUsername, string nickname);
    
    // Password Reset methods
    Task<User?> GetByEmailAsync(string email);
    Task<int> CreatePasswordResetTokenAsync(int userId, string email, string token, DateTime expiresAt);
    Task<PasswordReset?> VerifyPasswordResetTokenAsync(string token);
    Task<bool> MarkTokenAsUsedAsync(string token);
    Task<bool> ResetPasswordWithTokenAsync(string token, string newPassword);
}
