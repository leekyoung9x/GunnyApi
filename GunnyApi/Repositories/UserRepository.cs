using Dapper;
using GunnyApi.Infrastructure.Database;
using GunnyApi.Infrastructure.Repositories;
using GunnyApi.Infrastructure.Security;
using GunnyApi.Infrastructure.Settings;
using GunnyApi.Infrastructure.Utils;
using GunnyApi.Models;
using Microsoft.Extensions.Options;

namespace GunnyApi.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    protected override string TableName 
    { 
        get 
        {
            var encryptionMethod = _gameSettings.PasswordEncryptionMethod?.ToUpper() ?? "MD5";
            return encryptionMethod == "MD5" ? "Mem_Users" : "Mem_Account";
        }
    }
    protected override string IdColumn => "UserId";
    private readonly GameSettings _gameSettings;

    public UserRepository(IDbConnectionFactory connectionFactory, IOptions<GameSettings> gameSettings) 
        : base(connectionFactory)
    {
        _gameSettings = gameSettings.Value;
    }

    public override async Task<int> AddAsync(User entity)
    {
        // Validate inputs để chống SQL injection
        SqlInjectionProtection.ValidateInputs(
            (entity.Username, nameof(entity.Username)),
            (entity.Email, nameof(entity.Email)),
            (entity.FullName, nameof(entity.FullName))
        );

        var sql = @"
            INSERT INTO Users (Username, Email, FullName, CreatedAt, IsActive)
            VALUES (@Username, @Email, @FullName, @CreatedAt, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new
        {
            entity.Username,
            entity.Email,
            entity.FullName,
            CreatedAt = DateTime.Now,
            IsActive = true
        });
    }

    public override async Task<bool> UpdateAsync(User entity)
    {
        // Validate inputs để chống SQL injection
        SqlInjectionProtection.ValidateInputs(
            (entity.Username, nameof(entity.Username)),
            (entity.Email, nameof(entity.Email)),
            (entity.FullName, nameof(entity.FullName))
        );

        var sql = @"
            UPDATE Users 
            SET Username = @Username,
                Email = @Email,
                FullName = @FullName,
                UpdatedAt = @UpdatedAt,
                IsActive = @IsActive
            WHERE Id = @Id";

        var affectedRows = await ExecuteCommandAsync(sql, new
        {
            entity.Id,
            entity.Username,
            entity.Email,
            entity.FullName,
            UpdatedAt = DateTime.Now,
            entity.IsActive
        }, validateSql: false);

        return affectedRows > 0;
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        // Validate input để chống SQL injection
        SqlInjectionProtection.ValidateInput(username, nameof(username));

        var sql = "SELECT * FROM Users WHERE Username = @Username";
        return await ExecuteQueryFirstOrDefaultAsync<User>(sql, new { Username = username }, validateSql: false);
    }

    /// <summary>
    /// Lấy thông tin user kèm password để verify
    /// </summary>
    private async Task<User?> GetByUsernameWithPasswordAsync(string username)
    {
        // Validate input để chống SQL injection
        SqlInjectionProtection.ValidateInput(username, nameof(username));

        var encryptionMethod = _gameSettings.PasswordEncryptionMethod?.ToUpper() ?? "MD5";
        string sql;
        
        if (encryptionMethod == "MD5")
        {
            // Query cho bảng Mem_Users (dùng UserName)
            sql = "SELECT UserId as Id, UserName as Username, Password, Email, NickName as FullName, RegDate as CreatedAt, IsExist as IsActive FROM Mem_Users WHERE UserName = @Username";
        }
        else
        {
            // Query cho bảng Mem_Account (dùng Email làm Username, IsBan = 0 là active)
            sql = "SELECT UserID as Id, Email as Username, Password, Email, Fullname as FullName, DATEADD(s, TimeCreate, '1970-01-01') as CreatedAt, CAST(CASE WHEN IsBan = 0 THEN 1 ELSE 0 END AS BIT) as IsActive FROM Mem_Account WHERE Email = @Username";
        }
        
        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
    }

    public async Task<IEnumerable<User>> GetActiveUsersAsync()
    {
        var sql = "SELECT * FROM Users WHERE IsActive = 1";
        return await ExecuteQueryAsync<User>(sql, validateSql: false);
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        // Validate input để chống SQL injection
        SqlInjectionProtection.ValidateInput(username, nameof(username));

        var sql = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
        
        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Username = username });
        return count > 0;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        // Validate input để chống SQL injection
        SqlInjectionProtection.ValidateInput(email, nameof(email));

        var sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";
        
        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
        return count > 0;
    }

    public async Task<int?> LoginAsync(string applicationName, string userName, string password)
    {
        // Validate inputs để chống SQL injection
        SqlInjectionProtection.ValidateInputs(
            (applicationName, nameof(applicationName)),
            (userName, nameof(userName)),
            (password, nameof(password))
        );

        // Kiểm tra phương thức mã hóa mật khẩu từ config
        var encryptionMethod = _gameSettings.PasswordEncryptionMethod?.ToUpper() ?? "MD5";

        if (encryptionMethod == "MD5")
        {
            // Logic cũ: Dùng MD5 và stored procedure
            var hashedPassword = MD5Helper.ToMD5(password);

            // Execute stored procedure và nhận DynamicParameters trực tiếp
            var result = await ExecuteStoredProcedureAsync(
                "Mem_Users_Accede",
                inputParams: new
                {
                    ApplicationName = applicationName,
                    UserName = userName,
                    Password = hashedPassword
                },
                outputParamsDef: new Dictionary<string, System.Data.DbType>
                {
                    { "@UserId", System.Data.DbType.Int32 }
                }
            );

            // Lấy giá trị output trực tiếp từ DynamicParameters
            return result.Get<int?>("@UserId");
        }
        else // BCrypt
        {
            // Logic mới: Lấy user từ DB và verify password bằng BCrypt
            var user = await GetByUsernameWithPasswordAsync(userName);
            
            if (user == null)
            {
                return null; // User không tồn tại
            }

            // Verify password bằng BCrypt
            var isPasswordValid = BCryptHelper.VerifyPassword(password, user.Password);
            
            if (!isPasswordValid)
            {
                return null; // Password không đúng
            }

            // Kiểm tra user có active không
            if (!user.IsActive)
            {
                return null; // User không active
            }

            return user.Id;
        }
    }
}
