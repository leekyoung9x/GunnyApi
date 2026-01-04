namespace GunnyApi.Models;

/// <summary>
/// Base request model cho tất cả các API request
/// </summary>
public class BaseRequest
{
    /// <summary>
    /// User ID được extract từ JWT token (không bắt buộc)
    /// Sẽ được tự động gán bởi UserContextMiddleware nếu có token
    /// </summary>
    public int? UserId { get; set; }
}
