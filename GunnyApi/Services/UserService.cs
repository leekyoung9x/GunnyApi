using GunnyApi.Infrastructure.Repositories;
using GunnyApi.Infrastructure.Security;
using GunnyApi.Infrastructure.Services;
using GunnyApi.Models;
using GunnyApi.Repositories;

namespace GunnyApi.Services;

public class UserService : BaseService<User>, IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository) 
        : base(userRepository)
    {
        _userRepository = userRepository;
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
}
