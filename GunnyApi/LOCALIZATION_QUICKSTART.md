# Hướng Dẫn Sử Dụng Đa Ngôn Ngữ - Quick Guide

## Cách Sử Dụng

### 1. Chọn ngôn ngữ bằng Query String
```
http://localhost:5000/api/users?lang=vi  (Tiếng Việt)
http://localhost:5000/api/users?lang=en  (Tiếng Anh)
```

### 2. Chọn ngôn ngữ bằng Header
```
Accept-Language: vi-VN  (Tiếng Việt)
Accept-Language: en-US  (Tiếng Anh)
```

### 3. Mặc định
Nếu không chỉ định, hệ thống sử dụng Tiếng Việt

## Ví Dụ Response

### Tiếng Việt
```json
{
  "success": false,
  "message": "Không tìm thấy user"
}
```

### Tiếng Anh
```json
{
  "success": false,
  "message": "User not found"
}
```

## Cấu Hình

File `appsettings.json`:
```json
{
  "LocalizationSettings": {
    "DefaultLanguage": "vi",
    "SupportedLanguages": [ "vi", "en" ]
  }
}
```

## Files Đã Tạo

1. **ILocalizationService.cs** - Interface cho localization service
2. **LocalizationService.cs** - Implementation với translations cho vi/en
3. **LocalizationSettings.cs** - Settings class
4. **LanguageDetectionMiddleware.cs** - Middleware tự động detect ngôn ngữ
5. **README_Localization.md** - Hướng dẫn chi tiết

## Đã Cập Nhật

- ✅ `UserService.cs` - Tất cả messages
- ✅ `UsersController.cs` - Tất cả error messages
- ✅ `appsettings.json` - Thêm LocalizationSettings
- ✅ `Program.cs` - Đăng ký services và middleware

Xem file `README_Localization.md` để biết chi tiết về tất cả message keys và cách thêm key mới.
