# Charge Money API - Security Update

## 🔒 Cập Nhật Bảo Mật Quan Trọng

### Vấn Đề Bảo Mật Đã Được Sửa

**Trước đây:** Client truyền `username` và `userID` trong request body
```json
{
  "username": "player123",  // ❌ Client có thể giả mạo
  "userID": 123,            // ❌ Client có thể giả mạo 
  "money": 10000,
  "type": "paymongo",
  "needMoney": 299.00
}
```

**Vấn đề:**
- Client có thể giả mạo username và userID của người khác
- Không xác thực được danh tính thực sự của user
- Có thể nạp tiền vào tài khoản bất kỳ bằng cách thay đổi userID
- Lỗ hổng bảo mật nghiêm trọng!

---

## ✅ Giải Pháp

**Hiện tại:** Lấy `username` và `userID` từ JWT token (UserContext)

```json
{
  "money": 10000,
  "type": "paymongo",
  "needMoney": 299.00
}
```

**Các thay đổi:**
1. ✅ Username và UserID được lấy từ JWT token
2. ✅ Server tự động xác định user từ token
3. ✅ Client không thể giả mạo identity
4. ✅ Đảm bảo user chỉ nạp tiền vào tài khoản của chính mình

---

## 📝 Chi Tiết Thay Đổi

### 1. Model Update

**ChargeMoneyRequest.cs**
```csharp
// ❌ REMOVED: Các fields không còn cần thiết
// public string Username { get; set; }
// public int UserID { get; set; }

// ✅ Chỉ giữ lại fields cần thiết từ client
public class ChargeMoneyRequest
{
    public int Money { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal NeedMoney { get; set; }
}
```

### 2. Service Update

**IChargeMoneyService.cs**
```csharp
// Username và UserID giờ là parameters từ controller
Task<ChargeMoneyResponse> ChargeMoneyAsync(
    string username,      // Từ UserContext
    int userID,           // Từ UserContext
    int money,
    string type,
    decimal needMoney
);
```

### 3. Controller Update

**UsersController.cs**
```csharp
[HttpPost("charge-money")]
public async Task<IActionResult> ChargeMoney([FromBody] ChargeMoneyRequest request)
{
    // ✅ Lấy username từ JWT token
    var username = GetUsernameFromToken();
    
    // ✅ Lấy userID từ JWT token
    var userId = _userContext.GetUserId();
    
    // Validate
    if (string.IsNullOrEmpty(username) || !userId.HasValue || userId.Value <= 0)
    {
        return BadRequest(...);
    }
    
    // Gọi service với username và userID từ token
    var result = await _chargeMoneyService.ChargeMoneyAsync(
        username,           // ✅ Từ token
        userId.Value,       // ✅ Từ token
        request.Money,
        request.Type,
        request.NeedMoney
    );
    
    return Ok(result);
}
```

---

## 🔐 Luồng Xác Thực

```
1. Client gửi request với JWT token
   POST /api/users/charge-money
   Authorization: Bearer eyJhbGc...
   Body: { "money": 10000, "type": "paymongo", "needMoney": 299.00 }

2. Middleware xác thực JWT token
   ✓ Token valid?
   ✓ Token chưa expire?
   ✓ Signature đúng?

3. Controller lấy thông tin từ token
   username = GetUsernameFromToken()    // "player123"
   userId = UserContext.GetUserId()     // 123

4. Service xử lý charge money
   chargeID = Guid.NewGuid()
   content = $"{chargeID}|{username}|{money}|{type}|{needMoney}|{hash}"
   
5. Gửi request đến game server
   ChargeMoney.aspx?content={content}&site=a&nickname={userId}

6. Game server xác thực và nạp tiền
   ✓ Hash verification
   ✓ Nạp tiền vào tài khoản có username và userId từ content
```

---

## 🎯 Request Format

### Trước (Không An Toàn)
```bash
POST /api/users/charge-money
Authorization: Bearer {token}
Content-Type: application/json

{
  "username": "player123",    # ❌ Client tự truyền
  "userID": 123,              # ❌ Client tự truyền
  "money": 10000,
  "type": "paymongo",
  "needMoney": 299.00
}
```

### Sau (An Toàn)
```bash
POST /api/users/charge-money
Authorization: Bearer {token}
Content-Type: application/json

{
  "money": 10000,
  "type": "paymongo",
  "needMoney": 299.00
}
# ✅ username và userID được lấy từ token
```

