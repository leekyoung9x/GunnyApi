# Hướng Dẫn Sử Dụng API Nạp Tiền

## Tổng Quan

API Charge Money cho phép bạn nạp tiền vào tài khoản game một cách an toàn thông qua cơ chế mã hóa tương tự như API đăng nhập game. 

## Cơ Chế Hoạt Động

### 1. Luồng Xử Lý
```
Client → API Server → Game Server
  ↓           ↓            ↓
Request → Tạo Content → Xác thực & Nạp tiền
```

### 2. Quy Trình Chi Tiết

1. **Client gửi request** với thông tin giao dịch
2. **API Server tạo chuỗi content** với format: `chargeID|username|money|type|needMoney|hash`
3. **API Server gửi request** đến Game Server
4. **Game Server xác thực hash** và thực hiện nạp tiền
5. **Trả về kết quả** cho client

## Cấu Hình

### 1. Cập Nhật appsettings.json

```json
{
  "GameSettings": {
    "BaseRequestUrl": "http://127.0.0.1/request/",
    "LoginUrl": "http://127.0.0.1/request/CreateLogin.aspx",
    "ChargeKey": "QY-16-WAN-0668-2555555-7ROAD-ditmemaydiloz-12345678@Abc789-gtk",
    "Site": "a"
  }
}
```

**Giải thích:**
- `BaseRequestUrl`: URL cơ sở cho các endpoint của game server
- `LoginUrl`: Được tối ưu để sử dụng BaseRequestUrl + endpoint
- `ChargeKey`: Key bí mật dùng để tạo hash xác thực
- `Site`: Tên site được gửi đến game server (mặc định: "a")

### 2. Đồng Bộ ChargeKey

ChargeKey trên API Server **PHẢI KHỚP** với ChargeKey trên Game Server. Nếu không khớp, xác thực sẽ thất bại.

**Trên Game Server (web.config):**
```xml
<appSettings>
  <add key="ChargeKey" value="QY-16-WAN-0668-2555555-7ROAD-ditmemaydiloz-12345678@Abc789-gtk" />
</appSettings>
```

## Sử Dụng API

### Endpoint
```
POST /api/users/charge-money
```

### Headers
```
Authorization: Bearer YOUR_JWT_TOKEN
Content-Type: application/json
Accept-Language: vi-VN (hoặc en-US)
```

### Request Body

```json
{
  "money": 10000,
  "type": "paymongo",
  "needMoney": 299.00
}
```

**Giải thích các trường:**

| Trường | Kiểu | Bắt buộc | Mô tả |
|--------|------|----------|-------|
| `money` | int | Có | Số tiền nạp vào game (xu/đồng game) |
| `type` | string | Có | Loại giao dịch: "paymongo", "direct", "bank", etc. |
| `needMoney` | decimal | Có | Số tiền thực tế khách hàng phải trả (PHP, VND, USD...) |

**Lưu ý:** 
- `username` và `userID` được lấy từ JWT token (không cần truyền)
- `chargeID` được tự động tạo bằng GUID
- `site` được cấu hình trong `appsettings.json` (mặc định: "a")

### Response Thành Công

```json
{
  "success": true,
  "message": "Nạp tiền thành công",
  "content": "a1b2c3d4e5f6789...|player123|10000|paymongo|299|hash...",
  "requestUrl": "http://127.0.0.1/request/ChargeMoney.aspx?content=...&site=a&nickname=123"
}
```

**Lưu ý:** `chargeID` trong content là GUID tự động tạo (32 hex characters)

### Response Lỗi

```json
{
  "success": false,
  "message": "Mã giao dịch không được để trống"
}
```

## Ví Dụ Tích Hợp

### 1. JavaScript/React

