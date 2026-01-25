# Payment History System - PayMongo Integration

## 📋 Overview
Hệ thống lưu lịch sử giao dịch PayMongo với đầy đủ thông tin về checkout session, payment, và rewards.

---

## 🗄️ Database Setup

### 1. Tạo bảng Payment_History

Chạy script SQL sau để tạo bảng:

```sql
-- File: Database/Payment_History.sql
-- Chạy script này trên database của bạn
```

### 2. Cấu trúc bảng

| Column | Type | Description |
|--------|------|-------------|
| Id | INT | Primary key (auto increment) |
| UserId | INT | ID của user trong Mem_Account |
| Username | NVARCHAR(255) | Email/Username của user |
| Amount | DECIMAL(18, 2) | Số tiền (PHP) |
| AmountInCentavos | INT | Số tiền (centavos) |
| Currency | NVARCHAR(10) | Loại tiền tệ (default: PHP) |
| CheckoutSessionId | NVARCHAR(255) | cs_xxx từ PayMongo |
| PaymentIntentId | NVARCHAR(255) | pi_xxx từ PayMongo |
| PaymentId | NVARCHAR(255) | pay_xxx từ PayMongo |
| CheckoutUrl | NVARCHAR(1000) | URL thanh toán |
| TierId | INT | ID của payment tier |
| MoneyReward | INT | Xu nhận được |
| GoldReward | INT | Vàng nhận được |
| GiftTokenReward | INT | Lễ kim nhận được |
| Status | NVARCHAR(50) | pending, paid, failed, expired, cancelled |
| PaymentMethod | NVARCHAR(50) | gcash, card, qrph, etc. |
| CreatedAt | DATETIME | Thời gian tạo |
| PaidAt | DATETIME | Thời gian thanh toán |
| ExpiresAt | DATETIME | Thời gian hết hạn |

---

## 🔄 Luồng hoạt động

### 1. Tạo Checkout Session
```
User → POST /api/payment/create-checkout-session
↓
Server tạo session với PayMongo
↓
Lưu vào Payment_History với status = "pending"
↓
Trả về CheckoutUrl cho user
```

### 2. User thanh toán
```
User thanh toán qua PayMongo
↓
PayMongo gửi webhook → POST /api/payment/webhook
↓
Xử lý webhook (HandlePaymentPaid)
↓
Cộng tiền vào Mem_Account
↓
Cộng Gold/GiftToken vào Tank (nếu enabled)
↓
Update Payment_History: status = "paid", PaidAt = now
↓
Mark reward as processed
```

### 3. Xem lịch sử
```
User → GET /api/payment/history?pageNumber=1&pageSize=20
↓
Lấy danh sách từ Payment_History theo UserId
↓
Trả về list giao dịch với pagination
```

---

## 🔌 API Endpoints

### 1. Lấy lịch sử giao dịch

**Endpoint:** `GET /api/payment/history`

**Authentication:** ✅ Required (Bearer Token)

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| pageNumber | int | 1 | Số trang |
| pageSize | int | 20 | Số record mỗi trang (max: 100) |

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Lấy lịch sử giao dịch thành công",
  "data": [
    {
      "id": 1,
      "amount": 50.00,
      "currency": "PHP",
      "status": "paid",
      "paymentMethod": "gcash",
      "description": "Nạp tiền vào tài khoản",
      "productName": "Nạp tiền",
      "moneyReward": 500,
      "goldReward": 0,
      "giftTokenReward": 50,
      "checkoutSessionId": "cs_abc123",
      "paymentId": "pay_xyz789",
      "checkoutUrl": "https://pm.link/...",
      "createdAt": "2026-01-25T10:30:00",
      "paidAt": "2026-01-25T10:32:15",
      "expiresAt": "2026-01-25T11:30:00",
      "isExpired": false,
      "isPaid": true,
      "isFailed": false
    }
  ],
  "totalCount": 25
}
```

**Status Values:**
- `pending` - Đang chờ thanh toán
- `paid` - Đã thanh toán thành công
- `failed` - Thanh toán thất bại
- `expired` - Đã hết hạn
- `cancelled` - Đã hủy

---

### 2. Lấy chi tiết một giao dịch

**Endpoint:** `GET /api/payment/history/{id}`

**Authentication:** ✅ Required (Bearer Token)

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| id | int | ID của payment history |

**Success Response (200 OK):**
```json
{
  "success": true,
  "message": "Payment history retrieved successfully",
  "data": {
    "id": 1,
    "amount": 50.00,
    "currency": "PHP",
    "status": "paid",
    // ... các trường khác
  }
}
```

**Error Response (404 Not Found):**
```json
{
  "success": false,
  "message": "Không tìm thấy lịch sử giao dịch"
}
```

**Error Response (403 Forbidden):**
```json
{
  "message": "Forbidden"
}
```
> User chỉ có thể xem lịch sử giao dịch của chính mình

---

## 💡 Code Examples

### JavaScript/TypeScript

#### Lấy lịch sử giao dịch:
```javascript
const getPaymentHistory = async (pageNumber = 1, pageSize = 20) => {
  const token = localStorage.getItem('access_token');
  
  const response = await fetch(
    `https://your-domain.com/api/payment/history?pageNumber=${pageNumber}&pageSize=${pageSize}`,
    {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
      }
    }
  );
  
  const data = await response.json();
  return data;
};

