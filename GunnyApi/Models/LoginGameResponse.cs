namespace GunnyApi.Models;

public class LoginGameResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? RedirectUrl { get; set; }
    public string? AutoParam { get; set; }
}
