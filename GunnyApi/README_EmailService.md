# Email Service - Hướng dẫn sử dụng

## Tổng quan
Email Service cho phép gửi email thông qua SMTP server. Service hỗ trợ gửi email đơn, gửi đến nhiều người nhận, và gửi kèm file đính kèm.

## Cấu hình

### 1. Cấu hình trong appsettings.json

```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SenderEmail": "your-email@gmail.com",
  "SenderName": "Gunny Game",
  "Password": "your-app-password",
  "EnableSsl": true,
  "Timeout": 30000,
  "BccEmail": ""
}
```

### 2. Cấu hình cho Gmail

Để sử dụng Gmail SMTP:

1. **Bật xác thực 2 bước** cho tài khoản Gmail
2. **Tạo App Password**:
   - Vào [Google Account](https://myaccount.google.com/)
   - Chọn Security → 2-Step Verification → App passwords
   - Tạo password mới cho "Mail"
   - Copy password này vào `Password` trong cấu hình

3. **Cấu hình Gmail SMTP**:
```json
"EmailSettings": {
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": 587,
  "SenderEmail": "your-email@gmail.com",
  "SenderName": "Gunny Game",
  "Password": "xxxx xxxx xxxx xxxx",
  "EnableSsl": true,
  "Timeout": 30000
}
```

### 3. Cấu hình cho các SMTP khác

#### Outlook/Hotmail
```json
"EmailSettings": {
  "SmtpServer": "smtp-mail.outlook.com",
  "SmtpPort": 587,
  "SenderEmail": "your-email@outlook.com",
  "SenderName": "Gunny Game",
  "Password": "your-password",
  "EnableSsl": true
}
```

#### Yahoo
```json
"EmailSettings": {
  "SmtpServer": "smtp.mail.yahoo.com",
  "SmtpPort": 587,
  "SenderEmail": "your-email@yahoo.com",
  "SenderName": "Gunny Game",
  "Password": "your-app-password",
  "EnableSsl": true
}
```

#### Custom SMTP Server
```json
"EmailSettings": {
  "SmtpServer": "mail.yourdomain.com",
  "SmtpPort": 587,
  "SenderEmail": "noreply@yourdomain.com",
  "SenderName": "Your Company",
  "Password": "your-password",
  "EnableSsl": true
}
```

## Sử dụng trong Code

### 1. Inject Service

```csharp
public class YourController : ControllerBase
{
    private readonly IEmailService _emailService;

    public YourController(IEmailService emailService)
    {
        _emailService = emailService;
    }
}
```

### 2. Gửi Email đơn giản

```csharp
// Gửi email HTML
var result = await _emailService.SendEmailAsync(
    "user@example.com",
    "Tiêu đề email",
    "<h1>Nội dung HTML</h1><p>Đây là email test</p>",
    isHtml: true
);

// Gửi email Plain Text
var result = await _emailService.SendEmailAsync(
    "user@example.com",
    "Tiêu đề email",
    "Nội dung text thuần túy",
    isHtml: false
);
```

### 3. Gửi Email đến nhiều người

```csharp
var recipients = new List<string>
{
    "user1@example.com",
    "user2@example.com",
    "user3@example.com"
};

var result = await _emailService.SendEmailToMultipleAsync(
    recipients,
    "Tiêu đề email",
    "<h1>Nội dung cho tất cả</h1>",
    isHtml: true
);
```

### 4. Gửi Email với File đính kèm

```csharp
var attachmentPath = "C:\\path\\to\\file.pdf";

var result = await _emailService.SendEmailWithAttachmentAsync(
    "user@example.com",
    "Email với attachment",
    "<h1>Xem file đính kèm</h1>",
    attachmentPath,
    isHtml: true
);
```

### 5. Template Email chuyên nghiệp

```csharp
private string GetWelcomeEmailTemplate(string username, string email)
{
    return $@"
        <!DOCTYPE html>
        <html>
        <head>
            <style>
                body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
                           color: white; padding: 30px; text-align: center; }}
                .content {{ padding: 30px; background-color: #f9f9f9; }}
                .button {{ display: inline-block; padding: 12px 30px;
                          background-color: #667eea; color: white;
                          text-decoration: none; border-radius: 5px; }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>🎮 Chào mừng đến Gunny!</h1>
                </div>
                <div class='content'>
                    <h2>Xin chào {username}!</h2>
                    <p>Cảm ơn bạn đã tham gia cộng đồng game của chúng tôi.</p>
                    <p><strong>Email:</strong> {email}</p>
                    <a href='#' class='button'>Bắt đầu chơi ngay</a>
                </div>
            </div>
        </body>
        </html>
    ";
}

// Sử dụng
var htmlContent = GetWelcomeEmailTemplate("PlayerName", "player@email.com");
await _emailService.SendEmailAsync(
    "player@email.com",
    "Chào mừng bạn!",
    htmlContent,
    true
);
```

## API Endpoints

### 1. Test Email
```http
POST /api/email/send-test
Content-Type: application/json

{
  "toEmail": "test@example.com",
  "subject": "Test Email",
  "body": "<h1>Hello World</h1>",
  "isHtml": true
}
```

### 2. Email chào mừng
```http
POST /api/email/send-welcome
Content-Type: application/json

{
  "username": "PlayerName",
  "email": "player@example.com"
}
```

### 3. Email reset password
```http
POST /api/email/send-reset-password
Content-Type: application/json

{
  "username": "PlayerName",
  "email": "player@example.com",
  "resetCode": "123456"
}
```

## Best Practices

### 1. Xử lý lỗi
```csharp
try
{
    var result = await _emailService.SendEmailAsync(email, subject, body);
    if (!result)
    {
        _logger.LogWarning("Email gửi thất bại đến {Email}", email);
        // Handle failure
    }
}
catch (Exception ex)
{
    _logger.LogError(ex, "Lỗi khi gửi email");
    // Handle exception
}
```

### 2. Gửi email bất đồng bộ (Background)
```csharp
// Không chờ kết quả
_ = Task.Run(async () =>
{
    await _emailService.SendEmailAsync(email, subject, body);
});
```

### 3. Sử dụng BCC cho admin
Cấu hình `BccEmail` trong appsettings.json để tự động nhận bản sao mọi email:

```json
"EmailSettings": {
  "BccEmail": "admin@yourdomain.com"
}
```

### 4. Email Rate Limiting
Để tránh spam, nên implement rate limiting:

```csharp
// TODO: Implement rate limiting
// Ví dụ: Giới hạn 10 email/phút cho mỗi user
```

## Troubleshooting

### Lỗi "Authentication failed"
- Kiểm tra email và password
- Gmail: Đảm bảo sử dụng App Password, không phải password thường
- Kiểm tra 2-Step Verification đã được bật

### Lỗi "Connection timeout"
- Kiểm tra port SMTP (587 hoặc 465)
- Kiểm tra firewall có block không
- Thử tăng `Timeout` trong cấu hình

### Email vào Spam
- Cấu hình SPF, DKIM, DMARC cho domain
- Tránh từ ngữ spam trong tiêu đề và nội dung
- Sử dụng HTML template chuyên nghiệp

### Lỗi SSL/TLS
- Đảm bảo `EnableSsl: true` cho port 587
- Port 465 thường dùng implicit SSL
- Port 25 thường không mã hóa (không khuyến khích)

## Bảo mật

### 1. Không commit password vào Git
Sử dụng `appsettings.Local.json` hoặc User Secrets:

```bash
dotnet user-secrets set "EmailSettings:Password" "your-password"
```

### 2. Sử dụng Environment Variables
```bash
# Windows
set EmailSettings__Password=your-password

# Linux/Mac
export EmailSettings__Password=your-password
```

### 3. Azure Key Vault (Production)
```csharp
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential()
);
```

## Testing

### Test với MailTrap.io (Development)
```json
"EmailSettings": {
  "SmtpServer": "smtp.mailtrap.io",
  "SmtpPort": 587,
  "SenderEmail": "test@example.com",
  "SenderName": "Test Sender",
  "Password": "your-mailtrap-password",
  "EnableSsl": true
}
```

## Limitations

- **Gmail**: Giới hạn 500 email/ngày cho tài khoản miễn phí
- **Outlook**: Giới hạn 300 email/ngày
- **Yahoo**: Giới hạn 500 email/ngày

Để gửi số lượng lớn email, nên sử dụng:
- SendGrid
- Amazon SES
- Mailgun
- Postmark

## License
MIT License - Copyright (c) 2026 Gunny Game
