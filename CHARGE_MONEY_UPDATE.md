# Charge Money API - Update Log

## Cập Nhật: Thêm Parameters `nickname` và `site`

### Vấn Đề
API charge money ban đầu chỉ gửi parameter `content` đến game server, thiếu 2 parameters quan trọng:
- `nickname`: UserID của người dùng
- `site`: Tên site (để support multi-site với ChargeKey riêng)

### Giải Pháp
Đã cập nhật API để gửi đầy đủ 3 parameters theo đúng yêu cầu của game server.

---

## Chi Tiết Các Thay Đổi

### 1. Model Updates

#### File: `Models/ChargeMoneyRequest.cs`
Thêm 2 properties mới:
```csharp
public int UserID { get; set; }        // ID của user (required)
public string? Site { get; set; }      // Site name (optional)
```

**ChargeMoneyRequest hoàn chỉnh:**
```csharp
public class ChargeMoneyRequest
{
    public string ChargeID { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public int Money { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal NeedMoney { get; set; }
    public int UserID { get; set; }      // ✅ NEW
    public string? Site { get; set; }    // ✅ NEW
}
```

### 2. Controller Updates

#### File: `Controllers/UsersController.cs`

**Validation mới cho UserID:**
```csharp
if (request.UserID <= 0)
{
    return BadRequest(new ChargeMoneyResponse
    {
        Success = false,
        Message = _localization.GetString("Server.UserIDRequired")
    });
}
```

**Cập nhật URL generation:**
```csharp
// Cũ
string chargeUrl = $"{_gameSettings.BaseRequestUrl}ChargeMoney.aspx?content={encodedContent}";

// Mới
string site = string.IsNullOrEmpty(request.Site) ? "" : request.Site.ToLower();
string chargeUrl = $"{_gameSettings.BaseRequestUrl}ChargeMoney.aspx?content={encodedContent}&site={HttpUtility.UrlEncode(site)}&nickname={request.UserID}";
```

### 3. Localization Updates

#### File: `Infrastructure/Services/LocalizationService.cs`

**Thêm message key mới:**

Vietnamese:
```csharp
["Server.UserIDRequired"] = "UserID không được để trống",
```

English:
```csharp
["Server.UserIDRequired"] = "UserID is required",
```

### 4. Documentation Updates

#### Updated Files:
- ✅ `CHARGE_MONEY_API.md`
- ✅ `CHARGE_MONEY_GUIDE_VI.md`
- ✅ `charge-money.http`

**Request Body Example (Updated):**
```json
{
  "chargeID": "TXN20260214001",
  "username": "player123",
  "money": 10000,
  "type": "paymongo",
  "needMoney": 299.00,
  "userID": 123,         // ✅ NEW: Required
  "site": "default"      // ✅ NEW: Optional
}
```

---

## API Request Flow

### Request URL Structure
```
http://127.0.0.1/request/ChargeMoney.aspx?content={encoded}&site={site}&nickname={userID}
```

### Parameters Breakdown

| Parameter | Type | Required | Source | Description |
|-----------|------|----------|--------|-------------|
| `content` | string | Yes | Encoded string | chargeID\|username\|money\|type\|needMoney\|hash |
| `site` | string | No | Request body | Site identifier (empty string nếu null) |
| `nickname` | int | Yes | Request body (UserID) | User ID trong hệ thống |

### Game Server Processing

Game server nhận và xử lý như sau:

```csharp
protected void Page_Load(object sender, EventArgs e)
{
    int result = 1;
    
    // Lấy 3 parameters
    string content = HttpUtility.UrlDecode(Request["content"]);
    string site = (Request["site"] == null) ? "" : HttpUtility.UrlDecode(Request["site"]).ToLower();
    int userID = Convert.ToInt32(HttpUtility.UrlDecode(Request["nickname"]));
    
    // Giải mã content với site-specific ChargeKey
    string[] data = UnEncryptCharge(content, ref result, site);
    
    // Xử lý nạp tiền với userID...
    
    Response.Write(result);
}
```

### Multi-Site Support

Game server có thể sử dụng ChargeKey khác nhau cho từng site:

**web.config:**
```xml
<appSettings>
  <!-- Default ChargeKey -->
  <add key="ChargeKey" value="default-key-here" />
  
  <!-- Site-specific ChargeKeys -->
  <add key="ChargeKey_vip" value="vip-site-key-here" />
  <add key="ChargeKey_special" value="special-site-key-here" />
</appSettings>
```

