# Charge Money API - Summary of Changes

## Tổng Quan Thay Đổi

Đã implement thêm API Charge Money vào hệ thống GunnyApi với cơ chế tương tự như Login Game API, cho phép nạp tiền vào tài khoản game một cách an toàn.

## Files Đã Thêm/Sửa

### 1. Configuration Files

#### `appsettings.json`
**Thay đổi:**
- Thêm `BaseRequestUrl`: `"http://127.0.0.1/request/"`
- Thêm `ChargeKey`: Key dùng để tạo hash verification
- Tối ưu `LoginUrl` để sử dụng BaseRequestUrl

**Trước:**
```json
"GameSettings": {
  "LoginUrl": "http://127.0.0.1/request/CreateLogin.aspx",
  ...
}
```

**Sau:**
```json
"GameSettings": {
  "BaseRequestUrl": "http://127.0.0.1/request/",
  "LoginUrl": "http://127.0.0.1/request/CreateLogin.aspx",
  "ChargeKey": "QY-16-WAN-0668-2555555-7ROAD-ditmemaydiloz-12345678@Abc789-gtk",
  ...
}
```

### 2. Settings Class

#### `Infrastructure/Settings/GameSettings.cs`
**Thêm:**
- Property `BaseRequestUrl`
- Property `ChargeKey`

```csharp
public class GameSettings
{
    public string BaseRequestUrl { get; set; } = string.Empty;
    public string ChargeKey { get; set; } = string.Empty;
    // ... existing properties
}
```

### 3. Models

#### `Models/ChargeMoneyRequest.cs` (NEW)
Model cho request nạp tiền:
```csharp
public class ChargeMoneyRequest
{
    public string ChargeID { get; set; }
    public string Username { get; set; }
    public int Money { get; set; }
    public string Type { get; set; }
    public decimal NeedMoney { get; set; }
}
```

#### `Models/ChargeMoneyResponse.cs` (NEW)
Model cho response:
```csharp
public class ChargeMoneyResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string? Content { get; set; }
    public string? RequestUrl { get; set; }
}
```

### 4. Controller

#### `Controllers/UsersController.cs`
**Thêm endpoint mới:**
```csharp
[HttpPost("charge-money")]
public async Task<IActionResult> ChargeMoney([FromBody] ChargeMoneyRequest request)
```

**Chức năng:**
- Validate request (ChargeID, Username, Money)
- Tạo hash verification: MD5(chargeID + username + money + type + needMoney + key)
- Tạo content: `chargeID|username|money|type|needMoney|hash`
- Gọi game server API: `{BaseRequestUrl}ChargeMoney.aspx?content={encodedContent}`
- Trả về kết quả

### 5. Localization

#### `Infrastructure/Services/LocalizationService.cs`
**Thêm các message keys mới:**

Vietnamese:
- `Server.ChargeIDRequired`: "Mã giao dịch không được để trống"
- `Server.InvalidAmount`: "Số tiền không hợp lệ"
- `Server.ChargeSuccess`: "Nạp tiền thành công"
- `Server.ChargeFailed`: "Nạp tiền thất bại: {0}"
- `Server.ChargeError`: "Có lỗi xảy ra khi nạp tiền: {0}"

English:
- `Server.ChargeIDRequired`: "Transaction ID is required"
- `Server.InvalidAmount`: "Invalid amount"
- `Server.ChargeSuccess`: "Charge successful"
- `Server.ChargeFailed`: "Charge failed: {0}"
- `Server.ChargeError`: "An error occurred during charge: {0}"

### 6. Documentation Files (NEW)

