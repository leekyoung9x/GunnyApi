using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace GunnyApi.Infrastructure.Controllers;

public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Lấy User ID từ JWT token
    /// </summary>
    protected int? GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return null;

        if (int.TryParse(userIdClaim, out int userId))
            return userId;

        return null;
    }

    /// <summary>
    /// Lấy Username từ JWT token
    /// </summary>
    protected string? GetUsernameFromToken()
    {
        return User.FindFirst(ClaimTypes.Name)?.Value;
    }

    /// <summary>
    /// Lấy Email từ JWT token
    /// </summary>
    protected string? GetEmailFromToken()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value;
    }

    /// <summary>
    /// Lấy Full Name từ JWT token
    /// </summary>
    protected string? GetFullNameFromToken()
    {
        return User.FindFirst("FullName")?.Value;
    }

    /// <summary>
    /// Lấy tất cả thông tin user từ JWT token
    /// </summary>
    protected UserTokenInfo? GetUserInfoFromToken()
    {
        var userId = GetUserIdFromToken();
        if (!userId.HasValue)
            return null;

        return new UserTokenInfo
        {
            UserId = userId.Value,
            Username = GetUsernameFromToken(),
            Email = GetEmailFromToken(),
            FullName = GetFullNameFromToken()
        };
    }

    /// <summary>
    /// Kiểm tra xem user có claim cụ thể không
    /// </summary>
    protected bool HasClaim(string claimType, string? claimValue = null)
    {
        if (string.IsNullOrEmpty(claimValue))
            return User.HasClaim(c => c.Type == claimType);

        return User.HasClaim(claimType, claimValue);
    }
}

public class UserTokenInfo
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
}
