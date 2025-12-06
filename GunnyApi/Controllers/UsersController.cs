using GunnyApi.Infrastructure.Controllers;
using GunnyApi.Infrastructure.Http;
using GunnyApi.Infrastructure.Settings;
using GunnyApi.Infrastructure.Utils;
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

    public UsersController(
        IUserService userService,
        IOptions<GameSettings> gameSettings,
        IHttpClientService httpClientService)
    {
        _userService = userService;
        _gameSettings = gameSettings.Value;
        _httpClientService = httpClientService;
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
}
