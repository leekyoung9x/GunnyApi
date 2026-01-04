using System.Security.Claims;
using GunnyApi.Infrastructure.Context;

namespace GunnyApi.Infrastructure.Middleware;

/// <summary>
/// Middleware để đọc thông tin user từ JWT token và lưu vào UserContext
/// </summary>
public class UserContextMiddleware
{
    private readonly RequestDelegate _next;

    public UserContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IUserContext userContext)
    {
        // Kiểm tra nếu user đã authenticated
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            // Đọc thông tin từ claims
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            var usernameClaim = context.User.FindFirst(ClaimTypes.Name);
            var emailClaim = context.User.FindFirst(ClaimTypes.Email);
            var fullNameClaim = context.User.FindFirst("FullName");

            // Gán vào UserContext
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId))
            {
                userContext.UserId = userId;
            }

            userContext.Username = usernameClaim?.Value;
            userContext.Email = emailClaim?.Value;
            userContext.FullName = fullNameClaim?.Value;
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method để dễ dàng register middleware
/// </summary>
public static class UserContextMiddlewareExtensions
{
    public static IApplicationBuilder UseUserContext(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<UserContextMiddleware>();
    }
}
