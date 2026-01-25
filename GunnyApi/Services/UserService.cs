using GunnyApi.Infrastructure.Repositories;
using GunnyApi.Infrastructure.Security;
using GunnyApi.Infrastructure.Services;
using GunnyApi.Models;
using GunnyApi.Repositories;
using CenterService.Client.Factory;

namespace GunnyApi.Services;

public class UserService : BaseService<User>, IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;
    private readonly ILocalizationService _localization;

    public UserService(IUserRepository userRepository, IJwtTokenService jwtTokenService, IConfiguration configuration, ILocalizationService localization)
        : base(userRepository)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
        _localization = localization;
    }

    protected override async Task ValidateEntityAsync(User entity)
    {
        // Validate các trường bắt buộc
        if (string.IsNullOrWhiteSpace(entity.Username))
        {
            throw new ArgumentException(_localization.GetString("User.UsernameRequired"), nameof(entity.Username));
        }

        if (string.IsNullOrWhiteSpace(entity.Email))
        {
            throw new ArgumentException(_localization.GetString("User.EmailRequired"), nameof(entity.Email));
        }

        // Validate SQL injection
        SqlInjectionProtection.ValidateInputs(
            (entity.Username, nameof(entity.Username)),
            (entity.Email, nameof(entity.Email)),
            (entity.FullName, nameof(entity.FullName))
        );

        // Kiểm tra username đã tồn tại chưa (khi tạo mới)
        if (entity.Id == 0 && await _userRepository.UsernameExistsAsync(entity.Username))
        {
            throw new InvalidOperationException(_localization.GetString("User.UsernameExists", entity.Username));
        }

        // Kiểm tra email đã tồn tại chưa (khi tạo mới)
        if (entity.Id == 0 && await _userRepository.EmailExistsAsync(entity.Email))
        {
            throw new InvalidOperationException(_localization.GetString("User.EmailExists", entity.Email));
        }

        await base.ValidateEntityAsync(entity);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException(_localization.GetString("User.UsernameRequired"), nameof(username));
        }

        // Validate SQL injection
        SqlInjectionProtection.ValidateInput(username, nameof(username));

        return await _userRepository.GetByUsernameAsync(username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ArgumentException(_localization.GetString("User.EmailRequired"), nameof(email));
        }

        // Validate SQL injection
        SqlInjectionProtection.ValidateInput(email, nameof(email));

        return await _userRepository.GetByEmailAsync(email);
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        return await _userRepository.GetActiveUsersAsync();
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        // Set default ApplicationName if not provided
        if (string.IsNullOrWhiteSpace(request.ApplicationName))
        {
            request.ApplicationName = "DanDanTang";
        }

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return new LoginResponse
            {
                Success = false,
                Message = _localization.GetString("Login.UsernameRequired")
            };
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return new LoginResponse
            {
                Success = false,
                Message = _localization.GetString("Login.PasswordRequired")
            };
        }

        try
        {
            // Validate SQL injection
            SqlInjectionProtection.ValidateInputs(
                (request.ApplicationName, nameof(request.ApplicationName)),
                (request.UserName, nameof(request.UserName)),
                (request.Password, nameof(request.Password))
            );

            var userId = await _userRepository.LoginAsync(
                request.ApplicationName,
                request.UserName,
                request.Password
            );

            if (userId.HasValue && userId.Value > 0)
            {
                // Lấy thông tin user để generate token
                var user = await _userRepository.GetByIdAsync(userId.Value);
                if (user == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = _localization.GetString("User.NotFound")
                    };
                }

                user.Id = user.Id == 0 ? userId.Value : user.Id;
                user.Username = user.Email;

                // Generate JWT token
                var token = _jwtTokenService.GenerateToken(user);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();

                return new LoginResponse
                {
                    Success = true,
                    UserId = userId.Value,
                    Token = token,
                    RefreshToken = refreshToken,
                    Message = _localization.GetString("Login.Success")
                };
            }
            else
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Tên đăng nhập hoặc mật khẩu không đúng"
                };
            }
        }
        catch (SecurityException ex)
        {
            return new LoginResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
        catch (Exception ex)
        {
            return new LoginResponse
            {
                Success = false,
                Message = _localization.GetString("Login.Failed", ex.Message)
            };
        }
    }

    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Username))
        {
            return new RegisterResponse
            {
                Success = false,
                Message = _localization.GetString("User.UsernameRequired")
            };
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return new RegisterResponse
            {
                Success = false,
                Message = _localization.GetString("User.PasswordRequired")
            };
        }

        if (string.IsNullOrWhiteSpace(request.Nickname))
        {
            return new RegisterResponse
            {
                Success = false,
                Message = "Nickname không được rỗng"
            };
        }

        try
        {
            // Validate SQL injection
            SqlInjectionProtection.ValidateInputs(
                (request.Username, nameof(request.Username)),
                (request.Password, nameof(request.Password)),
                (request.Nickname, nameof(request.Nickname))
            );

            // B1: Thêm dữ liệu vào Mem_Account và lấy UserID
            var userId = await _userRepository.RegisterUserAsync(
                request.Username,
                request.Password,
                request.Nickname
            );

            if (!userId.HasValue || userId.Value <= 0)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = _localization.GetString("Register.EmailExistsOrError")
                };
            }

            // Lấy config cho exp, gold, money từ GameSettings
            var exp = _configuration.GetValue<int>("GameSettings:NewUserExp", 0);
            var gold = _configuration.GetValue<int>("GameSettings:NewUserGold", 0);
            var money = _configuration.GetValue<int>("GameSettings:NewUserMoney", 0);

            // B2: Gọi stored procedure Proc_InsertNewUserDetail trên TankConnection
            var insertDetailResult = await _userRepository.CallInsertUserDetailProcAsync(
                userId.Value,
                request.Username,
                request.Nickname,
                exp,
                gold,
                money,
                request.Sex
            );

            if (!insertDetailResult)
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = _localization.GetString("Register.AccountCreatedButDetailError"),
                    UserId = userId.Value
                };
            }

            return new RegisterResponse
            {
                Success = true,
                Message = _localization.GetString("Register.Success"),
                UserId = userId.Value
            };
        }
        catch (InvalidOperationException ex)
        {
            // Lỗi email hoặc nickname đã tồn tại
            return new RegisterResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
        catch (SecurityException ex)
        {
            // Lỗi SQL injection
            return new RegisterResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
        catch (Exception ex)
        {
            return new RegisterResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
    }

    public async Task<User?> GetCurrentUserAsync(int userId)
    {
        if (userId <= 0)
        {
            throw new ArgumentException("UserId không hợp lệ", nameof(userId));
        }

        var user = await _userRepository.GetByIdAsync(userId);

        // Không trả về password
        if (user != null)
        {
            user.Password = string.Empty;

            // Lấy thêm nickname từ bảng Sys_Users_Detail trong TankConnection
            // Username trong Mem_Account chính là email, dùng nó để query PlayerInfo
            try
            {
                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    var playerInfo = await _userRepository.GetPlayerByUserNameAsync(user.Email);
                    if (playerInfo != null)
                    {
                        // Gán nickname vào FullName để trả về
                        user.NickName = playerInfo.NickName ?? string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error nhưng không fail, vẫn trả về user info cơ bản
                Console.WriteLine($"Warning: Could not get player nickname: {ex.Message}");
            }
        }

        return user;
    }

    public async Task<TransferMoneyResponse> TransferMoneyAsync(int userId, int amount)
    {
        if (userId <= 0)
        {
            return new TransferMoneyResponse
            {
                Success = false,
                Message = "UserId không hợp lệ"
            };
        }

        if (amount <= 0)
        {
            return new TransferMoneyResponse
            {
                Success = false,
                Message = "Số tiền phải lớn hơn 0"
            };
        }

        // Step 1: Subtract money from Mem_Account
        var transferResult = await _userRepository.TransferMoneyAsync(userId, amount);

        if (!transferResult.Success)
        {
            return transferResult;
        }

        // Step 2: Send money via mail to player in Tank database using email (email từ db member = username của db tank)
        var sendMoneyResult = await SendMoneyAsync(transferResult.UserEmail, 0, amount, 0);

        if (!sendMoneyResult.Success)
        {
            // Note: Money already deducted from Mem_Account
            // Consider implementing compensation logic or manual intervention
            return new TransferMoneyResponse
            {
                Success = false,
                Message = $"Đã trừ tiền từ Mem_Account nhưng không gửi được mail: {sendMoneyResult.Message}",
                RemainingMemberMoney = transferResult.RemainingMemberMoney
            };
        }

        return new TransferMoneyResponse
        {
            Success = true,
            Message = _localization.GetString("Money.TransferSuccess", amount, sendMoneyResult.Message),
            RemainingMemberMoney = transferResult.RemainingMemberMoney
        };
    }

    /// <summary>
    /// Send money, gold, and gift tokens to a user
    /// </summary>
    public async Task<SendMoneyResponse> SendMoneyAsync(string userName, int gold, int money, int giftToken)
    {
        try
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(userName))
            {
                return new SendMoneyResponse
                {
                    Success = false,
                    Message = "Tên tài khoản không được để trống"
                };
            }

            // Validate amounts
            if (gold < 0 || money < 0 || giftToken < 0)
            {
                return new SendMoneyResponse
                {
                    Success = false,
                    Message = "Số tiền, vàng, lễ kim phải lớn hơn hoặc bằng 0"
                };
            }

            // Get player by username (email) from Tank database using SP_Users_SingleByUserName
            var player = await _userRepository.GetPlayerByUserNameAsync(userName);

            if (player == null)
            {
                return new SendMoneyResponse
                {
                    Success = false,
                    Message = _localization.GetString("User.AccountNotFound", userName)
                };
            }

            // Create mail info
            var mailInfo = new MailInfo
            {
                Title = "Send Money, Gold, Gift Token",
                Content = $"Bạn nhận được {money} Xu, {gold} Vàng, {giftToken} Lễ kim",
                ReceiverID = player.UserID,
                Sender = "Administrators",
                SenderID = 0,
                Gold = gold,
                Money = money,
                GiftToken = giftToken,
                Type = 0
            };

            // Send mail to database
            var mailSent = await _userRepository.SendMailAsync(mailInfo);

            if (!mailSent)
            {
                return new SendMoneyResponse
                {
                    Success = false,
                    Message = "Không thể gửi mail trong hệ thống"
                };
            }

            // Send notification via CenterService
            bool mailNoticeResult = false;
            string mailNoticeMessage = "";
            try
            {
                var centerServiceAddress = _configuration["CenterServiceSettings:ServerAddress"]
                    ?? "net.tcp://127.0.0.1:2109/";

                using var client = CenterServiceClientFactory.CreateNetTcpClient(centerServiceAddress);
                mailNoticeResult = client.MailNotice(player.UserID);
                mailNoticeMessage = mailNoticeResult ? " (Đã gửi thông báo realtime)" : " (Không gửi được thông báo realtime)";
            }
            catch (Exception ex)
            {
                // Log error but don't fail the operation
                Console.WriteLine($"Warning: Could not send mail notification via CenterService: {ex.Message}");
                mailNoticeMessage = _localization.GetString("Money.MailError", ex.Message);
            }

            return new SendMoneyResponse
            {
                Success = true,
                Message = _localization.GetString("Money.TransferSuccessDetail", money, gold, giftToken, userName, mailNoticeMessage)
            };
        }
        catch (Exception ex)
        {
            return new SendMoneyResponse
            {
                Success = false,
                Message = _localization.GetString("Money.ErrorOccurred", ex.Message)
            };
        }
    }

    public async Task<ChangePasswordResponse> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.OldPassword))
        {
            return new ChangePasswordResponse
            {
                Success = false,
                Message = _localization.GetString("Password.OldPasswordRequired")
            };
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return new ChangePasswordResponse
            {
                Success = false,
                Message = _localization.GetString("Password.NewPasswordRequired")
            };
        }

        if (string.IsNullOrWhiteSpace(request.ConfirmPassword))
        {
            return new ChangePasswordResponse
            {
                Success = false,
                Message = _localization.GetString("Password.ConfirmPasswordRequired")
            };
        }

        // Check if new password and confirm password match
        if (request.NewPassword != request.ConfirmPassword)
        {
            return new ChangePasswordResponse
            {
                Success = false,
                Message = _localization.GetString("Password.PasswordsDoNotMatch")
            };
        }

        // Check if new password is same as old password
        if (request.OldPassword == request.NewPassword)
        {
            return new ChangePasswordResponse
            {
                Success = false,
                Message = _localization.GetString("Password.NewPasswordSameAsOld")
            };
        }

        try
        {
            // Validate SQL injection
            SqlInjectionProtection.ValidateInputs(
                (request.OldPassword, nameof(request.OldPassword)),
                (request.NewPassword, nameof(request.NewPassword)),
                (request.ConfirmPassword, nameof(request.ConfirmPassword))
            );

            var result = await _userRepository.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword);

            if (result)
            {
                return new ChangePasswordResponse
                {
                    Success = true,
                    Message = _localization.GetString("Password.ChangeSuccess")
                };
            }
            else
            {
                return new ChangePasswordResponse
                {
                    Success = false,
                    Message = _localization.GetString("Password.OldPasswordIncorrect")
                };
            }
        }
        catch (SecurityException ex)
        {
            return new ChangePasswordResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
        catch (Exception ex)
        {
            return new ChangePasswordResponse
            {
                Success = false,
                Message = _localization.GetString("Password.ChangeFailed", ex.Message)
            };
        }
    }

    public async Task<UpdateProfileResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(request.Username))
            {
                return new UpdateProfileResponse
                {
                    Success = false,
                    Message = _localization.GetString("Profile.UsernameRequired")
                };
            }

            if (string.IsNullOrWhiteSpace(request.Nickname))
            {
                return new UpdateProfileResponse
                {
                    Success = false,
                    Message = _localization.GetString("Profile.NicknameRequired")
                };
            }

            // Validate SQL injection
            SqlInjectionProtection.ValidateInputs(
                (request.Username, nameof(request.Username)),
                (request.Nickname, nameof(request.Nickname))
            );

            // Validate username (email format)
            //if (!IsValidEmail(request.Username))
            //{
            //    return new UpdateProfileResponse
            //    {
            //        Success = false,
            //        Message = _localization.GetString("Profile.InvalidEmailFormat")
            //    };
            //}

            // Validate username length (email: 5-100 characters)
            if (request.Username.Length < 5 || request.Username.Length > 100)
            {
                return new UpdateProfileResponse
                {
                    Success = false,
                    Message = _localization.GetString("Profile.UsernameLengthInvalid")
                };
            }

            // Validate nickname length (3-20 characters)
            if (request.Nickname.Length < 3 || request.Nickname.Length > 20)
            {
                return new UpdateProfileResponse
                {
                    Success = false,
                    Message = _localization.GetString("Profile.NicknameLengthInvalid")
                };
            }

            // Validate nickname doesn't contain special characters (allow letters, numbers, spaces, underscore, dash)
            if (!IsValidNickname(request.Nickname))
            {
                return new UpdateProfileResponse
                {
                    Success = false,
                    Message = _localization.GetString("Profile.NicknameInvalidCharacters")
                };
            }

            // Get current user info
            var currentUser = await _userRepository.GetByIdAsync(userId);
            if (currentUser == null)
            {
                return new UpdateProfileResponse
                {
                    Success = false,
                    Message = _localization.GetString("User.NotFound")
                };
            }

            // Get current nickname from Sys_Users_Detail
            var currentPlayerInfo = await _userRepository.GetPlayerByUserNameAsync(currentUser.Email);
            string currentNickname = currentPlayerInfo?.NickName ?? string.Empty;

            bool usernameChanged = !string.Equals(currentUser.Email, request.Username, StringComparison.OrdinalIgnoreCase);
            bool nicknameChanged = !string.Equals(currentNickname, request.Nickname, StringComparison.Ordinal);

            // If nothing changed
            if (!usernameChanged && !nicknameChanged)
            {
                return new UpdateProfileResponse
                {
                    Success = false,
                    Message = _localization.GetString("Profile.NoChangesDetected")
                };
            }

            // Check if new username already exists (excluding current user)
            if (usernameChanged)
            {
                var usernameExists = await _userRepository.CheckUsernameExistsExcludingUserAsync(userId, request.Username);
                if (usernameExists)
                {
                    return new UpdateProfileResponse
                    {
                        Success = false,
                        Message = _localization.GetString("Profile.UsernameAlreadyExists", request.Username)
                    };
                }
            }

            // Check if new nickname already exists (excluding current user)
            if (nicknameChanged)
            {
                var nicknameExists = await _userRepository.CheckNicknameExistsExcludingUserAsync(currentUser.Email, request.Nickname);
                if (nicknameExists)
                {
                    return new UpdateProfileResponse
                    {
                        Success = false,
                        Message = _localization.GetString("Profile.NicknameAlreadyExists", request.Nickname)
                    };
                }
            }

            // Update username if changed
            if (usernameChanged)
            {
                var usernameUpdated = await _userRepository.UpdateUsernameAsync(userId, currentUser.Email, request.Username);
                if (!usernameUpdated)
                {
                    return new UpdateProfileResponse
                    {
                        Success = false,
                        Message = _localization.GetString("Profile.UpdateUsernameFailed")
                    };
                }
            }

            // Update nickname if changed
            if (nicknameChanged)
            {
                // Use new username if it was changed, otherwise use current username
                string usernameForNicknameUpdate = usernameChanged ? request.Username : currentUser.Email;
                var nicknameUpdated = await _userRepository.UpdateNicknameAsync(usernameForNicknameUpdate, request.Nickname);
                if (!nicknameUpdated)
                {
                    return new UpdateProfileResponse
                    {
                        Success = false,
                        Message = _localization.GetString("Profile.UpdateNicknameFailed")
                    };
                }
            }

            return new UpdateProfileResponse
            {
                Success = true,
                Message = _localization.GetString("Profile.UpdateSuccess"),
                Data = new ProfileData
                {
                    Username = request.Username,
                    Nickname = request.Nickname
                }
            };
        }
        catch (SecurityException ex)
        {
            return new UpdateProfileResponse
            {
                Success = false,
                Message = ex.Message
            };
        }
        catch (Exception ex)
        {
            return new UpdateProfileResponse
            {
                Success = false,
                Message = _localization.GetString("Profile.UpdateFailed", ex.Message)
            };
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private bool IsValidNickname(string nickname)
    {
        // Allow letters (any language), numbers, spaces, underscore, and dash
        // No special characters like @, #, $, %, etc.
        return System.Text.RegularExpressions.Regex.IsMatch(nickname, @"^[\w\s\-]+$");
    }

    /// <summary>
    /// Xử lý quên mật khẩu - gửi email với link reset
    /// </summary>
    public async Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        try
        {
            // Validate email
            if (string.IsNullOrWhiteSpace(request.Email))
            {
                return new ForgotPasswordResponse
                {
                    Success = false,
                    Message = _localization.GetString("User.EmailRequired")
                };
            }

            if (!IsValidEmail(request.Email))
            {
                return new ForgotPasswordResponse
                {
                    Success = false,
                    Message = _localization.GetString("User.EmailInvalid")
                };
            }

            // Validate SQL injection
            SqlInjectionProtection.ValidateInput(request.Email, nameof(request.Email));

            // Kiểm tra email có tồn tại không
            var user = await _userRepository.GetByEmailAsync(request.Email);
            
            // Luôn trả về success để không lộ thông tin email có tồn tại hay không
            // (security best practice)
            if (user == null)
            {
                return new ForgotPasswordResponse
                {
                    Success = true,
                    Message = "Nếu email tồn tại, chúng tôi đã gửi link đặt lại mật khẩu đến email của bạn."
                };
            }

            // Generate secure token
            var token = GenerateSecureToken();
            var expiresAt = DateTime.Now.AddHours(1); // Token hết hạn sau 1 giờ

            // Lưu token vào database
            await _userRepository.CreatePasswordResetTokenAsync(user.UserId, user.Email, token, expiresAt);

            return new ForgotPasswordResponse
            {
                Success = true,
                Message = "Nếu email tồn tại, chúng tôi đã gửi link đặt lại mật khẩu đến email của bạn.",
                Token = token // Trả về token để controller gửi email
            };
        }
        catch (Exception ex)
        {
            return new ForgotPasswordResponse
            {
                Success = false,
                Message = $"Lỗi khi xử lý yêu cầu: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Verify reset token
    /// </summary>
    public async Task<VerifyResetTokenResponse> VerifyResetTokenAsync(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new VerifyResetTokenResponse
                {
                    IsValid = false,
                    Message = "Token không hợp lệ"
                };
            }

            SqlInjectionProtection.ValidateInput(token, nameof(token));

            var passwordReset = await _userRepository.VerifyPasswordResetTokenAsync(token);
            
            if (passwordReset == null)
            {
                return new VerifyResetTokenResponse
                {
                    IsValid = false,
                    Message = "Token không hợp lệ hoặc đã hết hạn"
                };
            }

            return new VerifyResetTokenResponse
            {
                IsValid = true,
                Message = "Token hợp lệ",
                Email = passwordReset.Email
            };
        }
        catch (Exception ex)
        {
            return new VerifyResetTokenResponse
            {
                IsValid = false,
                Message = $"Lỗi khi xác thực token: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Reset password với token
    /// </summary>
    public async Task<ResetPasswordResponse> ResetPasswordAsync(ResetPasswordRequest request)
    {
        try
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(request.Token))
            {
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = "Token không hợp lệ"
                };
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = _localization.GetString("Password.Required")
                };
            }

            if (request.NewPassword != request.ConfirmPassword)
            {
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = "Mật khẩu xác nhận không khớp"
                };
            }

            // Validate password strength
            if (request.NewPassword.Length < 6)
            {
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = _localization.GetString("Password.TooShort")
                };
            }

            SqlInjectionProtection.ValidateInputs(
                (request.Token, nameof(request.Token)),
                (request.NewPassword, nameof(request.NewPassword))
            );

            // Verify token trước
            var verifyResult = await VerifyResetTokenAsync(request.Token);
            if (!verifyResult.IsValid)
            {
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = verifyResult.Message
                };
            }

            // Reset password
            var success = await _userRepository.ResetPasswordWithTokenAsync(request.Token, request.NewPassword);

            if (success)
            {
                return new ResetPasswordResponse
                {
                    Success = true,
                    Message = "Đặt lại mật khẩu thành công!"
                };
            }
            else
            {
                return new ResetPasswordResponse
                {
                    Success = false,
                    Message = "Không thể đặt lại mật khẩu. Vui lòng thử lại."
                };
            }
        }
        catch (Exception ex)
        {
            return new ResetPasswordResponse
            {
                Success = false,
                Message = $"Lỗi khi đặt lại mật khẩu: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Generate secure token cho password reset
    /// </summary>
    private string GenerateSecureToken()
    {
        // Tạo token an toàn, khó đoán
        var bytes = new byte[32];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }
}