#### `CHARGE_MONEY_API.md`
Tài liệu API hoàn chỉnh bằng tiếng Anh:
- Endpoint details
- Request/Response format
- Error codes
- Examples (cURL, JavaScript, C#, Python)
- Security considerations

#### `CHARGE_MONEY_GUIDE_VI.md`
Hướng dẫn chi tiết bằng tiếng Việt:
- Cơ chế hoạt động
- Cấu hình chi tiết
- Ví dụ tích hợp
- Troubleshooting
- FAQ

#### `GunnyApi/charge-money.http`
File test HTTP requests cho VS Code REST Client:
- Multiple test scenarios
- Success cases
- Error cases
- Different payment types

## Cơ Chế Hoạt Động

### Flow Diagram
```
Client Request
     ↓
[Validate Input]
     ↓
[Generate Hash]
     ↓
[Create Content String]
     ↓
[URL Encode]
     ↓
[Call Game Server]
     ↓
[Return Result]
```

### Content Format
```
chargeID|username|money|type|needMoney|hash
```

Trong đó:
- `hash = MD5(chargeID + username + money + type + needMoney + ChargeKey)`

### Example
```
Input:
- chargeID: "TXN001"
- username: "player123"
- money: 10000
- type: "paymongo"
- needMoney: 299.00
- ChargeKey: "secret-key-123"

Hash calculation:
MD5("TXN001" + "player123" + "10000" + "paymongo" + "299" + "secret-key-123")
= "a1b2c3d4e5f6..."

Content:
"TXN001|player123|10000|paymongo|299|a1b2c3d4e5f6..."

Request URL:
http://127.0.0.1/request/ChargeMoney.aspx?content=TXN001%7Cplayer123%7C10000%7C...
```

## API Endpoint

### Request
```http
POST /api/users/charge-money
Authorization: Bearer {JWT_TOKEN}
Content-Type: application/json
Accept-Language: vi-VN

{
  "chargeID": "TXN20260214001",
  "username": "player123",
  "money": 10000,
  "type": "paymongo",
  "needMoney": 299.00
}
```

### Response (Success)
```json
{
  "success": true,
  "message": "Nạp tiền thành công",
  "content": "TXN20260214001|player123|10000|paymongo|299|a1b2c3d4e5f6...",
  "requestUrl": "http://127.0.0.1/request/ChargeMoney.aspx?content=..."
}
```

### Response (Error)
```json
{
  "success": false,
  "message": "Mã giao dịch không được để trống"
}
```

## Error Codes

| Code | Description | Solution |
|------|-------------|----------|
| 0 | Success | - |
| 2 | Username empty | Provide valid username |
| 3 | Invalid amount | Money must be > 0 |
| 5 | Invalid IP | Add API server IP to whitelist |
| 6 | ChargeKey not configured | Configure ChargeKey in appsettings |
| 7 | Hash verification failed | Sync ChargeKey between API and Game Server |
| 8 | Invalid content format | Check content string format |

## Security Features

1. **JWT Authentication**: Endpoint requires valid JWT token
2. **Hash Verification**: MD5 hash để verify data integrity
3. **ChargeKey**: Secret key không được expose
4. **Unique ChargeID**: Mỗi transaction phải có unique ID
5. **IP Whitelist**: Game server validate IP source
6. **Input Validation**: Validate tất cả input fields

## Configuration Requirements

### API Server (appsettings.json)
```json
{
  "GameSettings": {
    "BaseRequestUrl": "http://127.0.0.1/request/",
    "ChargeKey": "YOUR_SECRET_KEY_HERE"
  }
}
```

### Game Server (web.config)
```xml
<appSettings>
  <add key="ChargeKey" value="YOUR_SECRET_KEY_HERE" />
  <add key="ChargeIP" value="127.0.0.1|192.168.1.100" />
</appSettings>
```

**⚠️ QUAN TRỌNG:** ChargeKey PHẢI GIỐNG NHAU trên cả API Server và Game Server!

## Testing

### Using REST Client (VS Code)
1. Mở file `charge-money.http`
2. Replace `YOUR_JWT_TOKEN_HERE` với token thực
3. Click "Send Request"

### Using Postman
1. Import collection từ documentation
2. Set Authorization header
3. Send POST request

### Using cURL
```bash
curl -X POST "http://localhost:5000/api/users/charge-money" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "chargeID": "TEST001",
    "username": "testuser",
    "money": 10000,
    "type": "test",
    "needMoney": 299.00
  }'
```

## Integration Examples

### React/JavaScript
```javascript
const result = await fetch('/api/users/charge-money', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    chargeID: `TXN${Date.now()}`,
    username: 'player123',
    money: 10000,
    type: 'paymongo',
    needMoney: 299.00
  })
});
```

### C#
```csharp
var request = new ChargeMoneyRequest
{
    ChargeID = $"TXN{DateTime.Now:yyyyMMddHHmmss}",
    Username = "player123",
    Money = 10000,
    Type = "paymongo",
    NeedMoney = 299.00m
};

var result = await _httpClient.PostAsJsonAsync(
    "/api/users/charge-money", 
    request
);
```

## Deployment Checklist

- [ ] Cập nhật ChargeKey trong appsettings.json (production)
- [ ] Sync ChargeKey với Game Server
- [ ] Cấu hình BaseRequestUrl đúng environment
- [ ] Thêm API server IP vào whitelist của Game Server
- [ ] Test connection giữa API Server và Game Server
- [ ] Verify hash calculation works correctly
- [ ] Test với real payment gateway
- [ ] Setup monitoring và logging
- [ ] Configure HTTPS cho production
- [ ] Test error handling và retry logic

## Troubleshooting

### Issue: "Hash verification failed"
**Solution:** Kiểm tra ChargeKey có giống nhau giữa API và Game Server không

### Issue: "Invalid IP"
**Solution:** Thêm IP của API Server vào ChargeIP config của Game Server

### Issue: "Connection timeout"
**Solution:** Kiểm tra network, firewall, và Game Server có đang chạy không

### Issue: "Username not found"
**Solution:** Kiểm tra username có tồn tại trong database không

## Next Steps

1. **Logging**: Thêm detailed logging cho debugging
2. **Monitoring**: Setup monitoring cho transaction success rate
3. **Rate Limiting**: Implement rate limiting để prevent abuse
4. **Transaction History**: Lưu lại lịch sử transaction
5. **Webhook Integration**: Tích hợp với payment gateway webhooks
6. **Notification**: Gửi email/SMS notification khi charge thành công
7. **Admin Panel**: Tạo admin panel để xem transaction history

## References

- API Documentation: [CHARGE_MONEY_API.md](CHARGE_MONEY_API.md)
- Vietnamese Guide: [CHARGE_MONEY_GUIDE_VI.md](CHARGE_MONEY_GUIDE_VI.md)
- HTTP Examples: [GunnyApi/charge-money.http](GunnyApi/charge-money.http)
- Related APIs: [API_DOCUMENTATION.md](API_DOCUMENTATION.md)

## Support

Nếu cần hỗ trợ:
1. Check documentation files
2. Review logs
3. Test với charge-money.http file
4. Contact development team

---
**Last Updated:** 2026-02-14
**Version:** 1.0.0
