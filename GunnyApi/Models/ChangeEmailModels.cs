namespace GunnyApi.Models;

/// <summary>
/// Request để bắt đầu thay đổi email (Step 1 - Verify Old Email)
/// </summary>
public class ChangeEmailRequestStep1
{
    /// <summary>
    /// Mật khẩu hiện tại để xác thực
    /// </summary>
    public string CurrentPassword { get; set; } = string.Empty;
    
    /// <summary>
    /// Email mới muốn đổi sang
    /// </summary>
    public string NewEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Xác nhận email mới
    /// </summary>
    public string ConfirmNewEmail { get; set; } = string.Empty;
}

/// <summary>
/// Request để verify OTP từ email cũ (Step 1)
/// </summary>
public class VerifyOldEmailOtpRequest
{
    /// <summary>
    /// Mã OTP nhận được từ email cũ
    /// </summary>
    public string OtpCode { get; set; } = string.Empty;
}

/// <summary>
/// Request để verify OTP từ email mới (Step 2)
/// </summary>
public class VerifyNewEmailOtpRequest
{
    /// <summary>
    /// Email mới
    /// </summary>
    public string NewEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Mã OTP nhận được từ email mới
    /// </summary>
    public string OtpCode { get; set; } = string.Empty;
}

/// <summary>
/// Request để gửi lại OTP
/// </summary>
public class ResendEmailOtpRequest
{
    /// <summary>
    /// Step cần gửi lại (1 hoặc 2)
    /// </summary>
    public int Step { get; set; }
    
    /// <summary>
    /// Email mới (cần thiết cho cả Step 1 và 2)
    /// </summary>
    public string? NewEmail { get; set; }
}

/// <summary>
/// Response cho Step 1 - Sau khi request đổi email
/// </summary>
public class ChangeEmailStep1Response
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Email cũ đã được mask (để hiển thị)
    /// </summary>
    public string MaskedCurrentEmail { get; set; } = string.Empty;
    
    /// <summary>
    /// Email mới đã được mask
    /// </summary>
    public string MaskedNewEmail { get; set; } = string.Empty;
}

/// <summary>
/// Response cho việc verify OTP old email
/// </summary>
public class VerifyOldEmailOtpResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Email mới đã được mask (để hiển thị cho step 2)
    /// </summary>
    public string MaskedNewEmail { get; set; } = string.Empty;
}

/// <summary>
/// Response cho Step 2 - Sau khi verify new email
/// </summary>
public class ChangeEmailCompleteResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Email mới (sau khi đổi thành công)
    /// </summary>
    public string NewEmail { get; set; } = string.Empty;
}

/// <summary>
/// Response cho resend OTP
/// </summary>
public class ResendOtpResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Request để xác nhận verify email
/// </summary>
public class ConfirmVerifyEmailRequest
{
    /// <summary>
    /// Mã OTP để verify email
    /// </summary>
    public string OtpCode { get; set; } = string.Empty;
}

/// <summary>
/// Model cho EmailChangeOTP từ database
/// </summary>
public class EmailChangeOtp
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string CurrentEmail { get; set; } = string.Empty;
    public string NewEmail { get; set; } = string.Empty;
    public int Step { get; set; }
    public string OtpCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public int AttemptCount { get; set; }
    public int MaxAttempts { get; set; }
    public bool IsBlocked { get; set; }
    public DateTime? BlockedUntil { get; set; }
}
