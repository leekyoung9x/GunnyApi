# Charge Money API Documentation

## Overview
API này cho phép nạp tiền vào tài khoản game thông qua cơ chế tương tự như login-game API. Chuỗi được tạo ra có thể được giải mã bởi game server sử dụng hàm `UnEncryptCharge`.

## Endpoint
```
POST /api/users/charge-money
```

## Authentication
Required: Yes (Bearer Token)

## Request Body
```json
{
  "money": 10000,             // Số tiền nạp (phải > 0)
  "type": "string",           // Loại giao dịch
  "needMoney": 299.00         // Số tiền thực tế cần thanh toán
}
```

### Request Parameters

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `money` | integer | Yes | Số tiền nạp vào game (phải lớn hơn 0) |
| `type` | string | Yes | Loại giao dịch (vd: "paymongo", "direct", etc.) |
| `needMoney` | decimal | Yes | Số tiền thực tế cần thanh toán |

**Notes:** 
- `username` và `userID` được lấy từ JWT token (UserContext), không cần gửi từ client
- `chargeID` được tự động tạo bằng GUID (UUID)
- `site` được cấu hình trong `appsettings.json` (mặc định: "a")

## Response

### Success Response (200 OK)
```json
{
  "success": true,
  "message": "Nạp tiền thành công",
  "content": "a1b2c3d4e5f6...|user123|10000|paymongo|299|hash...",
  "requestUrl": "http://127.0.0.1/request/ChargeMoney.aspx?content=...&site=a&nickname=123"
}
```

**Note:** `chargeID` trong content là GUID tự động tạo (32 hex characters)

### Error Response (400 Bad Request)
```json
{
  "success": false,
  "message": "Mã giao dịch không được để trống"
}
```

### Error Response (500 Internal Server Error)
```json
{
  "success": false,
  "message": "Có lỗi xảy ra khi nạp tiền: [error details]"
}
```

## Content Format
API tạo ra chuỗi content với format sau để gửi đến game server:
```
chargeID|username|money|type|needMoney|hash
```

Trong đó:
- `hash = MD5(chargeID + username + money + type + needMoney + ChargeKey)`

## Example

### cURL Request
```bash
curl -X POST "http://localhost:5000/api/users/charge-money" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "money": 10000,
    "type": "paymongo",
    "needMoney": 299.00
  }'
```

### JavaScript/Fetch Request
```javascript
const response = await fetch('http://localhost:5000/api/users/charge-money', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    money: 10000,
    type: 'paymongo',
    needMoney: 299.00
  })
});

const data = await response.json();
console.log(data);
```

## Configuration

### appsettings.json
```json
{
  "GameSettings": {
    "BaseRequestUrl": "http://127.0.0.1/request/",
    "ChargeKey": "QY-16-WAN-0668-2555555-7ROAD-ditmemaydiloz-12345678@Abc789-gtk",
    "Site": "a"
  }
}
```

### Keys Used
- `ChargeKey`: Key dùng để tạo hash verification cho charge money
- `BaseRequestUrl`: Base URL của game server request endpoints
- `Site`: Tên site được gửi đến game server (mặc định: "a")

## Server-side Decryption (Game Server)

Game server nhận 3 parameters từ request:
- `content`: Chuỗi đã encode chứa thông tin giao dịch
- `site`: Tên site (optional, để support multi-site với ChargeKey riêng)
- `nickname`: UserID của người dùng

Hàm `UnEncryptCharge` trên game server sẽ giải mã content như sau:

```csharp
protected void Page_Load(object sender, EventArgs e)
{
    int result = 1;
    
    // Lấy parameters từ request
    string content = HttpUtility.UrlDecode(Request["content"]);
    string site = (Request["site"] == null) ? "" : HttpUtility.UrlDecode(Request["site"]).ToLower();
    int userID = Convert.ToInt32(HttpUtility.UrlDecode(Request["nickname"]));
    
    // Giải mã và xác thực
    string[] data = UnEncryptCharge(content, ref result, site);
    
    if (data.Length > 5 && result == 0)
    {
        string chargeID = data[0];
        string username = data[1];
        int money = int.Parse(data[2]);
        string type = data[3];
        decimal needMoney = decimal.Parse(data[4]);
        
        // Thực hiện nạp tiền...
    }
    
    Response.Write(result);
}

public string[] UnEncryptCharge(string content, ref int result, string site)
{
    // Lấy ChargeKey theo site (nếu có)
    string chargeKey = string.Empty;
    if (!string.IsNullOrEmpty(site))
    {
        chargeKey = ConfigurationManager.AppSettings[$"ChargeKey_{site}"];
    }
    if (string.IsNullOrEmpty(chargeKey))
    {
        chargeKey = ConfigurationManager.AppSettings["ChargeKey"];
    }
    
    // Split content by '|'
    string[] strArray = content.Split('|');
    
    // strArray[0] = chargeID
    // strArray[1] = username
    // strArray[2] = money
    // strArray[3] = type
    // strArray[4] = needMoney
    // strArray[5] = hash
    
    if (strArray.Length > 5)
    {
        // Verify hash
        string calculatedHash = MD5(strArray[0] + strArray[1] + strArray[2] + 
                                     strArray[3] + strArray[4] + chargeKey);
        
        if (calculatedHash.ToLower() == strArray[5].ToLower())
        {
            result = 0; // Success
            return strArray;
        }
        else
        {
            result = 7; // Hash verification failed
        }
    }
    else
    {
        result = 8; // Invalid content format
    }
    
    return new string[0];
}
```

## Error Codes

| Result Code | Description |
|-------------|-------------|
| 0 | Success - Nạp tiền thành công |
| 2 | Username empty - Username không được để trống |
| 3 | Invalid amount - Số tiền không hợp lệ |
| 5 | Invalid IP - IP không được phép |
| 6 | ChargeKey not configured - ChargeKey chưa được cấu hình |
| 7 | Hash verification failed - Xác thực hash thất bại |
| 8 | Invalid content format - Format content không hợp lệ |

## Localization Support

API hỗ trợ đa ngôn ngữ (vi/en) thông qua Accept-Language header:

### Vietnamese
```
Accept-Language: vi-VN
```

### English
```
Accept-Language: en-US
```

## Security Considerations

1. **Authentication Required**: Endpoint yêu cầu JWT token hợp lệ
2. **Hash Verification**: Sử dụng MD5 hash để verify tính toàn vẹn của data
3. **ChargeKey**: Nên được lưu trữ an toàn và không expose ra public
4. **Unique ChargeID**: Mỗi giao dịch phải có mã unique để tránh duplicate
5. **IP Whitelist**: Game server nên validate IP của request

## Notes

- API này tương tự như `login-game` API nhưng dùng cho mục đích nạp tiền
- Content được tạo ra có thể được sử dụng để gọi trực tiếp đến game server
- ChargeKey mặc định sử dụng LoginKey nếu chưa được cấu hình
- Hỗ trợ cả BaseRequestUrl để tối ưu việc cấu hình các endpoint
