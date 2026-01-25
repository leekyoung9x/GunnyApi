namespace GunnyApi.Models;

public class UpdateProfileResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public ProfileData? Data { get; set; }
}

public class ProfileData
{
    public string Username { get; set; } = string.Empty;
    public string Nickname { get; set; } = string.Empty;
}
