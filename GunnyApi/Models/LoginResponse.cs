namespace GunnyApi.Models;

public class LoginResponse
{
    public bool Success { get; set; }
    public int? UserId { get; set; }
    public string Message { get; set; } = string.Empty;
}
