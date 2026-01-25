namespace GunnyApi.Infrastructure.Settings;

/// <summary>
/// Cấu hình cho Email SMTP
/// </summary>
public class EmailSettings
{
    /// <summary>
    /// SMTP Server (ví dụ: smtp.gmail.com)
    /// </summary>
    public string SmtpServer { get; set; } = string.Empty;

    /// <summary>
    /// SMTP Port (thường là 587 cho TLS, 465 cho SSL, 25 cho không mã hóa)
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// Email gửi đi
    /// </summary>
    public string SenderEmail { get; set; } = string.Empty;

    /// <summary>
    /// Tên người gửi hiển thị
    /// </summary>
    public string SenderName { get; set; } = string.Empty;

    /// <summary>
    /// Mật khẩu email hoặc App Password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Sử dụng SSL/TLS
    /// </summary>
    public bool EnableSsl { get; set; } = true;

    /// <summary>
    /// Timeout cho kết nối SMTP (milliseconds)
    /// </summary>
    public int Timeout { get; set; } = 30000;

    /// <summary>
    /// Email nhận bản sao (BCC)
    /// </summary>
    public string? BccEmail { get; set; }
}
