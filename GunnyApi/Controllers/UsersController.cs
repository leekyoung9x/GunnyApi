using GunnyApi.Infrastructure.Controllers;
using GunnyApi.Infrastructure.Http;
using GunnyApi.Infrastructure.Settings;
using GunnyApi.Infrastructure.Utils;
using GunnyApi.Models;
using GunnyApi.Repositories;
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
    private readonly IServerService _serverService;

    public UsersController(
        IUserService userService,
        IOptions<GameSettings> gameSettings,
        IHttpClientService httpClientService,
        IServerService serverService)
    {
        _userService = userService;
        _gameSettings = gameSettings.Value;
        _httpClientService = httpClientService;
        _serverService = serverService;
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
            return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
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
                return NotFound(new { message = "Không tìm thấy user" });
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
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
                return NotFound(new { message = "Không tìm thấy user" });
            }
            return Ok(user);
        }
        catch (Infrastructure.Security.SecurityException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
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
            return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
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
            return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
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
                    msg = "Vui lòng nhập tài khoản" 
                });
            }

            if (string.IsNullOrEmpty(request.Password))
            {
                return Ok(new 
                { 
                    error = "INVALID_PASSWORD",
                    msg = "Vui lòng nhập đầy đủ thông tin" 
                });
            }

            // Authenticate user
            var loginResult = await _userService.LoginAsync(request);

            if (!loginResult.Success)
            {
                return Ok(new 
                { 
                    error = "AUTH_FAILED",
                    msg = loginResult.Message ?? "Đăng nhập thất bại" 
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
                    msg = "Đăng nhập thành công"
                });
            }
            else
            {
                return Ok(new 
                { 
                    error = "GAME_SERVER_ERROR",
                    msg = $"Không thể kết nối game server: {result}" 
                });
            }
        }
        catch (Exception ex)
        {
            return Ok(new 
            { 
                error = "SERVER_ERROR",
                msg = $"Có lỗi xảy ra: {ex.Message}" 
            });
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
                    Message = "Username không được để trống"
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
                    Message = "Đăng nhập game thành công",
                    RedirectUrl = flashUrl,
                    AutoParam = _gameSettings.AutoParam
                });
            }
            else
            {
                return Ok(new LoginGameResponse
                {
                    Success = false,
                    Message = $"Đăng nhập game thất bại: {result}"
                });
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, new LoginGameResponse
            {
                Success = false,
                Message = $"Có lỗi xảy ra: {ex.Message}"
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
            return $"Lỗi kết nối: {ex.Message}";
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
            return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
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
                return BadRequest(new { message = "ID không khớp" });
            }

            var success = await _userService.UpdateAsync(user);
            if (!success)
            {
                return NotFound(new { message = "Không tìm thấy user để cập nhật" });
            }
            return Ok(new { message = "Cập nhật thành công" });
        }
        catch (Infrastructure.Security.SecurityException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
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
                return NotFound(new { message = "Không tìm thấy user để xóa" });
            }
            return Ok(new { message = "Xóa thành công" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
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
            return StatusCode(500, new { message = "Có lỗi xảy ra", error = ex.Message });
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
        catch (Exception ex)
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
