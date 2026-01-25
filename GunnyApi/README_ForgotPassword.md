# Forgot Password API - Hướng dẫn sử dụng

## Tổng quan
API Forgot Password cho phép người dùng đặt lại mật khẩu thông qua email xác thực. Quy trình bao gồm:
1. Người dùng yêu cầu reset password với email
2. Hệ thống gửi link reset password qua email
3. Người dùng click link và nhập mật khẩu mới
4. Mật khẩu được cập nhật

## Setup Database

### 1. Chạy SQL Script
Chạy file [Database/PasswordReset.sql](Database/PasswordReset.sql) để tạo:
- Bảng `PasswordResets`
- Stored procedures: `sp_CreatePasswordResetToken`, `sp_VerifyPasswordResetToken`, `sp_MarkPasswordResetTokenAsUsed`

```sql
-- Chạy script
USE [Db_Member1]
GO
-- Copy và chạy toàn bộ nội dung PasswordReset.sql
```

### 2. Cấu hình Frontend URL
Thêm vào `appsettings.json`:

```json
{
  "FrontendUrl": "http://localhost:3000",
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderName": "Gunny Game",
    "Password": "your-app-password",
    "EnableSsl": true
  }
}
```

## API Endpoints

### 1. Request Reset Password
Gửi email chứa link reset password đến user.

**Endpoint:** `POST /api/users/forgot-password`

**Request:**
```json
{
  "email": "user@example.com"
}
```

**Response - Success:**
```json
{
  "success": true,
  "message": "Nếu email tồn tại, chúng tôi đã gửi link đặt lại mật khẩu đến email của bạn."
}
```

**Response - Error:**
```json
{
  "success": false,
  "message": "Email không hợp lệ"
}
```

**Notes:**
- API luôn trả về message success dù email có tồn tại hay không (security best practice)
- Token có hiệu lực 1 giờ
- Email chứa link: `http://localhost:3000/reset-password?token=xxx`

---

### 2. Verify Reset Token
Kiểm tra token có hợp lệ không (optional - dùng để validate trước khi show form).

**Endpoint:** `GET /api/users/verify-reset-token?token={token}`

**Response - Valid:**
```json
{
  "isValid": true,
  "message": "Token hợp lệ",
  "email": "user@example.com"
}
```

**Response - Invalid:**
```json
{
  "isValid": false,
  "message": "Token không hợp lệ hoặc đã hết hạn"
}
```

---

### 3. Reset Password
Đặt lại mật khẩu mới với token.

**Endpoint:** `POST /api/users/reset-password`

**Request:**
```json
{
  "token": "abc123xyz...",
  "newPassword": "NewPassword123",
  "confirmPassword": "NewPassword123"
}
```

**Response - Success:**
```json
{
  "success": true,
  "message": "Đặt lại mật khẩu thành công!"
}
```

**Response - Error:**
```json
{
  "success": false,
  "message": "Token không hợp lệ hoặc đã hết hạn"
}
```

## Email Template

Email được gửi sẽ có dạng:

```
====================================
🔐 Đặt lại mật khẩu
====================================

Xin chào [Username]!

Bạn đã yêu cầu đặt lại mật khẩu cho tài khoản Gunny Game.

[Đặt lại mật khẩu] (Button)

Hoặc copy link sau:
http://localhost:3000/reset-password?token=xxx

⚠️ Lưu ý:
- Link hết hạn sau 1 giờ
- Chỉ sử dụng được 1 lần
- Không chia sẻ với ai

Nếu không yêu cầu, vui lòng bỏ qua email này.
====================================
```

## Frontend Integration

### React Example

```jsx
// Page: ForgotPassword.jsx
import { useState } from 'react';

export default function ForgotPassword() {
  const [email, setEmail] = useState('');
  const [message, setMessage] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);

    try {
      const response = await fetch('http://localhost:5176/api/users/forgot-password', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email })
      });

      const data = await response.json();
      setMessage(data.message);
    } catch (error) {
      setMessage('Có lỗi xảy ra. Vui lòng thử lại.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="forgot-password">
      <h2>Quên mật khẩu</h2>
      <form onSubmit={handleSubmit}>
        <input
          type="email"
          placeholder="Email của bạn"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          required
        />
        <button type="submit" disabled={loading}>
          {loading ? 'Đang gửi...' : 'Gửi link reset'}
        </button>
      </form>
      {message && <p>{message}</p>}
    </div>
  );
}
```

