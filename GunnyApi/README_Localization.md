# Hệ Thống Đa Ngôn Ngữ (Localization) - GunnyApi

## Tổng Quan

Project đã được tích hợp hệ thống đa ngôn ngữ (i18n) để hỗ trợ nhiều ngôn ngữ cho các message trả về từ API. Hiện tại hệ thống hỗ trợ:
- **Tiếng Việt** (vi) - Mặc định
- **Tiếng Anh** (en)

## Cấu Hình

### 1. File appsettings.json

```json
{
  "LocalizationSettings": {
    "DefaultLanguage": "vi",
    "SupportedLanguages": [ "vi", "en" ],
    "EnableHeaderDetection": true,
    "EnableQueryStringDetection": true
  }
}
```

**Các tùy chọn:**
- `DefaultLanguage`: Ngôn ngữ mặc định (vi hoặc en)
- `SupportedLanguages`: Danh sách các ngôn ngữ được hỗ trợ
- `EnableHeaderDetection`: Cho phép phát hiện ngôn ngữ từ Accept-Language header
- `EnableQueryStringDetection`: Cho phép phát hiện ngôn ngữ từ query string (?lang=vi)

## Sử Dụng

### 1. Từ Client API

#### Cách 1: Sử dụng Query String
```
GET /api/users?lang=vi
GET /api/users?lang=en
```

#### Cách 2: Sử dụng Accept-Language Header
```bash
curl -H "Accept-Language: vi-VN" http://localhost:5000/api/users
curl -H "Accept-Language: en-US" http://localhost:5000/api/users
```

#### Cách 3: Mặc định
Nếu không chỉ định, hệ thống sẽ sử dụng ngôn ngữ mặc định (Tiếng Việt)

### 2. Ví Dụ Response

#### Tiếng Việt (Mặc định)
```json
{
  "success": false,
  "message": "Không tìm thấy user"
}
```

#### Tiếng Anh (?lang=en)
```json
{
  "success": false,
  "message": "User not found"
}
```

## Các Message Key Hiện Có

### Common Messages
- `Error.Generic` - "Có lỗi xảy ra" / "An error occurred"
- `Error.NotFound` - "Không tìm thấy" / "Not found"
- `Success.Generic` - "Thành công" / "Success"

### User Messages
- `User.NotFound` - "Không tìm thấy user" / "User not found"
- `User.UsernameExists` - "Username '{0}' đã tồn tại" / "Username '{0}' already exists"
- `User.EmailExists` - "Email '{0}' đã tồn tại" / "Email '{0}' already exists"
- `User.UsernameRequired` - "Username không được rỗng" / "Username is required"
- `User.EmailRequired` - "Email không được rỗng" / "Email is required"
- `User.PasswordRequired` - "Password không được rỗng" / "Password is required"
- `User.AccountNotFound` - "Tài khoản <strong>{0}</strong> không tồn tại." / "Account <strong>{0}</strong> does not exist."

### Login Messages
- `Login.Success` - "Đăng nhập thành công" / "Login successful"
- `Login.Failed` - "Có lỗi xảy ra khi đăng nhập: {0}" / "An error occurred during login: {0}"
- `Login.UsernameRequired` - "Username không được rỗng" / "Username is required"
- `Login.PasswordRequired` - "Password không được rỗng" / "Password is required"
- `Login.Unauthorized` - "Không tìm thấy thông tin user từ token" / "User information not found in token"

### Register Messages
- `Register.Success` - "Đăng ký tài khoản thành công" / "Account registration successful"
- `Register.EmailExistsOrError` - "Email đã tồn tại hoặc có lỗi khi tạo tài khoản" / "Email already exists or an error occurred while creating the account"
- `Register.AccountCreatedButDetailError` - "Đã tạo tài khoản nhưng có lỗi khi tạo thông tin chi tiết người chơi" / "Account created but an error occurred while creating player details"

### Money Transfer Messages
- `Money.TransferSuccess` - "Chuyển {0} Xu thành công từ Member sang Tank qua mail. {1}" / "Successfully transferred {0} Xu from Member to Tank via mail. {1}"
- `Money.TransferSuccessDetail` - "Đã chuyển thành công <br /> {0} Xu <br /> {1} Vàng <br /> {2} Lễ kim <br /> cho tài khoản <strong>{3}</strong>{4}" / "Successfully transferred <br /> {0} Xu <br /> {1} Gold <br /> {2} Gift Token <br /> to account <strong>{3}</strong>{4}"
- `Money.MailError` - " (Lỗi thông báo: {0})" / " (Notification error: {0})"
- `Money.ErrorOccurred` - "Có lỗi xảy ra: {0}" / "An error occurred: {0}"

