namespace GunnyApi.Models;

public class LoginResponse
{
    public bool Success { get; set; }
    public int? UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public bool EmailVerified { get; set; }
}