```jsx
// Page: ResetPassword.jsx
import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';

export default function ResetPassword() {
  const [searchParams] = useSearchParams();
  const token = searchParams.get('token');
  
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [message, setMessage] = useState('');
  const [isValid, setIsValid] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    // Verify token khi load page
    fetch(`http://localhost:5176/api/users/verify-reset-token?token=${token}`)
      .then(res => res.json())
      .then(data => {
        setIsValid(data.isValid);
        if (!data.isValid) {
          setMessage(data.message);
        }
      })
      .finally(() => setLoading(false));
  }, [token]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    
    if (newPassword !== confirmPassword) {
      setMessage('Mật khẩu xác nhận không khớp');
      return;
    }

    setLoading(true);

    try {
      const response = await fetch('http://localhost:5176/api/users/reset-password', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ token, newPassword, confirmPassword })
      });

      const data = await response.json();
      setMessage(data.message);

      if (data.success) {
        // Redirect to login sau 2 giây
        setTimeout(() => {
          window.location.href = '/login';
        }, 2000);
      }
    } catch (error) {
      setMessage('Có lỗi xảy ra. Vui lòng thử lại.');
    } finally {
      setLoading(false);
    }
  };

  if (loading) return <div>Đang kiểm tra...</div>;
  if (!isValid) return <div className="error">{message}</div>;

  return (
    <div className="reset-password">
      <h2>Đặt lại mật khẩu</h2>
      <form onSubmit={handleSubmit}>
        <input
          type="password"
          placeholder="Mật khẩu mới"
          value={newPassword}
          onChange={(e) => setNewPassword(e.target.value)}
          minLength="6"
          required
        />
        <input
          type="password"
          placeholder="Xác nhận mật khẩu"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          minLength="6"
          required
        />
        <button type="submit" disabled={loading}>
          {loading ? 'Đang xử lý...' : 'Đặt lại mật khẩu'}
        </button>
      </form>
      {message && <p>{message}</p>}
    </div>
  );
}
```

## Testing với Postman

### 1. Request Reset Password
```
POST http://localhost:5176/api/users/forgot-password
Content-Type: application/json

{
  "email": "test@example.com"
}
```

### 2. Kiểm tra email
- Mở email nhận được
- Copy token từ link hoặc URL

### 3. Verify Token (Optional)
```
GET http://localhost:5176/api/users/verify-reset-token?token=YOUR_TOKEN_HERE
```

### 4. Reset Password
```
POST http://localhost:5176/api/users/reset-password
Content-Type: application/json

{
  "token": "YOUR_TOKEN_HERE",
  "newPassword": "NewPassword123",
  "confirmPassword": "NewPassword123"
}
```

## Security Features

### 1. Token Security
- Token được generate bằng cryptographically secure random bytes
- Token dài 43 ký tự, URL-safe
- Token được hash và lưu trong database

### 2. Expiration
- Token hết hạn sau 1 giờ
- Token chỉ sử dụng được 1 lần
- Token cũ tự động bị vô hiệu hóa khi tạo token mới

### 3. SQL Injection Protection
- Tất cả inputs đều được validate
- Sử dụng parameterized queries
- Sử dụng stored procedures

### 4. Email Enumeration Protection
- API luôn trả về message thành công dù email có tồn tại hay không
- Không lộ thông tin user existence

### 5. Rate Limiting (TODO)
```csharp
// Nên thêm rate limiting để tránh abuse
// Ví dụ: Giới hạn 3 request/15 phút cho mỗi email
```

## Database Maintenance

### Cleanup Expired Tokens
Chạy stored procedure định kỳ (ví dụ: mỗi ngày):

```sql
EXEC sp_CleanupExpiredPasswordResetTokens
```

Hoặc tạo SQL Server Agent Job:

```sql
-- Tạo job tự động chạy mỗi ngày lúc 2:00 AM
USE msdb;
GO

EXEC sp_add_job
    @job_name = 'Cleanup Expired Password Reset Tokens';

EXEC sp_add_jobstep
    @job_name = 'Cleanup Expired Password Reset Tokens',
    @step_name = 'Execute Cleanup',
    @command = 'EXEC Db_Member1.dbo.sp_CleanupExpiredPasswordResetTokens',
    @database_name = 'Db_Member1';

EXEC sp_add_schedule
    @schedule_name = 'Daily at 2AM',
    @freq_type = 4,  -- Daily
    @active_start_time = 020000;  -- 2:00 AM

EXEC sp_attach_schedule
    @job_name = 'Cleanup Expired Password Reset Tokens',
    @schedule_name = 'Daily at 2AM';

EXEC sp_add_jobserver
    @job_name = 'Cleanup Expired Password Reset Tokens';
GO
```

## Troubleshooting

### Email không được gửi
1. Kiểm tra cấu hình EmailSettings
2. Kiểm tra App Password Gmail
3. Xem logs trong console
4. Kiểm tra spam folder

### Token không hợp lệ
1. Kiểm tra token đã hết hạn chưa (1 giờ)
2. Token đã được sử dụng chưa
3. Kiểm tra database bảng PasswordResets

### Frontend URL không đúng
1. Cập nhật `FrontendUrl` trong appsettings.json
2. Rebuild và restart API

## Best Practices

### 1. HTTPS Only
Trong production, chỉ sử dụng HTTPS:
```json
{
  "FrontendUrl": "https://yourdomain.com"
}
```

### 2. Custom Email Template
Tùy chỉnh email template trong [UsersController.cs](Controllers/UsersController.cs#L260-L320)

### 3. Logging
```csharp
_logger.LogInformation("Password reset requested for email: {Email}", request.Email);
_logger.LogInformation("Password reset successful for token: {Token}", token);
```

### 4. Notification Email After Reset
Gửi thêm email thông báo sau khi reset thành công:

```csharp
// Sau khi reset thành công
await _emailService.SendEmailAsync(
    email,
    "Mật khẩu đã được thay đổi",
    "Mật khẩu của bạn vừa được thay đổi. Nếu không phải bạn, vui lòng liên hệ ngay."
);
```

## License
MIT License - Copyright (c) 2026 Gunny Game