```javascript
async function chargeMoney(chargeData) {
  try {
    const token = localStorage.getItem('authToken');
    
    const response = await fetch('http://localhost:5000/api/users/charge-money', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json',
        'Accept-Language': 'vi-VN'
      },
      body: JSON.stringify({
        money: chargeData.money,
        type: 'paymongo',
        needMoney: chargeData.amount
      })
    });

    const result = await response.json();
    
    if (result.success) {
      console.log('Nạp tiền thành công!');
      // Cập nhật UI, hiển thị thông báo...
    } else {
      console.error('Lỗi:', result.message);
    }
  } catch (error) {
    console.error('Exception:', error);
  }
}

// Sử dụng
chargeMoney({
  money: 10000,
  amount: 299.00
});
```

### 2. C# Client

```csharp
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class ChargeMoneyRequest
{
    public int Money { get; set; }
    public string Type { get; set; }
    public decimal NeedMoney { get; set; }
}

public async Task<bool> ChargeMoneyAsync(int money, decimal needMoney)
{
    using var client = new HttpClient();
    
    var request = new ChargeMoneyRequest
    {
        Money = money,
        Type = "paymongo",
        NeedMoney = needMoney
    };
    
    var json = JsonSerializer.Serialize(request);
    var content = new StringContent(json, Encoding.UTF8, "application/json");
    
    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
    client.DefaultRequestHeaders.Add("Accept-Language", "vi-VN");
    
    var response = await client.PostAsync(
        "http://localhost:5000/api/users/charge-money", 
        content
    );
    
    var result = await response.Content.ReadAsStringAsync();
    // Xử lý result...
    
    return response.IsSuccessStatusCode;
}
```

### 3. Python

```python
import requests
import time

def charge_money(money, need_money, token):
    url = "http://localhost:5000/api/users/charge-money"
    
    headers = {
        "Authorization": f"Bearer {token}",
        "Content-Type": "application/json",
        "Accept-Language": "vi-VN"
    }
    
    data = {
        "money": money,
        "type": "paymongo",
        "needMoney": need_money
    }
    
    response = requests.post(url, json=data, headers=headers)
    
    if response.status_code == 200:
        result = response.json()
        if result['success']:
            print(f"Nạp tiền thành công: {result['message']}")
            return True
        else:
            print(f"Lỗi: {result['message']}")
            return False
    else:
        print(f"HTTP Error: {response.status_code}")
        return False

# Sử dụng
charge_money(10000, 299.00, "your_jwt_token_here")
```

## Tích Hợp Với Payment Gateway

### Ví Dụ: PayMongo Webhook Handler

```csharp
[HttpPost("paymongo-webhook")]
public async Task<IActionResult> HandlePayMongoWebhook([FromBody] WebhookPayload payload)
{
    try
    {
        // Xác thực webhook signature
        if (!ValidateWebhookSignature(payload))
        {
            return Unauthorized();
        }

        // Kiểm tra payment status
        if (payload.Data.Attributes.Status == "paid")
        {
            var paymentIntent = payload.Data.Attributes;
            
            // Tạo charge request
            var chargeRequest = new ChargeMoneyRequest
            {
                Money = CalculateGameMoney(paymentIntent.Amount),
                Type = "paymongo",
                NeedMoney = paymentIntent.Amount / 100m // Convert từ cents
            };

            // Gọi charge money API
            var result = await CallChargeMoneyAPI(chargeRequest);
            
            if (result.Success)
            {
                // Update payment history
                await UpdatePaymentHistory(chargeRequest, "success");
                
                // Gửi email thông báo
                await SendChargeSuccessEmail(chargeRequest.Username);
                
                return Ok();
            }
        }

        return BadRequest();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing PayMongo webhook");
        return StatusCode(500);
    }
}
```

## Mã Lỗi và Xử Lý

