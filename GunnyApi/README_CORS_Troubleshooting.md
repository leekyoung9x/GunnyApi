# CORS Configuration & Troubleshooting Guide

## Vấn đề đã fix

### Lỗi CORS ban đầu
```
Access to XMLHttpRequest at 'https://api2.ddtanklegend.online/api/Users/login' 
from origin 'https://demo.ddtanklegend.online' has been blocked by CORS policy: 
Response to preflight request doesn't pass access control check: 
No 'Access-Control-Allow-Origin' header is present on the requested resource.
```

### Nguyên nhân
1. **Origin chưa được thêm vào AllowedOrigins**
2. **Dấu gạch chéo cuối (`/`) không khớp** - Browser gửi `https://demo.ddtanklegend.online` nhưng config có `https://demo.ddtanklegend.online/`

### Giải pháp
Đã cập nhật `appsettings.json`:
```json
"CorsSettings": {
  "PolicyName": "AllowLocalhost",
  "AllowedOrigins": [
    "http://localhost",
    "http://localhost:3000",
    "http://localhost:4200",
    "https://demo.ddtanklegend.online",
    "https://api2.ddtanklegend.online"
  ],
  "AllowCredentials": true
}
```

**Lưu ý quan trọng**: 
- ❌ `"https://demo.ddtanklegend.online/"` (có dấu `/` cuối)
- ✅ `"https://demo.ddtanklegend.online"` (không có dấu `/` cuối)

## Cấu hình CORS hiện tại

### Trong appsettings.json
```json
"CorsSettings": {
  "PolicyName": "AllowLocalhost",
  "AllowedOrigins": [
    "http://localhost",
    "http://localhost:3000",
    "http://localhost:4200",
    "https://demo.ddtanklegend.online",
    "https://api2.ddtanklegend.online"
  ],
  "AllowCredentials": true
}
```

### Các tham số

- **`PolicyName`**: Tên của CORS policy
- **`AllowedOrigins`**: Danh sách các origin được phép truy cập API
- **`AllowCredentials`**: Cho phép gửi credentials (cookies, authorization headers)

## Thêm origin mới

Để thêm một origin mới, chỉ cần cập nhật `appsettings.json`:

```json
"AllowedOrigins": [
  "http://localhost",
  "https://demo.ddtanklegend.online",
  "https://new-domain.com",           // ← Thêm domain mới
  "https://another-domain.com"        // ← Thêm nhiều domain
]
```

**Lưu ý**: Không cần dấu `/` ở cuối URL!

## CORS cho tất cả origin (không khuyến khích cho production)

Nếu muốn cho phép **TẤT CẢ** origin (chỉ dùng cho development/testing):

### Cách 1: Sửa Program.cs (tạm thời)
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policyBuilder =>
        {
            policyBuilder
                .AllowAnyOrigin()      // ← Cho phép tất cả origin
                .AllowAnyHeader()
                .AllowAnyMethod();
            // Không dùng AllowCredentials() với AllowAnyOrigin()
        });
});

// Sau đó dùng policy này
app.UseCors("AllowAll");
```

### Cách 2: Wildcard trong appsettings.json (khuyến nghị hơn)
Tạo một setting mới:
```json
"CorsSettings": {
  "PolicyName": "AllowLocalhost",
  "AllowAllOrigins": false,  // ← Thêm flag này
  "AllowedOrigins": [
    "https://demo.ddtanklegend.online"
  ],
  "AllowCredentials": true
}
```

Sau đó update `Program.cs`:
```csharp
if (corsSettings.AllowAllOrigins)
{
    policyBuilder
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
}
else
{
    policyBuilder
        .WithOrigins(corsSettings.AllowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod();
    
    if (corsSettings.AllowCredentials)
    {
        policyBuilder.AllowCredentials();
    }
}
```

## Kiểm tra CORS có hoạt động không

### 1. Kiểm tra trong browser console
```javascript
fetch('https://api2.ddtanklegend.online/api/Users/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({ username: 'test', password: 'test' })
})
.then(response => response.json())
.then(data => console.log(data))
.catch(error => console.error('Error:', error));
```

### 2. Kiểm tra Response Headers
Mở DevTools → Network → Chọn request → Headers

**Response Headers phải có**:
```
Access-Control-Allow-Origin: https://demo.ddtanklegend.online
Access-Control-Allow-Credentials: true
Access-Control-Allow-Methods: GET, POST, PUT, DELETE, OPTIONS
Access-Control-Allow-Headers: Content-Type, Authorization, ...
```

### 3. Test với curl
```bash
curl -X OPTIONS https://api2.ddtanklegend.online/api/Users/login \
  -H "Origin: https://demo.ddtanklegend.online" \
  -H "Access-Control-Request-Method: POST" \
  -H "Access-Control-Request-Headers: Content-Type" \
  -v
