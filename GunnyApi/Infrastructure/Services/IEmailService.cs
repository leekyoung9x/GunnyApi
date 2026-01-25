namespace GunnyApi.Infrastructure.Services;

/// <summary>
/// Interface cho Email Service
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Gửi email đơn giản
    /// </summary>
    /// <param name="toEmail">Email người nhận</param>
    /// <param name="subject">Tiêu đề email</param>
    /// <param name="body">Nội dung email</param>
    /// <param name="isHtml">Nội dung có phải HTML không (mặc định: true)</param>
    Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true);

    /// <summary>
    /// Gửi email đến nhiều người nhận
    /// </summary>
    /// <param name="toEmails">Danh sách email người nhận</param>
    /// <param name="subject">Tiêu đề email</param>
    /// <param name="body">Nội dung email</param>
    /// <param name="isHtml">Nội dung có phải HTML không (mặc định: true)</param>
    Task<bool> SendEmailToMultipleAsync(List<string> toEmails, string subject, string body, bool isHtml = true);

    /// <summary>
    /// Gửi email với attachment
    /// </summary>
    /// <param name="toEmail">Email người nhận</param>
    /// <param name="subject">Tiêu đề email</param>
    /// <param name="body">Nội dung email</param>
    /// <param name="attachmentPath">Đường dẫn file đính kèm</param>
    /// <param name="isHtml">Nội dung có phải HTML không (mặc định: true)</param>
    Task<bool> SendEmailWithAttachmentAsync(string toEmail, string subject, string body, string attachmentPath, bool isHtml = true);
}
