using GunnyApi.Infrastructure.Repositories;
using GunnyApi.Infrastructure.Security;
using GunnyApi.Infrastructure.Services;
using GunnyApi.Models;
using GunnyApi.Repositories;

namespace GunnyApi.Services;

public class UserService : BaseService<User>, IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;

    public UserService(IUserRepository userRepository, IJwtTokenService jwtTokenService) 
        : base(userRepository)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
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
}
