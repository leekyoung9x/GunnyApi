using Dapper;
using GunnyApi.Infrastructure.Database;
using GunnyApi.Infrastructure.Repositories;
using GunnyApi.Infrastructure.Security;
using GunnyApi.Infrastructure.Settings;
using GunnyApi.Infrastructure.Utils;
using GunnyApi.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using System.Data;

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
    private readonly IConfiguration _configuration;

    public UserRepository(IDbConnectionFactory connectionFactory, IOptions<GameSettings> gameSettings, IConfiguration configuration) 
        : base(connectionFactory)
    {
        _gameSettings = gameSettings.Value;
        _configuration = configuration;
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

        var sql = "SELECT COUNT(1) FROM Mem_Account WHERE Email = @Email";
        
        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Email = email });
        return count > 0;
    }

    public async Task<bool> NicknameExistsAsync(string nickname)
    {
        // Validate input để chống SQL injection
        SqlInjectionProtection.ValidateInput(nickname, nameof(nickname));

        var sql = "SELECT COUNT(1) FROM Mem_Account WHERE Fullname = @Nickname";
        
        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Nickname = nickname });
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

    /// <summary>
    /// Subtract money from Mem_Account and return user's nickname
    /// </summary>
    public async Task<TransferMoneyResponse> TransferMoneyAsync(int userId, int amount)
    {
        if (amount <= 0)
        {
            return new TransferMoneyResponse
            {
                Success = false,
                Message = "Số tiền phải lớn hơn 0"
            };
        }

        var memberConnectionString = _configuration.GetConnectionString("DefaultConnection");
        using var memberConnection = new SqlConnection(memberConnectionString);
        await memberConnection.OpenAsync();
        using var memberTransaction = memberConnection.BeginTransaction();

        try
        {
            // Step 1: Check if user has enough money in Mem_Account
            var checkMoneySql = "SELECT Money FROM Mem_Account WHERE UserID = @UserId";
            var currentMoney = await memberConnection.ExecuteScalarAsync<int?>(
                checkMoneySql, 
                new { UserId = userId }, 
                memberTransaction
            );

            if (!currentMoney.HasValue)
            {
                return new TransferMoneyResponse
                {
                    Success = false,
                    Message = "Không tìm thấy tài khoản người dùng"
                };
            }

            if (currentMoney.Value < amount)
            {
                return new TransferMoneyResponse
                {
                    Success = false,
                    Message = $"Số dư không đủ. Số dư hiện tại: {currentMoney.Value}, Số tiền cần chuyển: {amount}"
                };
            }

            // Step 2: Get user's email for later use (email từ db member = username của db tank)
            var getEmailSql = "SELECT Email FROM Mem_Account WHERE UserID = @UserId";
            var userEmail = await memberConnection.ExecuteScalarAsync<string>(
                getEmailSql, 
                new { UserId = userId }, 
                memberTransaction
            );

            if (string.IsNullOrWhiteSpace(userEmail))
            {
                return new TransferMoneyResponse
                {
                    Success = false,
                    Message = "Không tìm thấy thông tin email người dùng"
                };
            }

            // Step 3: Subtract money from Mem_Account
            var updateMemberSql = "UPDATE Mem_Account SET Money = Money - @Amount WHERE UserID = @UserId";
            var memberRowsAffected = await memberConnection.ExecuteAsync(
                updateMemberSql, 
                new { UserId = userId, Amount = amount }, 
                memberTransaction
            );

            if (memberRowsAffected == 0)
            {
                throw new Exception("Không thể trừ tiền từ tài khoản Member");
            }

            // Commit transaction
            await memberTransaction.CommitAsync();

            // Get updated balance
            var updatedMemberMoney = await memberConnection.ExecuteScalarAsync<int>(
                "SELECT Money FROM Mem_Account WHERE UserID = @UserId", 
                new { UserId = userId }
            );

            return new TransferMoneyResponse
            {
                Success = true,
                Message = $"Đã trừ {amount} từ tài khoản Member",
                RemainingMemberMoney = updatedMemberMoney,
                UserEmail = userEmail
            };
        }
        catch (Exception ex)
        {
            // Rollback transaction on error
            try
            {
                await memberTransaction.RollbackAsync();
            }
            catch
            {
                // Ignore rollback errors
            }

            return new TransferMoneyResponse
            {
                Success = false,
                Message = $"Lỗi khi trừ tiền: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Register new user in Mem_Account and return userId
    /// </summary>
    public async Task<int?> RegisterUserAsync(string username, string password, string fullname)
    {
        try
        {
            // Validate input
            SqlInjectionProtection.ValidateInputs(
                (username, nameof(username)),
                (password, nameof(password)),
                (fullname, nameof(fullname))
            );

            // Kiểm tra email đã tồn tại chưa
            if (await EmailExistsAsync(username))
            {
                throw new InvalidOperationException($"Email '{username}' đã tồn tại");
            }

            // Kiểm tra nickname đã tồn tại chưa
            if (await NicknameExistsAsync(fullname))
            {
                throw new InvalidOperationException($"Nickname '{fullname}' đã tồn tại");
            }

            // Hash password bằng BCrypt
            var hashedPassword = BCryptHelper.HashPassword(password);

            var sql = @"
                INSERT INTO Mem_Account (Email, Password, Fullname, Money, MoneyLock, TotalMoney, MoneyEvent, Point, CountLucky, VIPLevel, VIPExp, IsBan, AllowSocialLogin, TimeCreate)
                VALUES (@Email, @Password, @Fullname, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, DATEDIFF(s, '1970-01-01', GETDATE()));
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using var connection = _connectionFactory.CreateConnection();
            var userId = await connection.ExecuteScalarAsync<int>(sql, new
            {
                Email = username,
                Password = hashedPassword,
                Fullname = fullname
            });

            return userId;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error registering user: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Call Proc_InsertNewUserDetail stored procedure on TankConnection
    /// </summary>
    public async Task<bool> CallInsertUserDetailProcAsync(int userId, string username, string nickname, int exp, int gold, int money, bool sex)
    {
        try
        {
            // Validate input
            SqlInjectionProtection.ValidateInputs(
                (username, nameof(username)),
                (nickname, nameof(nickname))
            );

            var tankConnectionString = _configuration.GetConnectionString("TankConnection")
                ?? throw new InvalidOperationException("Connection string 'TankConnection' not found.");

            using var connection = new SqlConnection(tankConnectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@UserID", userId);
            parameters.Add("@UserName", username);
            parameters.Add("@NickName", nickname);
            parameters.Add("@exp", exp);
            parameters.Add("@gold", gold);
            parameters.Add("@money", money);
            parameters.Add("@sex", sex);

            await connection.ExecuteAsync(
                "Proc_InsertNewUserDetail",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error calling Proc_InsertNewUserDetail: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Get player information by nickname from Tank database
    /// </summary>
    public async Task<PlayerInfo?> GetPlayerByNickNameAsync(string nickName)
    {
        try
        {
            // Validate input
            SqlInjectionProtection.ValidateInput(nickName, nameof(nickName));

            var tankConnectionString = _configuration.GetConnectionString("TankConnection")
                ?? throw new InvalidOperationException("Connection string 'TankConnection' not found.");

            using var connection = new SqlConnection(tankConnectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@NickName", nickName);

            var result = await connection.QueryFirstOrDefaultAsync<PlayerInfo>(
                "SP_Users_SingleByNickName",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting player by nickname: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Get player information by username (email) from Tank database
    /// </summary>
    public async Task<PlayerInfo?> GetPlayerByUserNameAsync(string userName)
    {
        try
        {
            // Validate input
            SqlInjectionProtection.ValidateInput(userName, nameof(userName));

            var tankConnectionString = _configuration.GetConnectionString("TankConnection")
                ?? throw new InvalidOperationException("Connection string 'TankConnection' not found.");

            using var connection = new SqlConnection(tankConnectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@UserName", userName);

            var result = await connection.QueryFirstOrDefaultAsync<PlayerInfo>(
                "SP_Users_SingleByUserName",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return result;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error getting player by username: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Send mail to player in Tank database
    /// </summary>
    public async Task<bool> SendMailAsync(MailInfo mail)
    {
        try
        {
            var tankConnectionString = _configuration.GetConnectionString("TankConnection")
                ?? throw new InvalidOperationException("Connection string 'TankConnection' not found.");

            using var connection = new SqlConnection(tankConnectionString);
            
            var parameters = new DynamicParameters();
            parameters.Add("@ID", dbType: DbType.Int32, direction: ParameterDirection.Output);
            parameters.Add("@Annex1", mail.Annex1 ?? "", DbType.String);
            parameters.Add("@Annex2", mail.Annex2 ?? "", DbType.String);
            parameters.Add("@Content", mail.Content ?? "", DbType.String);
            parameters.Add("@Gold", mail.Gold, DbType.Int32);
            parameters.Add("@IsExist", true, DbType.Boolean);
            parameters.Add("@Money", mail.Money, DbType.Int32);
            parameters.Add("@Receiver", mail.Receiver ?? "", DbType.String);
            parameters.Add("@ReceiverID", mail.ReceiverID, DbType.Int32);
            parameters.Add("@Sender", mail.Sender ?? "", DbType.String);
            parameters.Add("@SenderID", mail.SenderID, DbType.Int32);
            parameters.Add("@Title", mail.Title ?? "", DbType.String);
            parameters.Add("@IfDelS", false, DbType.Boolean);
            parameters.Add("@IsDelete", false, DbType.Boolean);
            parameters.Add("@IsDelR", false, DbType.Boolean);
            parameters.Add("@IsRead", false, DbType.Boolean);
            parameters.Add("@SendTime", DateTime.Now, DbType.DateTime);
            parameters.Add("@Type", mail.Type, DbType.Int32);
            parameters.Add("@Annex1Name", mail.Annex1Name ?? "", DbType.String);
            parameters.Add("@Annex2Name", mail.Annex2Name ?? "", DbType.String);
            parameters.Add("@Annex3", mail.Annex3 ?? "", DbType.String);
            parameters.Add("@Annex4", mail.Annex4 ?? "", DbType.String);
            parameters.Add("@Annex5", mail.Annex5 ?? "", DbType.String);
            parameters.Add("@Annex3Name", mail.Annex3Name ?? "", DbType.String);
            parameters.Add("@Annex4Name", mail.Annex4Name ?? "", DbType.String);
            parameters.Add("@Annex5Name", mail.Annex5Name ?? "", DbType.String);
            parameters.Add("@ValidDate", 30, DbType.Int32);  // ValidDate is days count (int), not datetime
            parameters.Add("@AnnexRemark", mail.AnnexRemark ?? "", DbType.String);
            parameters.Add("@GiftToken", mail.GiftToken, DbType.Int32);

            await connection.ExecuteAsync(
                "SP_Mail_Send",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            mail.ID = parameters.Get<int>("@ID");
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error sending mail: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Change user password in Mem_Account
    /// </summary>
    public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword)
    {
        try
        {
            // Validate inputs
            SqlInjectionProtection.ValidateInputs(
                (oldPassword, nameof(oldPassword)),
                (newPassword, nameof(newPassword))
            );

            // Kiểm tra phương thức mã hóa mật khẩu từ config
            var encryptionMethod = _gameSettings.PasswordEncryptionMethod?.ToUpper() ?? "MD5";

            using var connection = _connectionFactory.CreateConnection();
            
            // Get current password from database
            string getCurrentPasswordSql;
            if (encryptionMethod == "MD5")
            {
                getCurrentPasswordSql = "SELECT Password FROM Mem_Users WHERE UserId = @UserId";
            }
            else // BCrypt
            {
                getCurrentPasswordSql = "SELECT Password FROM Mem_Account WHERE UserID = @UserId";
            }

            var currentHashedPassword = await connection.ExecuteScalarAsync<string>(
                getCurrentPasswordSql,
                new { UserId = userId }
            );

            if (string.IsNullOrEmpty(currentHashedPassword))
            {
                return false; // User not found
            }

            // Verify old password
            bool isOldPasswordValid;
            if (encryptionMethod == "MD5")
            {
                var oldPasswordHash = MD5Helper.ToMD5(oldPassword);
                isOldPasswordValid = oldPasswordHash == currentHashedPassword;
            }
            else // BCrypt
            {
                isOldPasswordValid = BCryptHelper.VerifyPassword(oldPassword, currentHashedPassword);
            }

            if (!isOldPasswordValid)
            {
                return false; // Old password is incorrect
            }

            // Hash new password
            string newPasswordHash;
            if (encryptionMethod == "MD5")
            {
                newPasswordHash = MD5Helper.ToMD5(newPassword);
            }
            else // BCrypt
            {
                newPasswordHash = BCryptHelper.HashPassword(newPassword);
            }

            // Update password in database
            string updatePasswordSql;
            if (encryptionMethod == "MD5")
            {
                updatePasswordSql = "UPDATE Mem_Users SET Password = @Password WHERE UserId = @UserId";
            }
            else // BCrypt
            {
                updatePasswordSql = "UPDATE Mem_Account SET Password = @Password WHERE UserID = @UserId";
            }

            var rowsAffected = await connection.ExecuteAsync(
                updatePasswordSql,
                new { UserId = userId, Password = newPasswordHash }
            );

            return rowsAffected > 0;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error changing password: {ex.Message}", ex);
        }
    }
}