// Usage
const history = await getPaymentHistory(1, 20);
console.log(`Total transactions: ${history.totalCount}`);
history.data.forEach(transaction => {
  console.log(`${transaction.createdAt}: ${transaction.amount} ${transaction.currency} - ${transaction.status}`);
});
```

#### Lấy chi tiết giao dịch:
```javascript
const getPaymentDetail = async (id) => {
  const token = localStorage.getItem('access_token');
  
  const response = await fetch(
    `https://your-domain.com/api/payment/history/${id}`,
    {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
      }
    }
  );
  
  return await response.json();
};
```

### C# (.NET)

```csharp
public async Task<PaymentHistoryResponse> GetPaymentHistoryAsync(int pageNumber = 1, int pageSize = 20)
{
    var request = new HttpRequestMessage(
        HttpMethod.Get,
        $"/api/payment/history?pageNumber={pageNumber}&pageSize={pageSize}"
    );
    
    var response = await _httpClient.SendAsync(request);
    var content = await response.Content.ReadAsStringAsync();
    
    return JsonSerializer.Deserialize<PaymentHistoryResponse>(content);
}
```

### Python

```python
import requests

def get_payment_history(token, page_number=1, page_size=20):
    url = f"https://your-domain.com/api/payment/history"
    headers = {
        "Authorization": f"Bearer {token}"
    }
    params = {
        "pageNumber": page_number,
        "pageSize": page_size
    }
    
    response = requests.get(url, headers=headers, params=params)
    return response.json()

# Usage
token = "your_jwt_token"
history = get_payment_history(token, 1, 20)
print(f"Total: {history['totalCount']}")
for transaction in history['data']:
    print(f"{transaction['createdAt']}: {transaction['amount']} {transaction['status']}")
```

---

## 🔍 Query Examples

### Lấy tất cả giao dịch đã thanh toán của một user:
```sql
SELECT * FROM Payment_History 
WHERE UserId = 123 AND Status = 'paid' 
ORDER BY PaidAt DESC;
```

### Lấy tổng số tiền user đã nạp:
```sql
SELECT 
    UserId,
    Username,
    COUNT(*) as TotalTransactions,
    SUM(Amount) as TotalAmount,
    SUM(MoneyReward) as TotalMoneyReceived
FROM Payment_History 
WHERE UserId = 123 AND Status = 'paid'
GROUP BY UserId, Username;
```

### Lấy các giao dịch pending sắp hết hạn:
```sql
SELECT * FROM Payment_History 
WHERE Status = 'pending' 
AND ExpiresAt < DATEADD(MINUTE, 30, GETDATE())
AND ExpiresAt > GETDATE()
ORDER BY ExpiresAt ASC;
```

### Thống kê giao dịch theo payment method:
```sql
SELECT 
    PaymentMethod,
    COUNT(*) as Count,
    SUM(Amount) as TotalAmount
FROM Payment_History 
WHERE Status = 'paid'
GROUP BY PaymentMethod
ORDER BY TotalAmount DESC;
```

---

## 📊 Monitoring & Debugging

### 1. Check pending payments:
```sql
SELECT * FROM Payment_History 
WHERE Status = 'pending' 
ORDER BY CreatedAt DESC;
```

### 2. Check failed payments:
```sql
SELECT * FROM Payment_History 
WHERE Status = 'failed' 
ORDER BY CreatedAt DESC;
```

### 3. Check unprocessed rewards:
```sql
SELECT * FROM Payment_History 
WHERE Status = 'paid' 
AND IsRewardProcessed = 0
ORDER BY PaidAt DESC;
```

### 4. Check payment history without matching user:
```sql
SELECT ph.* 
FROM Payment_History ph
LEFT JOIN Mem_Account ma ON ph.UserId = ma.UserID
WHERE ma.UserID IS NULL;
```

---

## ⚠️ Important Notes

1. **Transaction Tracking:**
   - Mỗi checkout session được lưu ngay khi tạo với status = "pending"
   - Khi webhook nhận được event "paid", status được update thành "paid"
   - Reward chỉ được cộng sau khi payment success

2. **Security:**
   - User chỉ có thể xem lịch sử giao dịch của chính mình
   - Tất cả endpoints đều require authentication
   - SQL injection protection được áp dụng

3. **Pagination:**
   - Default: 20 items per page
   - Maximum: 100 items per page
   - Sử dụng OFFSET/FETCH NEXT cho performance tốt

4. **Status Lifecycle:**
   ```
   pending → paid (success)
           → failed (payment failed)
           → expired (timeout)
           → cancelled (user cancelled)
   ```

5. **Reward Processing:**
   - Reward được cộng ngay sau khi payment success
   - Flag `IsRewardProcessed` để track việc cộng reward
   - Nếu có lỗi, `RewardErrorMessage` sẽ lưu thông tin lỗi

---

## 🐛 Troubleshooting

### Problem: Payment history không được tạo
**Solution:** 
- Check PaymentRepository đã được đăng ký trong Program.cs chưa
- Check connection string có đúng không
- Check user có quyền CREATE trên database không

### Problem: Webhook không update payment history
**Solution:**
- Check PayMongo webhook URL có đúng không
- Check server có nhận được webhook không (xem logs)
- Check CheckoutSessionId có match không

### Problem: Reward không được cộng
**Solution:**
- Check flag `IsRewardProcessed` và `RewardErrorMessage`
- Check UserId có tồn tại trong Mem_Account không
- Check stored procedure hoặc update query có lỗi không

---

## 📞 Support

Nếu có vấn đề, check logs tại:
- Application logs: `_logger.LogInformation/LogError`
- Database: Query Payment_History table
- PayMongo Dashboard: Check transaction status

---

**Last Updated:** January 25, 2026  
**Version:** 1.0.0