### Server Messages
- `Server.NotFoundOrError` - "Không tìm thấy server hoặc có lỗi xảy ra: {0}" / "Server not found or an error occurred: {0}"
- `Server.GetListSuccess` - "Lấy danh sách server thành công" / "Server list retrieved successfully"
- `Server.GetListError` - "Có lỗi xảy ra khi lấy danh sách server: {0}" / "An error occurred while retrieving server list: {0}"
- `Server.LoginSuccess` - "Đăng nhập game thành công" / "Game login successful"
- `Server.LoginError` - "Có lỗi xảy ra khi đăng nhập game: {0}" / "An error occurred during game login: {0}"
- `Server.CreateKeySuccess` - "Tạo key thành công" / "Key created successfully"
- `Server.CreateKeyError` - "Có lỗi xảy ra khi tạo key: {0}" / "An error occurred while creating key: {0}"

### Payment Messages
- `Payment.CreateCheckoutSuccess` - "Tạo phiên thanh toán thành công" / "Checkout session created successfully"
- `Payment.CreateCheckoutError` - "Có lỗi xảy ra khi tạo phiên thanh toán: {0}" / "An error occurred while creating checkout session: {0}"
- `Payment.WebhookProcessSuccess` - "Xử lý webhook thành công" / "Webhook processed successfully"
- `Payment.WebhookProcessError` - "Có lỗi xảy ra khi xử lý webhook: {0}" / "An error occurred while processing webhook: {0}"
- `Payment.InvalidRequest` - "Dữ liệu webhook không hợp lệ" / "Invalid webhook data"
- `Payment.InvalidSignature` - "Chữ ký webhook không hợp lệ" / "Invalid webhook signature"

## Thêm Message Key Mới

Để thêm message key mới, cập nhật file `Infrastructure/Services/LocalizationService.cs`:

```csharp
private void LoadTranslations()
{
    // Vietnamese translations
    _translations["vi"] = new Dictionary<string, string>
    {
        // ... existing keys
        ["YourNewKey"] = "Tin nhắn tiếng Việt"
    };

    // English translations
    _translations["en"] = new Dictionary<string, string>
    {
        // ... existing keys
        ["YourNewKey"] = "English message"
    };
}
```

## Sử Dụng Trong Code

### Trong Service
```csharp
public class YourService
{
    private readonly ILocalizationService _localization;

    public YourService(ILocalizationService localization)
    {
        _localization = localization;
    }

    public void YourMethod()
    {
        // Simple message
        string message = _localization.GetString("User.NotFound");
        
        // Message với parameters
        string message = _localization.GetString("User.UsernameExists", "john_doe");
    }
}
```

### Trong Controller
```csharp
public class YourController : ControllerBase
{
    private readonly ILocalizationService _localization;

    public YourController(ILocalizationService localization)
    {
        _localization = localization;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return NotFound(new { 
            message = _localization.GetString("User.NotFound") 
        });
    }
}
```

## Kiến Trúc

### 1. LocalizationService
- `ILocalizationService`: Interface định nghĩa các phương thức
- `LocalizationService`: Implementation lưu trữ và quản lý translations
- Đăng ký như Singleton trong Program.cs

### 2. LocalizationSettings
- Class chứa cấu hình từ appsettings.json
- Định nghĩa ngôn ngữ mặc định và các ngôn ngữ hỗ trợ

### 3. LanguageDetectionMiddleware
- Middleware tự động phát hiện ngôn ngữ từ request
- Priority: Query String > Accept-Language Header > Default Language

## Testing

### Test với Postman
1. Không header (mặc định tiếng Việt):
```
GET http://localhost:5000/api/users/1
```

2. Với query string:
```
GET http://localhost:5000/api/users/1?lang=en
```

3. Với header:
```
GET http://localhost:5000/api/users/1
Headers:
  Accept-Language: en-US
```

### Test với cURL
```bash
# Tiếng Việt (mặc định)
curl http://localhost:5000/api/users/1

# Tiếng Anh (query string)
curl "http://localhost:5000/api/users/1?lang=en"

# Tiếng Anh (header)
curl -H "Accept-Language: en-US" http://localhost:5000/api/users/1
```

## Lưu Ý

1. **Thứ tự ưu tiên ngôn ngữ:**
   - Query String (?lang=vi) - Cao nhất
   - Accept-Language Header - Trung bình
   - Default Language (từ config) - Thấp nhất

2. **Thêm ngôn ngữ mới:**
   - Cập nhật `SupportedLanguages` trong appsettings.json
   - Thêm translations vào LocalizationService
   - Test kỹ các message key

3. **Performance:**
   - LocalizationService được đăng ký như Singleton
   - Translations được load một lần khi khởi động
   - Không có I/O operations trong runtime

4. **Best Practices:**
   - Sử dụng message keys có ý nghĩa (e.g., "User.NotFound")
   - Group keys theo module/feature
   - Luôn có fallback message nếu key không tồn tại
   - Test cả hai ngôn ngữ sau khi thêm message mới
