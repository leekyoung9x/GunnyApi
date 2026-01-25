namespace GunnyApi.Models;

/// <summary>
/// Request để expire một checkout session
/// </summary>
public class ExpireCheckoutSessionRequest
{
    /// <summary>
    /// ID của checkout session cần expire
    /// </summary>
    public string CheckoutSessionId { get; set; } = string.Empty;
}

/// <summary>
/// Response từ PayMongo khi expire checkout session
/// </summary>
public class ExpireCheckoutSessionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ExpireCheckoutSessionData? Data { get; set; }
}

/// <summary>
/// Dữ liệu chi tiết checkout session đã expire
/// </summary>
public class ExpireCheckoutSessionData
{
    public string CheckoutSessionId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? ExpiredAt { get; set; }
}