**UnEncryptCharge logic:**
```csharp
string chargeKey = string.Empty;

// Ưu tiên site-specific key
if (!string.IsNullOrEmpty(site))
{
    chargeKey = ConfigurationManager.AppSettings[$"ChargeKey_{site}"];
}

// Fallback về default key
if (string.IsNullOrEmpty(chargeKey))
{
    chargeKey = ConfigurationManager.AppSettings["ChargeKey"];
}
```

---

## Testing Examples

### Basic Request
```bash
POST /api/users/charge-money
Authorization: Bearer {token}
Content-Type: application/json

{
  "chargeID": "TXN20260214001",
  "username": "player123",
  "money": 10000,
  "type": "paymongo",
  "needMoney": 299.00,
  "userID": 123,
  "site": "default"
}
```

### Multi-Site Request (VIP)
```bash
POST /api/users/charge-money
Authorization: Bearer {token}
Content-Type: application/json

{
  "chargeID": "VIP_TXN_001",
  "username": "vipplayer",
  "money": 50000,
  "type": "bank",
  "needMoney": 1499.00,
  "userID": 456,
  "site": "vip"
}
```

### Without Site (Empty String)
```bash
POST /api/users/charge-money
Authorization: Bearer {token}
Content-Type: application/json

{
  "chargeID": "TXN20260214002",
  "username": "player456",
  "money": 15000,
  "type": "gcash",
  "needMoney": 449.00,
  "userID": 789
  // site không cần gửi, sẽ default là ""
}
```

---

## Validation Changes

### Old Validation
✅ ChargeID không được empty  
✅ Username không được empty  
✅ Money phải > 0  

### New Validation
✅ ChargeID không được empty  
✅ Username không được empty  
✅ Money phải > 0  
✅ **UserID phải > 0** ← NEW  

---

## Error Codes

Không có thay đổi về error codes. Vẫn sử dụng:

| Code | Description |
|------|-------------|
| 0 | Success |
| 2 | Username empty |
| 3 | Invalid amount (money <= 0) |
| 5 | Invalid IP |
| 6 | ChargeKey not configured |
| 7 | Hash verification failed |
| 8 | Invalid content format |

---

## Migration Guide

### Nếu Bạn Đang Sử Dụng API Cũ

**Code cũ:**
```javascript
const response = await fetch('/api/users/charge-money', {
  method: 'POST',
  headers: { 
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    chargeID: 'TXN123',
    username: 'player',
    money: 10000,
    type: 'paymongo',
    needMoney: 299.00
  })
});
```

**Code mới (required):**
```javascript
const response = await fetch('/api/users/charge-money', {
  method: 'POST',
  headers: { 
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    chargeID: 'TXN123',
    username: 'player',
    money: 10000,
    type: 'paymongo',
    needMoney: 299.00,
    userID: 123,           // ✅ REQUIRED: Thêm dòng này
    site: 'default'        // ✅ OPTIONAL: Có thể bỏ qua
  })
});
```

---

## Benefits

### 1. ✅ Đúng Theo Thiết Kế Game Server
API giờ đây gửi đầy đủ parameters mà game server yêu cầu.

### 2. ✅ Support Multi-Site
Có thể sử dụng ChargeKey khác nhau cho từng site.

### 3. ✅ User Tracking
Game server nhận được UserID để tracking và logging tốt hơn.

### 4. ✅ Better Logging
Game server có thể log đầy đủ thông tin: chargeID, username, userID, site.

### 5. ✅ Backwards Compatible (Partial)
- `site` là optional, có thể bỏ qua (sẽ default là "")
- `userID` là required, client phải cập nhật

---

## Checklist

- [x] Thêm UserID và Site vào ChargeMoneyRequest model
- [x] Thêm validation cho UserID > 0
- [x] Cập nhật URL generation với 3 parameters
- [x] Thêm localization keys cho UserIDRequired
- [x] Cập nhật CHARGE_MONEY_API.md
- [x] Cập nhật CHARGE_MONEY_GUIDE_VI.md
- [x] Cập nhật charge-money.http examples
- [x] Test không có errors
- [x] Tạo documentation về changes

---

## Summary

API charge money giờ đây hoàn chỉnh và đúng theo thiết kế của game server:

**URL Format:**
```
ChargeMoney.aspx?content={encoded}&site={site}&nickname={userID}
```

**Required Changes for Clients:**
- Thêm `userID` (integer, required, must be > 0)
- Thêm `site` (string, optional)

**Testing:**
Sử dụng file `charge-money.http` để test các scenarios khác nhau.
