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

    public UserService(IUserRepository userRepository, IJwtTokenService jwtTokenService, IConfiguration configuration) 
        : base(userRepository)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
    }

    protected override async Task ValidateEntityAsync(User entity)
    {
        // Validate các trường bắt buộc
        if (string.IsNullOrWhiteSpace(entity.Username))
        {
            throw new ArgumentException("Username không được rỗng", nameof(entity.Username));
        }

        if (string.IsNullOrWhiteSpace(entity.Email))
        {
            throw new ArgumentException("Email không được rỗng", nameof(entity.Email));
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
            throw new InvalidOperationException($"Username '{entity.Username}' đã tồn tại");
        }

        // Kiểm tra email đã tồn tại chưa (khi tạo mới)
        if (entity.Id == 0 && await _userRepository.EmailExistsAsync(entity.Email))
        {
            throw new InvalidOperationException($"Email '{entity.Email}' đã tồn tại");
        }

        await base.ValidateEntityAsync(entity);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Username không được rỗng", nameof(username));
        }

        // Validate SQL injection
        SqlInjectionProtection.ValidateInput(username, nameof(username));

        return await _userRepository.GetByUsernameAsync(username);
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
                Message = "Username không được rỗng" 
            };
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return new LoginResponse 
            { 
                Success = false, 
                Message = "Password không được rỗng" 
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
                        Message = "Không tìm thấy thông tin user"
                    };
                }

                user.Id = user.Id == 0 ? userId.Value : user.Id;

                // Generate JWT token
                var token = _jwtTokenService.GenerateToken(user);
                var refreshToken = _jwtTokenService.GenerateRefreshToken();

                return new LoginResponse 
                { 
                    Success = true, 
                    UserId = userId.Value,
                    Token = token,
                    RefreshToken = refreshToken,
                    Message = "Đăng nhập thành công" 
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
                Message = "Có lỗi xảy ra khi đăng nhập: " + ex.Message 
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

        return await _userRepository.TransferMoneyAsync(userId, amount);
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

            // Get player by nickname from Tank database
            var player = await _userRepository.GetPlayerByNickNameAsync(userName);
            
            if (player == null)
            {
                return new SendMoneyResponse
                {
                    Success = false,
                    Message = $"Tài khoản <strong>{userName}</strong> không tồn tại."
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
                mailNoticeMessage = $" (Lỗi thông báo: {ex.Message})";
            }

            return new SendMoneyResponse
            {
                Success = true,
                Message = $"Đã chuyển thành công <br /> {money} Xu <br /> {gold} Vàng <br /> {giftToken} Lễ kim <br /> cho tài khoản <strong>{userName}</strong>{mailNoticeMessage}"
            };
        }
        catch (Exception ex)
        {
            return new SendMoneyResponse
            {
                Success = false,
                Message = $"Có lỗi xảy ra: {ex.Message}"
            };
        }
    }
}
