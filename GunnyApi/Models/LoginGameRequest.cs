namespace GunnyApi.Models;

public class LoginGameRequest
{
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
}