```

Phải thấy response headers như trên.

## Các lỗi CORS thường gặp

### 1. Dấu gạch chéo cuối (trailing slash)
**❌ Sai**:
```json
"AllowedOrigins": ["https://demo.ddtanklegend.online/"]
```

**✅ Đúng**:
```json
"AllowedOrigins": ["https://demo.ddtanklegend.online"]
```

### 2. Protocol không khớp (http vs https)
**❌ Sai**:
```json
"AllowedOrigins": ["http://demo.ddtanklegend.online"]  // http
// Nhưng frontend dùng: https://demo.ddtanklegend.online
```

**✅ Đúng**:
```json
"AllowedOrigins": ["https://demo.ddtanklegend.online"]  // https
```

### 3. Port không khớp
**❌ Sai**:
```json
"AllowedOrigins": ["http://localhost"]  
// Nhưng frontend chạy trên: http://localhost:3000
```

**✅ Đúng**:
```json
"AllowedOrigins": [
  "http://localhost",
  "http://localhost:3000"
]
```

### 4. Subdomain khác nhau
**Lưu ý**: Mỗi subdomain là một origin khác nhau!

```json
"AllowedOrigins": [
  "https://demo.ddtanklegend.online",      // Origin 1
  "https://api.ddtanklegend.online",       // Origin 2 (khác!)
  "https://admin.ddtanklegend.online"      // Origin 3 (khác!)
]
```

### 5. AllowCredentials với AllowAnyOrigin
**❌ Không được phép**:
```csharp
policyBuilder
    .AllowAnyOrigin()
    .AllowCredentials();  // ← LỖI: Không thể dùng cả 2
```

**✅ Phải chọn 1 trong 2**:
```csharp
// Option 1: Specific origins + credentials
policyBuilder
    .WithOrigins("https://demo.ddtanklegend.online")
    .AllowCredentials();

// Option 2: Any origin + NO credentials
policyBuilder
    .AllowAnyOrigin();
```

## Production Best Practices

### 1. Chỉ liệt kê origins cần thiết
```json
"AllowedOrigins": [
  "https://demo.ddtanklegend.online",
  "https://www.ddtanklegend.online"
]
```

### 2. Tách môi trường
**appsettings.Development.json**:
```json
"CorsSettings": {
  "AllowedOrigins": [
    "http://localhost:3000",
    "http://localhost:4200"
  ]
}
```

**appsettings.Production.json**:
```json
"CorsSettings": {
  "AllowedOrigins": [
    "https://demo.ddtanklegend.online",
    "https://www.ddtanklegend.online"
  ]
}
```

### 3. Environment Variables
Nếu deploy lên nhiều môi trường, dùng environment variables:

```json
"CorsSettings": {
  "AllowedOrigins": "${ALLOWED_ORIGINS}"
}
```

Sau đó set biến môi trường:
```bash
# Development
export ALLOWED_ORIGINS="http://localhost:3000,http://localhost:4200"

# Production
export ALLOWED_ORIGINS="https://demo.ddtanklegend.online,https://www.ddtanklegend.online"
```

## Monitoring & Debugging

### 1. Log CORS requests
Thêm vào `Program.cs`:
```csharp
app.Use(async (context, next) =>
{
    var origin = context.Request.Headers["Origin"].ToString();
    if (!string.IsNullOrEmpty(origin))
    {
        Console.WriteLine($"CORS Request from: {origin}");
    }
    await next();
});
```

### 2. Enable CORS logging
Trong `appsettings.json`:
```json
"Logging": {
  "LogLevel": {
    "Default": "Information",
    "Microsoft.AspNetCore.Cors": "Debug"
  }
}
```

## Quick Checklist

Khi gặp lỗi CORS, kiểm tra:

- [ ] Origin có trong `AllowedOrigins` không?
- [ ] Không có dấu `/` thừa ở cuối URL?
- [ ] Protocol đúng (http vs https)?
- [ ] Port đúng (nếu có)?
- [ ] `UseCors()` được gọi trước `UseAuthentication()`?
- [ ] Request method được phép (GET, POST, etc.)?
- [ ] Headers được phép?
- [ ] Credentials setting đúng?

## Restart Required

Sau khi thay đổi `appsettings.json`, **BẮT BUỘC phải restart** ứng dụng để áp dụng cấu hình mới!

```bash
# Stop ứng dụng đang chạy, sau đó:
dotnet run --configuration Release
```

Hoặc nếu deploy trên server/IIS, restart service/app pool.
