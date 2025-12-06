using Dapper;
using GunnyApi.Infrastructure.Database;
using GunnyApi.Infrastructure.Repositories;
using GunnyApi.Infrastructure.Security;
using GunnyApi.Models;

namespace GunnyApi.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    protected override string TableName => "Users";
    protected override string IdColumn => "Id";

    public UserRepository(IDbConnectionFactory connectionFactory) 
        : base(connectionFactory)
    {
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
}
