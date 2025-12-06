using GunnyApi.Models;
using GunnyApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GunnyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
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
