using GunnyApi.Infrastructure.Context;
using GunnyApi.Infrastructure.Controllers;
using GunnyApi.Infrastructure.Http;
using GunnyApi.Infrastructure.Settings;
using GunnyApi.Infrastructure.Utils;
using GunnyApi.Infrastructure.Services;
using GunnyApi.Models;
using GunnyApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Web;

namespace GunnyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : BaseApiController
{
    private readonly IUserService _userService;
    private readonly GameSettings _gameSettings;
    private readonly IHttpClientService _httpClientService;
    private readonly IUserContext _userContext;
    private readonly IServerService _serverService;
    private readonly ILocalizationService _localization;

    public UsersController(
        IUserService userService,
        IOptions<GameSettings> gameSettings,
        IHttpClientService httpClientService,
        IUserContext userContext,
        IServerService serverService,
        ILocalizationService localization)
    {
        _userService = userService;
        _gameSettings = gameSettings.Value;
        _httpClientService = httpClientService;
        _userContext = userContext;
        _serverService = serverService;
        _localization = localization;
    }

    /// <summary>
    /// Lấy tất cả users
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localization.GetString("Error.Generic"), error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy user theo ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var user = await _userService.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = _localization.GetString("User.NotFound") });
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localization.GetString("Error.Generic"), error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy user theo username
    /// </summary>
    [HttpGet("username/{username}")]
    public async Task<IActionResult> GetByUsername(string username)
    {
        try
        {
            var user = await _userService.GetByUsernameAsync(username);
            if (user == null)
            {
                return NotFound(new { message = _localization.GetString("User.NotFound") });
            }
            return Ok(user);
        }
        catch (Infrastructure.Security.SecurityException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localization.GetString("Error.Generic"), error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy danh sách users đang active
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActiveUsers()
    {
        try
        {
            var users = await _userService.GetActiveUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localization.GetString("Error.Generic"), error = ex.Message });
        }
    }

    /// <summary>
    /// Lấy thông tin user hiện tại từ token
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            // Lấy UserId từ UserContext (đã được middleware inject)
            if (!_userContext.UserId.HasValue)
            {
                return Unauthorized(new { message = _localization.GetString("Login.Unauthorized") });
            }

            var user = await _userService.GetCurrentUserAsync(_userContext.UserId.Value);
            
            if (user == null)
            {
                return NotFound(new { message = _localization.GetString("User.NotFound") });
            }

            return Ok(new
            {
                id = user.UserId,
                username = user.Username,
                email = user.Email,
                nickname = user.NickName,
                fullname = user.FullName,
                money = user.Money,
                createdAt = user.CreatedAt,
                isActive = user.IsActive
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localization.GetString("Error.Generic"), error = ex.Message });
        }
    }

    /// <summary>
    /// Login API - Đăng nhập bằng username và password
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _userService.LoginAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return Unauthorized(result);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localization.GetString("Error.Generic"), error = ex.Message });
        }
    }

    /// <summary>
    /// Register API - Đăng ký tài khoản mới
    /// </summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _userService.RegisterAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localization.GetString("Error.Generic"), error = ex.Message });
        }
    }

    /// <summary>
    /// Change Password API - Đổi mật khẩu cho user đã đăng nhập
    /// </summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            // Lấy UserId từ UserContext (đã được middleware inject)
            if (!_userContext.UserId.HasValue)
            {
                return Unauthorized(new { message = _localization.GetString("Login.Unauthorized") });
            }

            var result = await _userService.ChangePasswordAsync(_userContext.UserId.Value, request);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localization.GetString("Error.Generic"), error = ex.Message });
        }
    }

    /// <summary>
    /// Forgot Password API - Gửi email reset password
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            var result = await _userService.ForgotPasswordAsync(request);

            if (result.Success && !string.IsNullOrEmpty(result.Token))
            {
                // Lấy email service để gửi email
                var emailService = HttpContext.RequestServices.GetRequiredService<Infrastructure.Services.IEmailService>();
                var configuration = HttpContext.RequestServices.GetRequiredService<IConfiguration>();

                // Tạo reset link (có thể cấu hình trong appsettings.json)
                var frontendUrl = configuration["FrontendUrl"] ?? "http://localhost:3000";
                var resetLink = $"{frontendUrl}/reset-password?token={result.Token}";

                // Lấy thông tin user để gửi email
                var user = await _userService.GetByEmailAsync(request.Email);
                var username = user?.Username ?? request.Email;

                // Get localized strings
                var subject = _localization.GetString("ForgotPassword.EmailSubject");
                var title = _localization.GetString("ForgotPassword.EmailTitle");
                var greeting = _localization.GetString("ForgotPassword.EmailGreeting", username);
                var intro = _localization.GetString("ForgotPassword.EmailIntro");
                var clickButton = _localization.GetString("ForgotPassword.EmailClickButton");
                var buttonText = _localization.GetString("ForgotPassword.EmailButtonText");
                var orCopyLink = _localization.GetString("ForgotPassword.EmailOrCopyLink");
                var warningTitle = _localization.GetString("ForgotPassword.EmailWarningTitle");
                var warning1 = _localization.GetString("ForgotPassword.EmailWarning1");
                var warning2 = _localization.GetString("ForgotPassword.EmailWarning2");
                var warning3 = _localization.GetString("ForgotPassword.EmailWarning3");
                var ignore = _localization.GetString("ForgotPassword.EmailIgnore");
                var autoMessage = _localization.GetString("ForgotPassword.EmailAutoMessage");
                var copyright = _localization.GetString("ForgotPassword.EmailCopyright");

                // Template email
                var body = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                            .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                            .header {{ 
                                background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                                color: white; 
                                padding: 30px; 
                                text-align: center;
                                border-radius: 10px 10px 0 0;
                            }}
                            .content {{ 
                                padding: 30px; 
                                background-color: #f9f9f9;
                                border: 1px solid #e0e0e0;
                            }}
                            .button {{ 
                                display: inline-block; 
                                padding: 15px 40px;
                                background-color: #667eea; 
                                color: white;
                                text-decoration: none; 
                                border-radius: 5px;
                                font-weight: bold;
                                margin: 20px 0;
                            }}
                            .button:hover {{
                                background-color: #5568d3;
                            }}
                            .footer {{ 
                                text-align: center; 
                                padding: 20px; 
                                color: #777; 
                                font-size: 12px;
                                background-color: #f0f0f0;
                                border-radius: 0 0 10px 10px;
                            }}
                            .warning {{
                                background-color: #fff3cd;
                                border-left: 4px solid #ffc107;
                                padding: 15px;
                                margin: 20px 0;
                            }}
                            .token-box {{
                                background-color: #fff;
                                border: 2px dashed #667eea;
                                padding: 15px;
                                margin: 20px 0;
                                text-align: center;
                                font-family: monospace;
                                font-size: 14px;
                                word-break: break-all;
                            }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h1>{title}</h1>
                            </div>
                            <div class='content'>
                                <h2>{greeting}</h2>
                                <p>{intro}</p>
                                
                                <p>{clickButton}</p>
                                
                                <div style='text-align: center;'>
                                    <a href='{resetLink}' class='button'>{buttonText}</a>
                                </div>

                                <p>{orCopyLink}</p>
                                <div class='token-box'>{resetLink}</div>

                                <div class='warning'>
                                    <strong>{warningTitle}</strong>
                                    <ul style='margin: 10px 0; padding-left: 20px;'>
                                        <li>{warning1}</li>
                                        <li>{warning2}</li>
                                        <li>{warning3}</li>
                                    </ul>
                                </div>

                                <p>{ignore}</p>
                            </div>
                            <div class='footer'>
                                <p>{autoMessage}</p>
                                <p>{copyright}</p>
                            </div>
                        </div>
                    </body>
                    </html>
                ";

                // Gửi email (không chờ kết quả để response nhanh hơn)
                _ = Task.Run(async () =>
                {
                    await emailService.SendEmailAsync(request.Email, subject, body, true);
                });
            }

            // Không trả về token trong response (security)
            return Ok(new 
            { 
                success = result.Success, 
                message = result.Message 
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localization.GetString("Error.Generic"), error = ex.Message });
        }
    }

    /// <summary>
    /// Verify Reset Token API - Kiểm tra token có hợp lệ không
    /// </summary>
    [HttpGet("verify-reset-token")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyResetToken([FromQuery] string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { success = false, message = "Token không hợp lệ" });
            }

            var result = await _userService.VerifyResetTokenAsync(token);

            if (result.IsValid)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localization.GetString("Error.Generic"), error = ex.Message });
        }
    }

    /// <summary>
    /// Reset Password API - Đặt lại mật khẩu với token
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            var result = await _userService.ResetPasswordAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localization.GetString("Error.Generic"), error = ex.Message });
        }
    }

    /// <summary>
    /// Update Profile API - Cập nhật username và/hoặc nickname
    /// </summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            // Lấy UserId từ UserContext (đã được middleware inject)
            if (!_userContext.UserId.HasValue)
            {
                return Unauthorized(new { message = _localization.GetString("Login.Unauthorized") });
            }

            var result = await _userService.UpdateProfileAsync(_userContext.UserId.Value, request);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localization.GetString("Error.Generic"), error = ex.Message });
        }
    }

    /// <summary>
    /// Login Game API - Đăng nhập và chuyển hướng đến game
    /// </summary>
    [HttpGet("login-game")]
    public async Task<IActionResult> LoginGame()
    {
        try
        {
            // Lấy username từ token hoặc từ request
            var username = GetUsernameFromToken();
            
            // Nếu không có username từ token, kiểm tra request
            if (string.IsNullOrEmpty(username))
            {
                return BadRequest(new LoginGameResponse
                {
                    Success = false,
                    Message = _localization.GetString("Server.UsernameRequired")
                });
            }

            // Tạo password tạm thời nếu không có
            string password = Guid.NewGuid().ToString();

            // Tạo timestamp
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Lấy login key từ config
            string key = string.IsNullOrEmpty(_gameSettings.LoginKey) 
                ? "default-key" 
                : _gameSettings.LoginKey;

            // Tạo verification hash: md5(username + password + time + key)
            string verificationHash = MD5Helper.ToMD5(username + password + timestamp.ToString() + key);

            // Tạo content để gửi đến game server
            string content = $"{username}|{password}|{timestamp}|{verificationHash}";
            string encodedContent = HttpUtility.UrlEncode(content);

            // Gọi API của game server
            string loginUrl = $"{_gameSettings.LoginUrl}?content={encodedContent}";
            string result = await RequestContent(loginUrl);

            if (result == "0") // Login thành công
            {
                string flashUrl;

                // Kiểm tra chế độ content2
                if (_gameSettings.Content2Mode == "1")
                {
                    // Sử dụng mã hóa Triple DES
                    string origin = $"{username}|{password}";
                    string iv = _gameSettings.TripleDesIV;
                    string encryptedContent = CryptoHelper.TripleDesEncrypt(
                        _gameSettings.TripleDesKey, 
                        origin, 
                        ref iv
                    );
                    
                    flashUrl = $"{_gameSettings.FlashUrl}?content2={HttpUtility.UrlEncode(encryptedContent)}";
                }
                else
                {
                    // Chế độ thông thường
                    flashUrl = $"{_gameSettings.FlashSite}Loading.swf?user={HttpUtility.UrlEncode(username)}&key={HttpUtility.UrlEncode(password)}&config={_gameSettings.FlashConfig}";
                }

                return Ok(new LoginGameResponse
                {
                    Success = true,
                    Message = _localization.GetString("Server.LoginSuccess"),
                    RedirectUrl = flashUrl,
                    AutoParam = _gameSettings.AutoParam
                });
            }
            else
            {
                return Ok(new LoginGameResponse
                {
                    Success = false,
                    Message = _localization.GetString("Server.LoginFailed", result)
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new LoginGameResponse
            {
                Success = false,
                Message = _localization.GetString("Server.LoginGameError", ex.Message)
            });
        }
    }

    /// <summary>
    /// Gửi HTTP request và nhận response
    /// </summary>
    private async Task<string> RequestContent(string url)
    {
        try
        {
            return await _httpClientService.GetAsync(url);
        }
        catch (Exception ex)
        {
            return _localization.GetString("User.ConnectionError", ex.Message);
        }
    }

    /// <summary>
    /// Tạo user mới
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] User user)
    {
        try
        {
            var userId = await _userService.CreateAsync(user);
            return CreatedAtAction(nameof(GetById), new { id = userId }, new { id = userId });
        }
        catch (Infrastructure.Security.SecurityException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localization.GetString("Error.Generic"), error = ex.Message });
        }
    }

    /// <summary>
    /// Cập nhật user
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] User user)
    {
        try
        {
            if (id != user.Id)
            {
                return BadRequest(new { message = _localization.GetString("User.IdMismatch") });
            }

            var success = await _userService.UpdateAsync(user);
            if (!success)
            {
                return NotFound(new { message = _localization.GetString("User.NotFound") });
            }
            return Ok(new { message = _localization.GetString("User.UpdateSuccess") });
        }
        catch (Infrastructure.Security.SecurityException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localization.GetString("Error.Generic"), error = ex.Message });
        }
    }

    /// <summary>
    /// Chuyển tiền từ Member database sang Tank database
    /// </summary>
    [HttpPost("transfer-money")]
    public async Task<IActionResult> TransferMoney([FromBody] TransferMoneyRequest request)
    {
        try
        {
            // Lấy userId từ UserContext (từ JWT token)
            var userId = _userContext.UserId;
            
            if (!userId.HasValue || userId.Value <= 0)
            {
                return Unauthorized(new { message = _localization.GetString("User.Unauthorized") });
            }

            // Validate amount
            if (request.Amount <= 0)
            {
                return BadRequest(new { message = _localization.GetString("User.AmountMustBePositive") });
            }

            // Thực hiện chuyển tiền
            var result = await _userService.TransferMoneyAsync(userId.Value, request.Amount);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                success = false, 
                message = _localization.GetString("Error.Generic"), 
                error = ex.Message 
            });
        }
    }

    /// <summary>
    /// Xóa user
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var success = await _userService.DeleteAsync(id);
            if (!success)
            {
                return NotFound(new { message = _localization.GetString("User.NotFound") });
            }
            return Ok(new { message = _localization.GetString("User.DeleteSuccess") });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localization.GetString("Error.Generic"), error = ex.Message });
        }
    }

    /// <summary>
    /// Gửi tiền, vàng, lễ kim cho người chơi
    /// </summary>
    [HttpPost("send-money")]
    [AllowAnonymous]
    public async Task<IActionResult> SendMoney([FromBody] SendMoneyRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserName))
            {
                return BadRequest(new { success = false, message = _localization.GetString("User.UsernameRequiredNotEmpty") });
            }

            var result = await _userService.SendMoneyAsync(
                request.UserName, 
                request.Gold, 
                request.Money, 
                request.GiftToken
            );

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                success = false, 
                message = _localization.GetString("Error.Generic"), 
                error = ex.Message 
            });
        }
    }

    /// <summary>
    /// Login Mobile API - Đăng nhập cho mobile client và trả về token để kết nối game
    /// </summary>
    [HttpPost("login-mobile")]
    [AllowAnonymous]
    public async Task<IActionResult> LoginMobile([FromBody] LoginRequest request)
    {
        try
        {
            // Validate input
            if (string.IsNullOrEmpty(request.UserName))
            {
                return Ok(new
                {
                    error = "INVALID_USERNAME",
                    msg = _localization.GetString("Login.InvalidUsername")
                });
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                return Ok(new
                {
                    error = "INVALID_PASSWORD",
                    msg = _localization.GetString("Login.InvalidPassword")
                });
            }

            // Authenticate user
            var loginResult = await _userService.LoginAsync(request);

            if (!loginResult.Success)
            {
                return Ok(new
                {
                    error = "AUTH_FAILED",
                    msg = loginResult.Message ?? _localization.GetString("Login.AuthFailed")
                });
            }

            // Generate game login credentials
            string username = request.UserName;
            // Tạo password tạm thời cho game session (không dùng password thật)
            string password = Guid.NewGuid().ToString();
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string key = string.IsNullOrEmpty(_gameSettings.LoginKey)
                ? "default-key"
                : _gameSettings.LoginKey;

            // Create verification hash
            string verificationHash = MD5Helper.ToMD5(username + password + timestamp.ToString() + key);

            // Create content for game server
            string content = $"{username}|{password}|{timestamp}|{verificationHash}";
            string encodedContent = HttpUtility.UrlEncode(content);

            // Call game server API to register session
            string loginUrl = $"{_gameSettings.LoginUrl}?content={encodedContent}";
            string result = await RequestContent(loginUrl);

            if (result == "0") // Game server accepted the session
            {
                // Return success with token (password is the key for game)
                return Ok(new
                {
                    token = password,
                    username = username,
                    msg = _localization.GetString("Login.Success")
                });
            }
            else
            {
                return Ok(new
                {
                    error = "GAME_SERVER_ERROR",
                    msg = _localization.GetString("Login.GameServerError", result)
                });
            }
        }
        catch (Exception ex)
        {
            return Ok(new
            {
                error = "SERVER_ERROR",
                msg = _localization.GetString("Login.ServerError", ex.Message)
            });
        }
    }

    /// <summary>
    /// Lấy danh sách servers từ Server_List
    /// </summary>
    [HttpPost("server")]
    [AllowAnonymous]
    public async Task<IActionResult> GetServerList([FromBody] ServerListRequest request)
    {
        try
        {
            var response = await _serverService.GetServerListAsync(request.Version);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = _localization.GetString("Error.Generic"), error = ex.Message });
        }
    }

    /// <summary>
    /// Create Key API - Tạo key bảo mật cho user
    /// </summary>
    [HttpPost("createKey")]
    [AllowAnonymous]
    public async Task<IActionResult> CreateKey([FromBody] CreateKeyRequest request)
    {
        try
        {
            // Validate input
            if (string.IsNullOrEmpty(request.username))
            {
                return Ok(new CreateKeyResponse
                {
                    code = 0,
                    key = string.Empty
                });
            }

            if (string.IsNullOrEmpty(request.password))
            {
                return Ok(new CreateKeyResponse
                {
                    code = 0,
                    key = string.Empty
                });
            }

            // Authenticate user
            var loginRequest = new LoginRequest
            {
                UserName = request.username,
                Password = request.password
            };

            var loginResult = await _userService.LoginAsync(loginRequest);

            if (!loginResult.Success)
            {
                return Ok(new CreateKeyResponse
                {
                    code = 0,
                    key = string.Empty
                });
            }

            // Generate game login credentials
            string username = request.username;
            // Tạo password tạm thời cho game session (không dùng password thật)
            string password = Guid.NewGuid().ToString();
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string key = string.IsNullOrEmpty(_gameSettings.LoginKey)
                ? "default-key"
                : _gameSettings.LoginKey;

            // Create verification hash
            string verificationHash = MD5Helper.ToMD5(username + password + timestamp.ToString() + key);

            // Create content for game server
            string content = $"{username}|{password}|{timestamp}|{verificationHash}";
            string encodedContent = HttpUtility.UrlEncode(content);

            // Call game server API to register session
            string loginUrl = $"{_gameSettings.LoginUrl}?content={encodedContent}";
            string result = await RequestContent(loginUrl);

            if (result == "0") // Game server accepted the session
            {
                // Return success with key (password is the key for game)
                return Ok(new CreateKeyResponse
                {
                    code = 0,
                    key = password
                });
            }
            else
            {
                return Ok(new CreateKeyResponse
                {
                    code = 0,
                    key = string.Empty
                });
            }
        }
        catch (Exception)
        {
            // Trả về exception với code khác 0
            return Ok(new CreateKeyResponse
            {
                code = 1,
                key = string.Empty
            });
        }
    }
}
