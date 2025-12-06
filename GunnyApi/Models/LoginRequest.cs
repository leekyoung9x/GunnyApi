namespace GunnyApi.Models;

public class LoginRequest
{
    public string ApplicationName { get; set; } = "DanDanTang";
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
