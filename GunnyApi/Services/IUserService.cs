using GunnyApi.Infrastructure.Services;
using GunnyApi.Models;

namespace GunnyApi.Services;

public interface IUserService : IBaseService<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetActiveUsersAsync();
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    Task<User?> GetCurrentUserAsync(int userId);
    Task<TransferMoneyResponse> TransferMoneyAsync(int userId, int amount);
    Task<SendMoneyResponse> SendMoneyAsync(string userName, int gold, int money, int giftToken);
    Task<ChangePasswordResponse> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<UpdateProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    
    // Password Reset methods
    Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<VerifyResetTokenResponse> VerifyResetTokenAsync(string token);
    Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request);
    
    // Email Change methods
    Task<(bool Success, string Message, string OtpCode)> CreateEmailChangeOtpAsync(int userId, string currentEmail, string newEmail, int step);
    Task<(bool Success, string Message, string NewEmail)> VerifyEmailChangeOtpAsync(int userId, string otpCode, int step);
    Task<bool> UpdateUserEmailAsync(int userId, string newEmail);
    Task<EmailChangeOtp?> GetLatestEmailChangeOtpAsync(int userId, int step);
    Task<bool> VerifyPasswordAsync(int userId, string password);
    Task MarkEmailAsVerifiedAsync(int userId);
}
