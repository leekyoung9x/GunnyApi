# Kestrel Server Configuration

## Giới thiệu

Cấu hình Kestrel server giờ đây được quản lý thông qua `appsettings.json`, cho phép dễ dàng thay đổi port và bật/tắt HTTPS mà không cần sửa code.

## Cấu hình trong appsettings.json

```json
"KestrelSettings": {
  "HttpPort": 5176,
  "HttpsPort": 7062,
  "EnableHttps": true,
  "ListenOnAllInterfaces": true
}
```

### Các tham số:

- **`HttpPort`**: Port cho HTTP (mặc định: 5176)
- **`HttpsPort`**: Port cho HTTPS (mặc định: 7062)
- **`EnableHttps`**: Bật/tắt HTTPS (mặc định: true)
- **`ListenOnAllInterfaces`**: 
  - `true`: Listen trên `0.0.0.0` (tất cả network interfaces) - có thể truy cập từ máy khác
  - `false`: Listen trên `127.0.0.1` (chỉ localhost) - chỉ truy cập từ máy local

## Các trường hợp sử dụng

### 1. Development trên máy local (chỉ localhost)
```json
"KestrelSettings": {
  "HttpPort": 5176,
  "HttpsPort": 7062,
  "EnableHttps": true,
  "ListenOnAllInterfaces": false
}
```
Truy cập: `http://localhost:5176` hoặc `https://localhost:7062`

### 2. Development với nhiều máy trong LAN (test từ mobile, máy khác)
```json
"KestrelSettings": {
  "HttpPort": 5176,
  "HttpsPort": 7062,
  "EnableHttps": true,
  "ListenOnAllInterfaces": true
}
```
Truy cập: `http://<IP_của_máy>:5176` hoặc `https://<IP_của_máy>:7062`

### 3. Production (chỉ HTTP, không HTTPS - vì có reverse proxy)
```json
"KestrelSettings": {
  "HttpPort": 5000,
  "HttpsPort": 5001,
  "EnableHttps": false,
  "ListenOnAllInterfaces": true
}
```
Truy cập: `http://<IP_của_máy>:5000`

### 4. Docker Container
```json
"KestrelSettings": {
  "HttpPort": 80,
  "HttpsPort": 443,
  "EnableHttps": false,
  "ListenOnAllInterfaces": true
}
```
**Lưu ý**: Container thường không cần HTTPS vì sẽ có reverse proxy (nginx, traefik) xử lý SSL.

## Thay đổi port nhanh

Chỉ cần sửa trong `appsettings.json`:

```json
"KestrelSettings": {
  "HttpPort": 8080,  // Thay đổi port HTTP
  "HttpsPort": 8443, // Thay đổi port HTTPS
  "EnableHttps": false, // Tắt HTTPS nếu không cần
  "ListenOnAllInterfaces": true
}
```

Sau đó restart ứng dụng, không cần build lại.

## Override cấu hình theo môi trường

Tạo file `appsettings.Development.json` hoặc `appsettings.Production.json`:

### appsettings.Development.json
```json
{
  "KestrelSettings": {
    "HttpPort": 5176,
    "HttpsPort": 7062,
    "EnableHttps": true,
    "ListenOnAllInterfaces": false
  }
}
```

### appsettings.Production.json
```json
{
  "KestrelSettings": {
    "HttpPort": 80,
    "HttpsPort": 443,
    "EnableHttps": false,
    "ListenOnAllInterfaces": true
  }
}
```

## Firewall

Nếu `ListenOnAllInterfaces = true`, đảm bảo firewall cho phép kết nối:

### Windows Firewall
```powershell
# Cho phép HTTP port
netsh advfirewall firewall add rule name="GunnyApi HTTP" dir=in action=allow protocol=TCP localport=5176

# Cho phép HTTPS port
netsh advfirewall firewall add rule name="GunnyApi HTTPS" dir=in action=allow protocol=TCP localport=7062
```

### Linux (Ubuntu/Debian)
```bash
# Cho phép HTTP port
sudo ufw allow 5176/tcp

# Cho phép HTTPS port
sudo ufw allow 7062/tcp
```

## Kiểm tra ứng dụng đang listen trên port nào

### Windows
```powershell
netstat -ano | findstr :5176
```

### Linux
```bash
netstat -tuln | grep :5176
```

## Best Practices

1. **Development**: `ListenOnAllInterfaces = false` (an toàn hơn)
2. **Staging/Production**: `ListenOnAllInterfaces = true` 
3. **Behind Reverse Proxy**: `EnableHttps = false` (để proxy xử lý SSL)
4. **Direct Exposure**: `EnableHttps = true` (bắt buộc có SSL certificate)

## Troubleshooting

### Không kết nối được từ máy khác
- Kiểm tra `ListenOnAllInterfaces = true`
- Kiểm tra firewall cho phép port
- Kiểm tra IP address của máy server: `ipconfig` (Windows) hoặc `ip addr` (Linux)

### Port đã bị sử dụng
```
Error: Failed to bind to address http://0.0.0.0:5176: address already in use
```
**Giải pháp**: Thay đổi port trong `appsettings.json` hoặc stop service đang dùng port đó.

### Certificate không hợp lệ (HTTPS)
Khi `EnableHttps = true` mà không có certificate:
- Development: .NET tự động dùng development certificate
- Production: Cần cấu hình SSL certificate trong Kestrel hoặc dùng reverse proxy

## Migration từ cấu hình cũ

**Trước đây** (hardcode trong Program.cs):
```csharp
serverOptions.ListenAnyIP(5176);
serverOptions.ListenAnyIP(7062, listenOptions => 
{
    listenOptions.UseHttps();
});
```

**Bây giờ** (config trong appsettings.json):
```json
"KestrelSettings": {
  "HttpPort": 5176,
  "HttpsPort": 7062,
  "EnableHttps": true,
  "ListenOnAllInterfaces": true
}
```

✅ Linh hoạt hơn, không cần build lại code khi thay đổi port!
