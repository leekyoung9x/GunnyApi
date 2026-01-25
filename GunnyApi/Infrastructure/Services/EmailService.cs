using System.Net;
using System.Net.Mail;
using GunnyApi.Infrastructure.Settings;
using Microsoft.Extensions.Options;

namespace GunnyApi.Infrastructure.Services;

/// <summary>
/// Service gửi email qua SMTP
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Gửi email đơn giản
    /// </summary>
    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
    {
        try
        {
            ValidateEmailSettings();

            using var smtpClient = CreateSmtpClient();
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(toEmail);

            // Thêm BCC nếu có
            if (!string.IsNullOrEmpty(_emailSettings.BccEmail))
            {
                mailMessage.Bcc.Add(_emailSettings.BccEmail);
            }

            await smtpClient.SendMailAsync(mailMessage);
            
            _logger.LogInformation("Email đã được gửi thành công đến {ToEmail}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi email đến {ToEmail}", toEmail);
            return false;
        }
    }

    /// <summary>
    /// Gửi email đến nhiều người nhận
    /// </summary>
    public async Task<bool> SendEmailToMultipleAsync(List<string> toEmails, string subject, string body, bool isHtml = true)
    {
        try
        {
            ValidateEmailSettings();

            if (toEmails == null || !toEmails.Any())
            {
                _logger.LogWarning("Danh sách email người nhận trống");
                return false;
            }

            using var smtpClient = CreateSmtpClient();
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            // Thêm tất cả người nhận
            foreach (var email in toEmails)
            {
                if (!string.IsNullOrWhiteSpace(email))
                {
                    mailMessage.To.Add(email);
                }
            }

            // Thêm BCC nếu có
            if (!string.IsNullOrEmpty(_emailSettings.BccEmail))
            {
                mailMessage.Bcc.Add(_emailSettings.BccEmail);
            }

            await smtpClient.SendMailAsync(mailMessage);
            
            _logger.LogInformation("Email đã được gửi thành công đến {Count} người nhận", toEmails.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi email đến nhiều người nhận");
            return false;
        }
    }

    /// <summary>
    /// Gửi email với attachment
    /// </summary>
    public async Task<bool> SendEmailWithAttachmentAsync(string toEmail, string subject, string body, string attachmentPath, bool isHtml = true)
    {
        try
        {
            ValidateEmailSettings();

            if (!File.Exists(attachmentPath))
            {
                _logger.LogError("File đính kèm không tồn tại: {AttachmentPath}", attachmentPath);
                return false;
            }

            using var smtpClient = CreateSmtpClient();
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtml
            };

            mailMessage.To.Add(toEmail);

            // Thêm file đính kèm
            var attachment = new Attachment(attachmentPath);
            mailMessage.Attachments.Add(attachment);

            // Thêm BCC nếu có
            if (!string.IsNullOrEmpty(_emailSettings.BccEmail))
            {
                mailMessage.Bcc.Add(_emailSettings.BccEmail);
            }

            await smtpClient.SendMailAsync(mailMessage);
            
            _logger.LogInformation("Email với file đính kèm đã được gửi thành công đến {ToEmail}", toEmail);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi email với file đính kèm đến {ToEmail}", toEmail);
            return false;
        }
    }

    /// <summary>
    /// Tạo SMTP Client
    /// </summary>
    private SmtpClient CreateSmtpClient()
    {
        var smtpClient = new SmtpClient(_emailSettings.SmtpServer, _emailSettings.SmtpPort)
        {
            EnableSsl = _emailSettings.EnableSsl,
            Timeout = _emailSettings.Timeout,
            Credentials = new NetworkCredential(_emailSettings.SenderEmail, _emailSettings.Password)
        };

        return smtpClient;
    }

    /// <summary>
    /// Kiểm tra cấu hình email
    /// </summary>
    private void ValidateEmailSettings()
    {
        if (string.IsNullOrEmpty(_emailSettings.SmtpServer))
        {
            throw new InvalidOperationException("SMTP Server chưa được cấu hình");
        }

        if (string.IsNullOrEmpty(_emailSettings.SenderEmail))
        {
            throw new InvalidOperationException("Sender Email chưa được cấu hình");
        }

        if (string.IsNullOrEmpty(_emailSettings.Password))
        {
            throw new InvalidOperationException("Email Password chưa được cấu hình");
        }
    }
}
