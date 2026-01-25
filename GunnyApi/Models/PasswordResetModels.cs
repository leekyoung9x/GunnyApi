namespace GunnyApi.Models;

/// <summary>
/// Request quên mật khẩu
/// </summary>
public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Response quên mật khẩu
/// </summary>
public class ForgotPasswordResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; } // Internal use only - để gửi email
}

/// <summary>
/// Request reset mật khẩu với token
/// </summary>
public class ResetPasswordRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Response reset mật khẩu
/// </summary>
public class ResetPasswordResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Request verify token
/// </summary>
public class VerifyResetTokenRequest
{
    public string Token { get; set; } = string.Empty;
}

/// <summary>
/// Response verify token
/// </summary>
public class VerifyResetTokenResponse
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Email { get; set; }
}
