using GunnyApi.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GunnyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailController> _logger;

    public EmailController(IEmailService emailService, ILogger<EmailController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Gửi email test
    /// </summary>
    [HttpPost("send-test")]
    public async Task<IActionResult> SendTestEmail([FromBody] SendEmailRequest request)
    {
        try
        {
            var result = await _emailService.SendEmailAsync(
                request.ToEmail,
                request.Subject,
                request.Body,
                request.IsHtml
            );

            if (result)
            {
                return Ok(new { success = true, message = "Email đã được gửi thành công!" });
            }

            return BadRequest(new { success = false, message = "Gửi email thất bại. Vui lòng kiểm tra log." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi email test");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Gửi email chào mừng cho người dùng mới
    /// </summary>
    [HttpPost("send-welcome")]
    public async Task<IActionResult> SendWelcomeEmail([FromBody] WelcomeEmailRequest request)
    {
        try
        {
            var subject = "Chào mừng bạn đến với Gunny!";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .footer {{ text-align: center; padding: 20px; color: #777; font-size: 12px; }}
                        .button {{ display: inline-block; padding: 10px 20px; background-color: #4CAF50; 
                                   color: white; text-decoration: none; border-radius: 5px; margin-top: 20px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Chào mừng đến với Gunny!</h1>
                        </div>
                        <div class='content'>
                            <h2>Xin chào {request.Username}!</h2>
                            <p>Cảm ơn bạn đã đăng ký tài khoản tại Gunny Game.</p>
                            <p>Thông tin tài khoản của bạn:</p>
                            <ul>
                                <li><strong>Tên đăng nhập:</strong> {request.Username}</li>
                                <li><strong>Email:</strong> {request.Email}</li>
                            </ul>
                            <p>Bạn có thể bắt đầu chơi ngay bây giờ!</p>
                            <a href='#' class='button'>Bắt đầu chơi</a>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2026 Gunny Game. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            var result = await _emailService.SendEmailAsync(request.Email, subject, body, true);

            if (result)
            {
                return Ok(new { success = true, message = "Email chào mừng đã được gửi!" });
            }

            return BadRequest(new { success = false, message = "Gửi email thất bại." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi email chào mừng");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Gửi email reset password
    /// </summary>
    [HttpPost("send-reset-password")]
    public async Task<IActionResult> SendResetPasswordEmail([FromBody] ResetPasswordEmailRequest request)
    {
        try
        {
            var subject = "Yêu cầu đặt lại mật khẩu";
            var body = $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                        .header {{ background-color: #f44336; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .code {{ background-color: #fff; border: 2px dashed #f44336; padding: 15px; 
                                 text-align: center; font-size: 24px; font-weight: bold; margin: 20px 0; }}
                        .footer {{ text-align: center; padding: 20px; color: #777; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>Đặt lại mật khẩu</h1>
                        </div>
                        <div class='content'>
                            <h2>Xin chào {request.Username}!</h2>
                            <p>Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản của mình.</p>
                            <p>Mã xác nhận của bạn là:</p>
                            <div class='code'>{request.ResetCode}</div>
                            <p>Mã này sẽ hết hạn sau 15 phút.</p>
                            <p>Nếu bạn không yêu cầu đặt lại mật khẩu, vui lòng bỏ qua email này.</p>
                        </div>
                        <div class='footer'>
                            <p>&copy; 2026 Gunny Game. All rights reserved.</p>
                        </div>
                    </div>
                </body>
                </html>
            ";

            var result = await _emailService.SendEmailAsync(request.Email, subject, body, true);

            if (result)
            {
                return Ok(new { success = true, message = "Email reset password đã được gửi!" });
            }

            return BadRequest(new { success = false, message = "Gửi email thất bại." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gửi email reset password");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}

// Request Models
public class SendEmailRequest
{
    public string ToEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
}

public class WelcomeEmailRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordEmailRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string ResetCode { get; set; } = string.Empty;
}