---

## 🛡️ Bảo Mật

### Các Layer Bảo Mật

1. **JWT Authentication**
   - Token phải valid
   - Token phải chưa expire
   - Signature phải đúng

2. **UserContext**
   - Username được extract từ token claims
   - UserID được extract từ token claims
   - Không thể giả mạo

3. **Hash Verification**
   - Game server verify hash: MD5(chargeID + username + money + type + needMoney + key)
   - Đảm bảo data integrity

4. **IP Whitelist (Game Server)**
   - Game server chỉ accept request từ API server IP
   - Prevent direct access

### Attack Prevention

| Attack Type | Prevention |
|-------------|------------|
| **Identity Spoofing** | Username/UserID từ JWT token, không từ client |
| **MITM** | HTTPS + JWT signature verification |
| **Replay Attack** | ChargeID unique (GUID), game server check duplicate |
| **Data Tampering** | Hash verification trên game server |
| **Direct Access** | IP whitelist trên game server |

---

## 📊 So Sánh

| Aspect | Trước | Sau |
|--------|-------|-----|
| **Username Source** | Client request body | JWT token |
| **UserID Source** | Client request body | JWT token |
| **Security** | ❌ Low - Client có thể giả mạo | ✅ High - Server xác thực |
| **Request Fields** | 5 fields | 3 fields |
| **Code Complexity** | Simple but unsafe | Slightly complex but secure |
| **Attack Surface** | Large | Small |

---

## 🚀 Migration Guide

### Nếu Bạn Đang Sử Dụng API Cũ

**Code cũ (unsafe):**
```javascript
const response = await fetch('/api/users/charge-money', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    username: 'player123',    // ❌ Xóa
    userID: 123,              // ❌ Xóa
    money: 10000,
    type: 'paymongo',
    needMoney: 299.00
  })
});
```

**Code mới (secure):**
```javascript
const response = await fetch('/api/users/charge-money', {
  method: 'POST',
  headers: {
    'Authorization': `Bearer ${token}`,  // ✅ Token bắt buộc
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    money: 10000,
    type: 'paymongo',
    needMoney: 299.00
  })
});
```

---

## ✅ Benefits

1. **Bảo Mật Cao**
   - Không thể giả mạo identity
   - Username và UserID được xác thực qua JWT

2. **Đơn Giản Hơn Cho Client**
   - Client chỉ cần gửi 3 fields thay vì 5
   - Không lo việc lấy username và userID

3. **Tuân Thủ Best Practices**
   - Identity từ authentication token
   - Principle of least privilege
   - Defense in depth

4. **Audit Trail**
   - Có thể trace được chính xác user nào thực hiện giao dịch
   - Username và UserID khớp với token

---

## 📝 Key Takeaways

- ✅ **NEVER** tin tưởng identity từ client request body
- ✅ **ALWAYS** lấy identity từ authenticated source (JWT token)
- ✅ **VALIDATE** token ở middleware layer
- ✅ **EXTRACT** user info từ token claims
- ✅ **USE** UserContext để access user info

---

## 🔍 Testing

### Test Security
```bash
# Test 1: Gửi request với valid token
curl -X POST "http://localhost:5000/api/users/charge-money" \
  -H "Authorization: Bearer VALID_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"money": 10000, "type": "test", "needMoney": 299.00}'
# Expected: Success với username/userID từ token

# Test 2: Gửi request không có token
curl -X POST "http://localhost:5000/api/users/charge-money" \
  -H "Content-Type: application/json" \
  -d '{"money": 10000, "type": "test", "needMoney": 299.00}'
# Expected: 401 Unauthorized

# Test 3: Gửi request với token expired
curl -X POST "http://localhost:5000/api/users/charge-money" \
  -H "Authorization: Bearer EXPIRED_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"money": 10000, "type": "test", "needMoney": 299.00}'
# Expected: 401 Unauthorized
```

---

## Summary

Cập nhật này khắc phục một lỗ hổng bảo mật nghiêm trọng bằng cách:
1. Xóa username và userID khỏi request body
2. Lấy identity từ JWT token thông qua UserContext
3. Đảm bảo user chỉ có thể nạp tiền vào tài khoản của chính mình

**🔒 Security First!**