| Mã | Ý nghĩa | Xử lý |
|----|---------|-------|
| 0 | Thành công | Hiển thị thông báo thành công |
| 2 | Username trống | Kiểm tra lại username |
| 3 | Số tiền không hợp lệ | Money phải > 0 |
| 5 | IP không hợp lệ | Kiểm tra IP whitelist trên game server |
| 6 | ChargeKey chưa cấu hình | Cấu hình ChargeKey trong appsettings |
| 7 | Hash không khớp | Đồng bộ lại ChargeKey giữa API và Game Server |
| 8 | Format content lỗi | Kiểm tra lại format content |

## Bảo Mật

### 1. ChargeKey
- **KHÔNG BAO GIỜ** expose ChargeKey ra public
- Lưu trong appsettings.json hoặc environment variables
- Sử dụng secrets manager trong production

### 2. ChargeID
- Phải unique cho mỗi giao dịch
- Nên có timestamp hoặc random component
- Kiểm tra duplicate trước khi tạo request

### 3. IP Whitelist
Game server nên validate IP của request:

```csharp
public static bool ValidLoginIP(string ip)
{
    string allowedIPs = ConfigurationManager.AppSettings["ChargeIP"];
    return allowedIPs.Split('|').Contains(ip);
}
```

### 4. JWT Token
- API yêu cầu authentication
- Token phải valid và chưa expire
- Sử dụng HTTPS trong production

## Testing

### 1. Unit Test

```csharp
[Fact]
public async Task ChargeMoney_ValidRequest_ReturnsSuccess()
{
    // Arrange
    var request = new ChargeMoneyRequest
    {
        ChargeID = "TEST001",
        Username = "testuser",
        Money = 10000,
        Type = "test",
        NeedMoney = 299.00m
    };

    // Act
    var response = await _controller.ChargeMoney(request);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(response);
    var result = Assert.IsType<ChargeMoneyResponse>(okResult.Value);
    Assert.True(result.Success);
}
```

### 2. Manual Test với Postman

```
POST http://localhost:5000/api/users/charge-money
Authorization: Bearer eyJhbGc...
Content-Type: application/json

{
  "chargeID": "TEST20260214001",
  "username": "testplayer",
  "money": 10000,
  "type": "manual",
  "needMoney": 299.00
}
```

## Troubleshooting

### Lỗi: "Hash verification failed" (result = 7)

**Nguyên nhân:** ChargeKey không khớp giữa API Server và Game Server

**Giải pháp:**
1. Kiểm tra ChargeKey trong appsettings.json (API Server)
2. Kiểm tra ChargeKey trong web.config (Game Server)
3. Đảm bảo cả 2 key giống hệt nhau

### Lỗi: "Invalid IP" (result = 5)

**Nguyên nhân:** IP của API Server không nằm trong whitelist

**Giải pháp:**
1. Thêm IP của API Server vào ChargeIP trong web.config game server
2. Format: `<add key="ChargeIP" value="127.0.0.1|192.168.1.100" />`

### Lỗi: "Username is required"

**Nguyên nhân:** Username bị null hoặc empty

**Giải pháp:**
1. Kiểm tra request body có trường username
2. Đảm bảo username không empty

## FAQ

**Q: ChargeKey có thể khác LoginKey không?**
A: Có, nhưng nếu không cấu hình ChargeKey, hệ thống sẽ fallback sang dùng LoginKey.

**Q: ChargeID có format bắt buộc không?**
A: Không có format bắt buộc, nhưng nên dùng format dễ trace như `TXN{timestamp}` hoặc `CHG{paymentId}`.

**Q: Có thể nạp âm tiền không?**
A: Không, money phải > 0. API sẽ trả về lỗi nếu money <= 0.

**Q: Có giới hạn số lần gọi API không?**
A: Tùy vào cấu hình rate limiting. Mặc định không có giới hạn.

**Q: API có support retry không?**
A: Nên implement retry logic ở client side với exponential backoff.

## Liên Hệ & Hỗ Trợ

Nếu gặp vấn đề, vui lòng:
1. Kiểm tra logs trong appsettings.json (LogLevel)
2. Kiểm tra game server logs
3. Liên hệ team support với thông tin chi tiết về error
