# API Lịch Sử Thanh Toán (Payment History API)

Tài liệu này mô tả các API endpoint để lấy thông tin lịch sử giao dịch thanh toán của người dùng thông qua PayMongo.

## Mục Lục
- [Xác Thực (Authentication)](#xác-thực-authentication)
- [Endpoint 1: Lấy Danh Sách Lịch Sử Thanh Toán](#endpoint-1-lấy-danh-sách-lịch-sử-thanh-toán)
- [Endpoint 2: Lấy Chi Tiết Một Giao Dịch](#endpoint-2-lấy-chi-tiết-một-giao-dịch)
- [Endpoint 3: Hủy Giao Dịch Đang Chờ](#endpoint-3-hủy-giao-dịch-đang-chờ)
- [Cấu Trúc Dữ Liệu](#cấu-trúc-dữ-liệu)
- [Mã Code Mẫu](#mã-code-mẫu)
- [Xử Lý Lỗi](#xử-lý-lỗi)

---

## Xác Thực (Authentication)

Tất cả các API endpoint yêu cầu **JWT Bearer Token** trong header:

```http
Authorization: Bearer <your_jwt_token>
```

Token được lấy từ API đăng nhập (`POST /api/users/login`).

---

## Endpoint 1: Lấy Danh Sách Lịch Sử Thanh Toán

### **GET** `/api/payment/history`

Lấy danh sách tất cả các giao dịch thanh toán của người dùng hiện tại với hỗ trợ phân trang.

### Query Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `pageNumber` | integer | No | 1 | Số trang cần lấy (bắt đầu từ 1) |
| `pageSize` | integer | No | 10 | Số lượng giao dịch trên mỗi trang (tối đa 100) |

### Request Example

```http
GET /api/payment/history?pageNumber=1&pageSize=20 HTTP/1.1
Host: your-api-domain.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Response Success (200 OK)

```json
{
  "success": true,
  "message": "Lấy lịch sử thanh toán thành công",
  "data": [
    {
      "id": 1,
      "userId": 12345,
      "username": "player123",
      "checkoutSessionId": "cs_test_a1b2c3d4e5f6",
      "paymentIntentId": "pi_test_x1y2z3w4v5u6",
      "paymentId": "pay_test_k1l2m3n4o5p6",
      "checkoutUrl": "https://checkout.paymongo.com/cs_test_a1b2c3d4e5f6",
      "amount": 500.00,
      "amountInCentavos": 50000,
      "currency": "PHP",
      "tierId": 3,
      "moneyReward": 5000,
      "goldReward": 1000,
      "giftTokenReward": 50,
      "status": "paid",
      "paymentMethod": "gcash",
      "isRewardProcessed": true,
      "createdAt": "2026-01-25T10:30:00",
      "paidAt": "2026-01-25T10:35:22",
      "expiresAt": null,
      "isPaid": true,
      "isFailed": false,
      "isExpired": false
    },
    {
      "id": 2,
      "userId": 12345,
      "username": "player123",
      "checkoutSessionId": "cs_test_b2c3d4e5f6g7",
      "paymentIntentId": null,
      "paymentId": null,
      "checkoutUrl": "https://checkout.paymongo.com/cs_test_b2c3d4e5f6g7",
      "amount": 200.00,
      "amountInCentavos": 20000,
      "currency": "PHP",
      "tierId": 1,
      "moneyReward": 2000,
      "goldReward": 400,
      "giftTokenReward": 20,
      "status": "pending",
      "paymentMethod": null,
      "isRewardProcessed": false,
      "createdAt": "2026-01-25T14:20:00",
      "paidAt": null,
      "expiresAt": null,
      "isPaid": false,
      "isFailed": false,
      "isExpired": false
    }
  ],
  "totalCount": 15
}
```

### Response Fields

| Field | Type | Description |
|-------|------|-------------|
| `success` | boolean | Trạng thái thành công của request |
| `message` | string | Thông báo kết quả |
| `data` | array | Danh sách các giao dịch |
| `data[].id` | integer | ID của giao dịch trong database |
| `data[].userId` | integer | ID người dùng |
| `data[].username` | string | Tên tài khoản |
| `data[].checkoutSessionId` | string | ID checkout session từ PayMongo |
| `data[].paymentIntentId` | string | ID payment intent (null nếu chưa thanh toán) |
| `data[].paymentId` | string | ID payment (null nếu chưa thanh toán) |
| `data[].checkoutUrl` | string | URL trang thanh toán |
| `data[].amount` | decimal | Số tiền (PHP) |
| `data[].amountInCentavos` | integer | Số tiền tính bằng centavos |
| `data[].currency` | string | Đơn vị tiền tệ (mặc định: PHP) |
| `data[].tierId` | integer | ID gói nạp tiền |
| `data[].moneyReward` | integer | Số tiền game nhận được |
| `data[].goldReward` | integer | Số vàng nhận được |
| `data[].giftTokenReward` | integer | Số gift token nhận được |
| `data[].status` | string | Trạng thái: `pending`, `paid`, `failed`, `expired`, `cancelled` |
| `data[].paymentMethod` | string | Phương thức thanh toán (gcash, paymaya, grab_pay, card, v.v.) |
| `data[].isRewardProcessed` | boolean | Phần thưởng đã được xử lý chưa |
| `data[].createdAt` | datetime | Thời gian tạo giao dịch |
| `data[].paidAt` | datetime | Thời gian thanh toán thành công (null nếu chưa thanh toán) |
| `data[].expiresAt` | datetime | Thời gian hết hạn (null nếu không có) |
| `data[].isPaid` | boolean | Đã thanh toán thành công? |
| `data[].isFailed` | boolean | Thanh toán thất bại? |
| `data[].isExpired` | boolean | Đã hết hạn? |
| `totalCount` | integer | Tổng số giao dịch (dùng cho pagination) |

### Response Error (401 Unauthorized)

```json
{
  "success": false,
  "message": "Unauthorized - Token không hợp lệ hoặc đã hết hạn"
}
```

---

## Endpoint 2: Lấy Chi Tiết Một Giao Dịch

### **GET** `/api/payment/history/{id}`

Lấy thông tin chi tiết của một giao dịch cụ thể. Chỉ có thể xem giao dịch của chính mình.

### Path Parameters

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `id` | integer | Yes | ID của giao dịch cần lấy |

### Request Example

```http
GET /api/payment/history/1 HTTP/1.1
Host: your-api-domain.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Response Success (200 OK)

```json
{
  "success": true,
  "message": "Lấy chi tiết giao dịch thành công",
  "data": [
    {
      "id": 1,
      "userId": 12345,
      "username": "player123",
      "checkoutSessionId": "cs_test_a1b2c3d4e5f6",
      "paymentIntentId": "pi_test_x1y2z3w4v5u6",
      "paymentId": "pay_test_k1l2m3n4o5p6",
      "checkoutUrl": "https://checkout.paymongo.com/cs_test_a1b2c3d4e5f6",
      "amount": 500.00,
      "amountInCentavos": 50000,
      "currency": "PHP",
      "tierId": 3,
      "tierName": "Gói VIP",
      "tierDescription": "Nạp 500 PHP nhận 5000 Money + 1000 Gold + 50 Gift Token",
      "moneyReward": 5000,
      "goldReward": 1000,
      "giftTokenReward": 50,
      "status": "paid",
      "paymentMethod": "gcash",
      "referenceNumber": "REF123456789",
      "eventType": "checkout_session.payment.paid",
      "eventId": "evt_test_12345",
      "failureCode": null,
      "failureMessage": null,
      "metadata": "{\"ip\":\"192.168.1.1\",\"user_agent\":\"Mozilla/5.0...\"}",
      "isRewardProcessed": true,
      "rewardProcessedAt": "2026-01-25T10:35:25",
      "rewardErrorMessage": null,
      "createdAt": "2026-01-25T10:30:00",
      "updatedAt": "2026-01-25T10:35:25",
      "paidAt": "2026-01-25T10:35:22",
      "expiresAt": null,
      "isPaid": true,
      "isFailed": false,
      "isExpired": false
    }
  ],
  "totalCount": 1
}
```

### Response Error (404 Not Found)

```json
{
  "success": false,
  "message": "Không tìm thấy giao dịch"
}
```

### Response Error (403 Forbidden)

```json
{
  "success": false,
  "message": "Bạn không có quyền xem giao dịch này"
}
```

---

## Endpoint 3: Hủy Giao Dịch Đang Chờ

### **POST** `/api/payment/expire-checkout-session`

Hủy (expire) một checkout session đang ở trạng thái pending. Sử dụng khi người dùng muốn hủy giao dịch mà họ đã tạo nhưng chưa thanh toán.

**Lưu ý**: Chỉ có thể hủy được các giao dịch ở trạng thái `pending`. Các giao dịch đã `paid`, `failed`, hoặc `expired` không thể hủy.

### Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `checkoutSessionId` | string | Yes | ID của checkout session cần hủy (lấy từ response khi tạo giao dịch) |

### Request Example

```http
POST /api/payment/expire-checkout-session HTTP/1.1
Host: your-api-domain.com
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "checkoutSessionId": "cs_test_a1b2c3d4e5f6"
}
```

### Response Success (200 OK)

```json
{
  "success": true,
  "message": "Hủy phiên thanh toán thành công",
  "data": {
    "checkoutSessionId": "cs_test_a1b2c3d4e5f6",
    "status": "expired",
    "expiredAt": "2026-01-25T15:30:45"
  }
}
```

### Response Error (400 Bad Request) - Invalid Session ID

```json
{
  "success": false,
  "message": "ID phiên thanh toán không hợp lệ"
}
```

### Response Error (400 Bad Request) - Already Expired or Paid

```json
{
  "success": false,
  "message": "Phiên thanh toán đã bị hủy hoặc đã thanh toán",
  "currentStatus": "paid"
}
```

### Response Error (404 Not Found)

```json
{
  "success": false,
  "message": "Không tìm thấy phiên thanh toán"
}
```

### Response Error (403 Forbidden)

```json
{
  "success": false,
  "message": "Bạn không có quyền hủy giao dịch này"
}
```

### Use Cases

**Khi nào nên sử dụng endpoint này:**

1. **User thay đổi ý định**: Người dùng tạo giao dịch nhưng không muốn thanh toán nữa
2. **Tạo giao dịch mới**: User muốn tạo giao dịch khác với số tiền khác
3. **Quản lý UI**: Hiển thị nút "Hủy giao dịch" cho các giao dịch pending trong lịch sử
4. **Cleanup**: Tự động hủy các giao dịch pending cũ để giữ dữ liệu sạch

**Lợi ích:**

- Giúp người dùng quản lý các giao dịch của mình tốt hơn
- Tránh nhầm lẫn với các link thanh toán cũ
- Cập nhật trạng thái trong database đồng bộ với PayMongo
- Giảm số lượng giao dịch pending không sử dụng

---

## Cấu Trúc Dữ Liệu

### Trạng Thái Giao Dịch (Status)

| Status | Description |
|--------|-------------|
| `pending` | Giao dịch đang chờ thanh toán |
| `paid` | Thanh toán thành công, phần thưởng đã được cộng |
| `failed` | Thanh toán thất bại |
| `expired` | Link thanh toán đã hết hạn |
| `cancelled` | Giao dịch đã bị hủy |

### Phương Thức Thanh Toán (Payment Method)

Các phương thức thanh toán phổ biến từ PayMongo:

- `gcash` - GCash e-wallet
- `paymaya` - PayMaya e-wallet
- `grab_pay` - GrabPay e-wallet
- `card` - Credit/Debit card
- `billease` - Billease installment
- `dob` - Dragon Pay Online Banking
- `dob_ubp` - UnionBank Online Banking

---

## Mã Code Mẫu

### JavaScript / TypeScript (Fetch API)

```javascript
// Lấy danh sách lịch sử thanh toán
async function getPaymentHistory(pageNumber = 1, pageSize = 10) {
  const token = localStorage.getItem('authToken');
  
  try {
    const response = await fetch(
      `https://your-api-domain.com/api/payment/history?pageNumber=${pageNumber}&pageSize=${pageSize}`,
      {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      }
    );

    const result = await response.json();
    
    if (result.success) {
      console.log('Tổng số giao dịch:', result.totalCount);
      console.log('Danh sách giao dịch:', result.data);
      
      // Lọc các giao dịch đã thanh toán
      const paidTransactions = result.data.filter(t => t.isPaid);
      console.log('Số giao dịch đã thanh toán:', paidTransactions.length);
      
      // Tính tổng số tiền đã nạp
      const totalAmount = paidTransactions.reduce((sum, t) => sum + t.amount, 0);
      console.log('Tổng tiền đã nạp:', totalAmount, 'PHP');
      
      return result.data;
    } else {
      console.error('Lỗi:', result.message);
      return null;
    }
  } catch (error) {
    console.error('Network error:', error);
    return null;
  }
}

// Lấy chi tiết một giao dịch
async function getPaymentDetail(transactionId) {
  const token = localStorage.getItem('authToken');
  
  try {
    const response = await fetch(
      `https://your-api-domain.com/api/payment/history/${transactionId}`,
      {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        }
      }
    );

    const result = await response.json();
    
    if (result.success && result.data.length > 0) {
      const transaction = result.data[0];
      console.log('Chi tiết giao dịch:', transaction);
      
      // Hiển thị thông tin
      console.log(`Mã giao dịch: #${transaction.id}`);
      console.log(`Trạng thái: ${transaction.status}`);
      console.log(`Số tiền: ${transaction.amount} PHP`);
      console.log(`Phần thưởng: ${transaction.moneyReward} Money + ${transaction.goldReward} Gold`);
      console.log(`Thời gian tạo: ${new Date(transaction.createdAt).toLocaleString()}`);
      
      if (transaction.isPaid) {
        console.log(`Đã thanh toán lúc: ${new Date(transaction.paidAt).toLocaleString()}`);
        console.log(`Phương thức: ${transaction.paymentMethod}`);
      }
      
      return transaction;
    } else {
      console.error('Lỗi:', result.message);
      return null;
    }
  } catch (error) {
    console.error('Network error:', error);
    return null;
  }
}

// Sử dụng
getPaymentHistory(1, 20).then(transactions => {
  if (transactions) {
    // Render UI
  }
});

getPaymentDetail(123).then(transaction => {
  if (transaction) {
    // Hiển thị chi tiết
  }
});

// Hủy giao dịch đang chờ
async function expireCheckoutSession(checkoutSessionId) {
  const token = localStorage.getItem('authToken');
  
  try {
    const response = await fetch(
      `https://your-api-domain.com/api/payment/expire-checkout-session`,
      {
        method: 'POST',
        headers: {
          'Authorization': `Bearer ${token}`,
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({
          checkoutSessionId: checkoutSessionId
        })
      }
    );

    const result = await response.json();
    
    if (result.success) {
      console.log('Hủy giao dịch thành công:', result.data);
      console.log(`Session ID: ${result.data.checkoutSessionId}`);
      console.log(`Trạng thái mới: ${result.data.status}`);
      console.log(`Thời gian hủy: ${new Date(result.data.expiredAt).toLocaleString()}`);
      
      return result.data;
    } else {
      console.error('Lỗi:', result.message);
      if (result.currentStatus) {
        console.log('Trạng thái hiện tại:', result.currentStatus);
      }
      return null;
    }
  } catch (error) {
    console.error('Network error:', error);
    return null;
  }
}

// Sử dụng
expireCheckoutSession('cs_test_a1b2c3d4e5f6').then(result => {
  if (result) {
    alert('Giao dịch đã được hủy thành công!');
    // Refresh danh sách lịch sử
    getPaymentHistory(1, 20);
  }
});
```

### React Hooks Example

```jsx
import { useState, useEffect } from 'react';

function PaymentHistoryComponent() {
  const [transactions, setTransactions] = useState([]);
  const [loading, setLoading] = useState(false);
  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 10;

  useEffect(() => {
    fetchPaymentHistory();
  }, [page]);

  const fetchPaymentHistory = async () => {
    setLoading(true);
    const token = localStorage.getItem('authToken');

    try {
      const response = await fetch(
        `https://your-api-domain.com/api/payment/history?pageNumber=${page}&pageSize=${pageSize}`,
        {
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          }
        }
      );

      const result = await response.json();
      
      if (result.success) {
        setTransactions(result.data);
        setTotalCount(result.totalCount);
      }
    } catch (error) {
      console.error('Error fetching payment history:', error);
    } finally {
      setLoading(false);
    }
  };

  const expireTransaction = async (checkoutSessionId) => {
    if (!window.confirm('Bạn có chắc muốn hủy giao dịch này?')) {
      return;
    }

    const token = localStorage.getItem('authToken');

    try {
      const response = await fetch(
        'https://your-api-domain.com/api/payment/expire-checkout-session',
        {
          method: 'POST',
          headers: {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
          },
          body: JSON.stringify({ checkoutSessionId })
        }
      );

      const result = await response.json();
      
      if (result.success) {
        alert('Hủy giao dịch thành công!');
        // Refresh danh sách
        fetchPaymentHistory();
      } else {
        alert(`Lỗi: ${result.message}`);
      }
    } catch (error) {
      console.error('Error expiring transaction:', error);
      alert('Có lỗi xảy ra khi hủy giao dịch');
    }
  };

  const totalPages = Math.ceil(totalCount / pageSize);

  return (
    <div>
      <h2>Lịch Sử Thanh Toán</h2>
      
      {loading ? (
        <p>Đang tải...</p>
      ) : (
        <>
          <table>
            <thead>
              <tr>
                <th>ID</th>
                <th>Số Tiền</th>
                <th>Trạng Thái</th>
                <th>Phương Thức</th>
                <th>Thời Gian</th>
                <th>Hành Động</th>
              </tr>
            </thead>
            <tbody>
              {transactions.map(tx => (
                <tr key={tx.id}>
                  <td>#{tx.id}</td>
                  <td>{tx.amount} PHP</td>
                  <td>
                    <span className={`status-${tx.status}`}>
                      {tx.status}
                    </span>
                  </td>
                  <td>{tx.paymentMethod || 'N/A'}</td>
                  <td>{new Date(tx.createdAt).toLocaleString()}</td>
                  <td>
                    {tx.status === 'pending' && (
                      <button 
                        onClick={() => expireTransaction(tx.checkoutSessionId)}
                        className="btn-cancel"
                      >
                        Hủy
                      </button>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>

          <div className="pagination">
            <button 
              disabled={page === 1} 
              onClick={() => setPage(page - 1)}
            >
              Trước
            </button>
            <span>Trang {page} / {totalPages}</span>
            <button 
              disabled={page === totalPages} 
              onClick={() => setPage(page + 1)}
            >
              Sau
            </button>
          </div>
        </>
      )}
    </div>
  );
}
```

### C# (.NET)

```csharp
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

public class PaymentHistoryService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl = "https://your-api-domain.com";
    private string _token;

    public PaymentHistoryService(string token)
    {
        _httpClient = new HttpClient();
        _token = token;
    }

    // Lấy danh sách lịch sử thanh toán
    public async Task<PaymentHistoryResponse> GetPaymentHistoryAsync(int pageNumber = 1, int pageSize = 10)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _token);

            var url = $"{_baseUrl}/api/payment/history?pageNumber={pageNumber}&pageSize={pageSize}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PaymentHistoryResponse>(json, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (result.Success)
                {
                    Console.WriteLine($"Tổng số giao dịch: {result.TotalCount}");
                    Console.WriteLine($"Số giao dịch trong trang: {result.Data.Count}");
                    
                    foreach (var transaction in result.Data)
                    {
                        Console.WriteLine($"ID: {transaction.Id}, Amount: {transaction.Amount} PHP, Status: {transaction.Status}");
                    }
                }
                
                return result;
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            return null;
        }
    }

    // Lấy chi tiết một giao dịch
    public async Task<PaymentTransaction> GetPaymentDetailAsync(int transactionId)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _token);

            var url = $"{_baseUrl}/api/payment/history/{transactionId}";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PaymentHistoryResponse>(json, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (result.Success && result.Data.Count > 0)
                {
                    var transaction = result.Data[0];
                    Console.WriteLine($"Chi tiết giao dịch #{transaction.Id}:");
                    Console.WriteLine($"  Số tiền: {transaction.Amount} PHP");
                    Console.WriteLine($"  Trạng thái: {transaction.Status}");
                    Console.WriteLine($"  Phần thưởng: {transaction.MoneyReward} Money + {transaction.GoldReward} Gold");
                    
                    return transaction;
                }
                
                return null;
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            return null;
        }
    }

    // Hủy giao dịch đang chờ
    public async Task<ExpireCheckoutSessionResponse> ExpireCheckoutSessionAsync(string checkoutSessionId)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _token);

            var url = $"{_baseUrl}/api/payment/expire-checkout-session";
            
            var requestBody = new 
            { 
                checkoutSessionId = checkoutSessionId 
            };
            
            var jsonContent = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<ExpireCheckoutSessionResponse>(json, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                if (result.Success)
                {
                    Console.WriteLine($"Hủy giao dịch thành công:");
                    Console.WriteLine($"  Session ID: {result.Data.CheckoutSessionId}");
                    Console.WriteLine($"  Trạng thái: {result.Data.Status}");
                    Console.WriteLine($"  Thời gian hủy: {result.Data.ExpiredAt}");
                }
                
                return result;
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error details: {errorContent}");
                return null;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
            return null;
        }
    }
}

// Models
public class PaymentHistoryResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public List<PaymentTransaction> Data { get; set; }
    public int TotalCount { get; set; }
}

public class PaymentTransaction
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
    public string CheckoutSessionId { get; set; }
    public string PaymentIntentId { get; set; }
    public string PaymentId { get; set; }
    public string CheckoutUrl { get; set; }
    public decimal Amount { get; set; }
    public int AmountInCentavos { get; set; }
    public string Currency { get; set; }
    public int TierId { get; set; }
    public int MoneyReward { get; set; }
    public int GoldReward { get; set; }
    public int GiftTokenReward { get; set; }
    public string Status { get; set; }
    public string PaymentMethod { get; set; }
    public bool IsRewardProcessed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsPaid { get; set; }
    public bool IsFailed { get; set; }
    public bool IsExpired { get; set; }
}

public class ExpireCheckoutSessionResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public ExpireCheckoutSessionData Data { get; set; }
}

public class ExpireCheckoutSessionData
{
    public string CheckoutSessionId { get; set; }
    public string Status { get; set; }
    public DateTime? ExpiredAt { get; set; }
}

// Sử dụng
class Program
{
    static async Task Main(string[] args)
    {
        var token = "your_jwt_token_here";
        var service = new PaymentHistoryService(token);

        // Lấy lịch sử
        var history = await service.GetPaymentHistoryAsync(1, 20);
        
        if (history != null && history.Success)
        {
            // Tính tổng tiền đã nạp
            decimal totalPaid = 0;
            foreach (var tx in history.Data)
            {
                if (tx.IsPaid)
                {
                    totalPaid += tx.Amount;
                }
            }
            Console.WriteLine($"Tổng tiền đã nạp: {totalPaid} PHP");
        }

        // Lấy chi tiết
        var detail = await service.GetPaymentDetailAsync(123);

        // Hủy giao dịch
        var expireResult = await service.ExpireCheckoutSessionAsync("cs_test_a1b2c3d4e5f6");
        if (expireResult != null && expireResult.Success)
        {
            Console.WriteLine("Đã hủy giao dịch thành công!");
        }
    }
}
```

        // Lấy lịch sử
        var history = await service.GetPaymentHistoryAsync(1, 20);
        
        if (history != null && history.Success)
        {
            // Tính tổng tiền đã nạp
            decimal totalPaid = 0;
            foreach (var tx in history.Data)
            {
                if (tx.IsPaid)
                {
                    totalPaid += tx.Amount;
                }
            }
            Console.WriteLine($"Tổng tiền đã nạp: {totalPaid} PHP");
        }

        // Lấy chi tiết
        var detail = await service.GetPaymentDetailAsync(123);
    }
}
```

### Python (requests)

```python
import requests
from typing import Optional, List, Dict
from datetime import datetime

class PaymentHistoryClient:
    def __init__(self, base_url: str, token: str):
        self.base_url = base_url
        self.token = token
        self.headers = {
            'Authorization': f'Bearer {token}',
            'Content-Type': 'application/json'
        }
    
    def get_payment_history(self, page_number: int = 1, page_size: int = 10) -> Optional[Dict]:
        """Lấy danh sách lịch sử thanh toán"""
        try:
            url = f"{self.base_url}/api/payment/history"
            params = {
                'pageNumber': page_number,
                'pageSize': page_size
            }
            
            response = requests.get(url, headers=self.headers, params=params)
            response.raise_for_status()
            
            result = response.json()
            
            if result.get('success'):
                print(f"Tổng số giao dịch: {result.get('totalCount')}")
                print(f"Số giao dịch trong trang: {len(result.get('data', []))}")
                
                # Phân tích dữ liệu
                paid_transactions = [t for t in result['data'] if t.get('isPaid')]
                total_amount = sum(t.get('amount', 0) for t in paid_transactions)
                
                print(f"Số giao dịch đã thanh toán: {len(paid_transactions)}")
                print(f"Tổng tiền đã nạp: {total_amount} PHP")
                
                return result
            else:
                print(f"Error: {result.get('message')}")
                return None
                
        except requests.exceptions.RequestException as e:
            print(f"Request error: {e}")
            return None
    
    def get_payment_detail(self, transaction_id: int) -> Optional[Dict]:
        """Lấy chi tiết một giao dịch"""
        try:
            url = f"{self.base_url}/api/payment/history/{transaction_id}"
            
            response = requests.get(url, headers=self.headers)
            response.raise_for_status()
            
            result = response.json()
            
            if result.get('success') and result.get('data'):
                transaction = result['data'][0]
                
                print(f"Chi tiết giao dịch #{transaction['id']}:")
                print(f"  Số tiền: {transaction['amount']} PHP")
                print(f"  Trạng thái: {transaction['status']}")
                print(f"  Phần thưởng: {transaction['moneyReward']} Money + {transaction['goldReward']} Gold")
                
                if transaction.get('isPaid'):
                    paid_at = datetime.fromisoformat(transaction['paidAt'].replace('Z', '+00:00'))
                    print(f"  Đã thanh toán lúc: {paid_at.strftime('%Y-%m-%d %H:%M:%S')}")
                    print(f"  Phương thức: {transaction.get('paymentMethod', 'N/A')}")
                
                return transaction
            else:
                print(f"Error: {result.get('message')}")
                return None
                
        except requests.exceptions.RequestException as e:
            print(f"Request error: {e}")
            return None
    
    def expire_checkout_session(self, checkout_session_id: str) -> Optional[Dict]:
        """Hủy một checkout session đang chờ"""
        try:
            url = f"{self.base_url}/api/payment/expire-checkout-session"
            
            payload = {
                'checkoutSessionId': checkout_session_id
            }
            
            response = requests.post(url, headers=self.headers, json=payload)
            response.raise_for_status()
            
            result = response.json()
            
            if result.get('success'):
                data = result.get('data', {})
                
                print(f"Hủy giao dịch thành công:")
                print(f"  Session ID: {data.get('checkoutSessionId')}")
                print(f"  Trạng thái: {data.get('status')}")
                
                if data.get('expiredAt'):
                    expired_at = datetime.fromisoformat(data['expiredAt'].replace('Z', '+00:00'))
                    print(f"  Thời gian hủy: {expired_at.strftime('%Y-%m-%d %H:%M:%S')}")
                
                return data
            else:
                print(f"Error: {result.get('message')}")
                if result.get('currentStatus'):
                    print(f"Trạng thái hiện tại: {result.get('currentStatus')}")
                return None
                
        except requests.exceptions.RequestException as e:
            print(f"Request error: {e}")
            return None
    
    def print_transaction_summary(self, transactions: List[Dict]):
        """In tóm tắt danh sách giao dịch"""
        print("\n" + "="*80)
        print(f"{'ID':<8} {'Số Tiền':<12} {'Trạng Thái':<12} {'Phương Thức':<15} {'Thời Gian':<20}")
        print("="*80)
        
        for tx in transactions:
            tx_id = f"#{tx['id']}"
            amount = f"{tx['amount']} PHP"
            status = tx['status']
            method = tx.get('paymentMethod') or 'N/A'
            created = datetime.fromisoformat(tx['createdAt'].replace('Z', '+00:00'))
            created_str = created.strftime('%Y-%m-%d %H:%M')
            
            print(f"{tx_id:<8} {amount:<12} {status:<12} {method:<15} {created_str:<20}")
        
        print("="*80 + "\n")

# Sử dụng
if __name__ == "__main__":
    # Khởi tạo client
    client = PaymentHistoryClient(
        base_url="https://your-api-domain.com",
        token="your_jwt_token_here"
    )
    
    # Lấy lịch sử thanh toán (trang 1, 20 giao dịch)
    history = client.get_payment_history(page_number=1, page_size=20)
    
    if history and history.get('success'):
        # In danh sách
        client.print_transaction_summary(history['data'])
        
        # Lấy chi tiết giao dịch đầu tiên
        if history['data']:
            first_tx_id = history['data'][0]['id']
            detail = client.get_payment_detail(first_tx_id)
            
            # Nếu có giao dịch pending, thử hủy
            pending_transactions = [tx for tx in history['data'] if tx['status'] == 'pending']
            if pending_transactions:
                first_pending = pending_transactions[0]
                print(f"\nTìm thấy giao dịch pending: {first_pending['checkoutSessionId']}")
                
                # Hủy giao dịch
                expire_result = client.expire_checkout_session(first_pending['checkoutSessionId'])
                if expire_result:
                    print("✓ Đã hủy giao dịch thành công!")
```

### PHP

```php
<?php

class PaymentHistoryClient {
    private $baseUrl;
    private $token;
    
    public function __construct($baseUrl, $token) {
        $this->baseUrl = $baseUrl;
        $this->token = $token;
    }
    
    // Lấy danh sách lịch sử thanh toán
    public function getPaymentHistory($pageNumber = 1, $pageSize = 10) {
        $url = $this->baseUrl . "/api/payment/history?pageNumber={$pageNumber}&pageSize={$pageSize}";
        
        $ch = curl_init();
        curl_setopt($ch, CURLOPT_URL, $url);
        curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
        curl_setopt($ch, CURLOPT_HTTPHEADER, [
            'Authorization: Bearer ' . $this->token,
            'Content-Type: application/json'
        ]);
        
        $response = curl_exec($ch);
        $httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
        curl_close($ch);
        
        if ($httpCode == 200) {
            $result = json_decode($response, true);
            
            if ($result['success']) {
                echo "Tổng số giao dịch: " . $result['totalCount'] . "\n";
                
                // Tính tổng tiền đã nạp
                $totalPaid = 0;
                foreach ($result['data'] as $tx) {
                    if ($tx['isPaid']) {
                        $totalPaid += $tx['amount'];
                    }
                }
                echo "Tổng tiền đã nạp: {$totalPaid} PHP\n";
                
                return $result['data'];
            }
        }
        
        return null;
    }
    
    // Lấy chi tiết một giao dịch
    public function getPaymentDetail($transactionId) {
        $url = $this->baseUrl . "/api/payment/history/{$transactionId}";
        
        $ch = curl_init();
        curl_setopt($ch, CURLOPT_URL, $url);
        curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
        curl_setopt($ch, CURLOPT_HTTPHEADER, [
            'Authorization: Bearer ' . $this->token,
            'Content-Type: application/json'
        ]);
        
        $response = curl_exec($ch);
        $httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
        curl_close($ch);
        
        if ($httpCode == 200) {
            $result = json_decode($response, true);
            
            if ($result['success'] && count($result['data']) > 0) {
                $tx = $result['data'][0];
                
                echo "Chi tiết giao dịch #{$tx['id']}:\n";
                echo "  Số tiền: {$tx['amount']} PHP\n";
                echo "  Trạng thái: {$tx['status']}\n";
                echo "  Phần thưởng: {$tx['moneyReward']} Money + {$tx['goldReward']} Gold\n";
                
                return $tx;
            }
        }
        
        return null;
    }
    
    // Hủy giao dịch đang chờ
    public function expireCheckoutSession($checkoutSessionId) {
        $url = $this->baseUrl . "/api/payment/expire-checkout-session";
        
        $payload = json_encode([
            'checkoutSessionId' => $checkoutSessionId
        ]);
        
        $ch = curl_init();
        curl_setopt($ch, CURLOPT_URL, $url);
        curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
        curl_setopt($ch, CURLOPT_POST, true);
        curl_setopt($ch, CURLOPT_POSTFIELDS, $payload);
        curl_setopt($ch, CURLOPT_HTTPHEADER, [
            'Authorization: Bearer ' . $this->token,
            'Content-Type: application/json'
        ]);
        
        $response = curl_exec($ch);
        $httpCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
        curl_close($ch);
        
        if ($httpCode == 200) {
            $result = json_decode($response, true);
            
            if ($result['success']) {
                $data = $result['data'];
                
                echo "Hủy giao dịch thành công:\n";
                echo "  Session ID: {$data['checkoutSessionId']}\n";
                echo "  Trạng thái: {$data['status']}\n";
                echo "  Thời gian hủy: {$data['expiredAt']}\n";
                
                return $data;
            } else {
                echo "Lỗi: {$result['message']}\n";
            }
        } else {
            echo "HTTP Error: {$httpCode}\n";
        }
        
        return null;
    }
}

// Sử dụng
$client = new PaymentHistoryClient(
    'https://your-api-domain.com',
    'your_jwt_token_here'
);

// Lấy lịch sử
$transactions = $client->getPaymentHistory(1, 20);

if ($transactions) {
    foreach ($transactions as $tx) {
        echo "ID: {$tx['id']}, Amount: {$tx['amount']} PHP, Status: {$tx['status']}\n";
    }
    
    // Hủy giao dịch pending (nếu có)
    foreach ($transactions as $tx) {
        if ($tx['status'] == 'pending') {
            echo "\nTìm thấy giao dịch pending, đang hủy...\n";
            $expireResult = $client->expireCheckoutSession($tx['checkoutSessionId']);
            if ($expireResult) {
                echo "✓ Đã hủy giao dịch thành công!\n";
            }
            break; // Chỉ hủy 1 giao dịch đầu tiên
        }
    }
}

// Lấy chi tiết
$detail = $client->getPaymentDetail(123);
?>
```

---

## Xử Lý Lỗi

### Các Mã Lỗi HTTP

| Status Code | Description | Xử lý |
|-------------|-------------|-------|
| **200** | Success | Dữ liệu trả về trong response body |
| **400** | Bad Request | Kiểm tra lại tham số pageNumber, pageSize |
| **401** | Unauthorized | Token không hợp lệ hoặc hết hạn, yêu cầu đăng nhập lại |
| **403** | Forbidden | Không có quyền truy cập giao dịch (chỉ xem được giao dịch của mình) |
| **404** | Not Found | Không tìm thấy giao dịch với ID đã cho |
| **500** | Internal Server Error | Lỗi server, thử lại sau hoặc liên hệ support |

### Xử Lý Token Hết Hạn

```javascript
async function fetchWithTokenRefresh(url, options) {
  let response = await fetch(url, options);
  
  // Nếu token hết hạn
  if (response.status === 401) {
    // Thử refresh token hoặc yêu cầu đăng nhập lại
    const newToken = await refreshAuthToken(); // Implement hàm này
    
    if (newToken) {
      // Thử lại request với token mới
      options.headers['Authorization'] = `Bearer ${newToken}`;
      response = await fetch(url, options);
    } else {
      // Chuyển về trang đăng nhập
      window.location.href = '/login';
      return null;
    }
  }
  
  return response;
}
```

### Best Practices

1. **Cache dữ liệu**: Lưu cache danh sách giao dịch để giảm số lần gọi API
2. **Pagination**: Không lấy quá nhiều giao dịch một lúc, sử dụng pagination hợp lý
3. **Error handling**: Luôn xử lý các trường hợp lỗi (network error, timeout, unauthorized)
4. **Loading state**: Hiển thị loading indicator khi đang fetch data
5. **Retry logic**: Implement retry với exponential backoff cho network errors
6. **Security**: Không lưu token trong localStorage nếu có thể, ưu tiên httpOnly cookies

---

## Tính Năng Nâng Cao

### Lọc và Tìm Kiếm

Mặc dù API hiện tại chưa hỗ trợ filter, bạn có thể filter ở client side:

```javascript
// Lọc theo trạng thái
const paidTransactions = transactions.filter(tx => tx.status === 'paid');
const pendingTransactions = transactions.filter(tx => tx.status === 'pending');

// Lọc theo phương thức thanh toán
const gcashTransactions = transactions.filter(tx => tx.paymentMethod === 'gcash');

// Lọc theo khoảng thời gian
const today = new Date();
const startOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);

const thisMonthTransactions = transactions.filter(tx => {
  const txDate = new Date(tx.createdAt);
  return txDate >= startOfMonth;
});

// Tính tổng tiền theo tháng
const monthlyTotal = thisMonthTransactions
  .filter(tx => tx.isPaid)
  .reduce((sum, tx) => sum + tx.amount, 0);
```

### Xuất Báo Cáo (Export)

```javascript
function exportToCSV(transactions) {
  const headers = ['ID', 'Ngày', 'Số Tiền', 'Trạng Thái', 'Phương Thức', 'Phần Thưởng'];
  const rows = transactions.map(tx => [
    tx.id,
    new Date(tx.createdAt).toLocaleString(),
    tx.amount,
    tx.status,
    tx.paymentMethod || 'N/A',
    `${tx.moneyReward} Money + ${tx.goldReward} Gold`
  ]);
  
  const csv = [
    headers.join(','),
    ...rows.map(row => row.join(','))
  ].join('\n');
  
  // Download file
  const blob = new Blob([csv], { type: 'text/csv' });
  const url = window.URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `payment_history_${Date.now()}.csv`;
  a.click();
}
```

---

## Liên Hệ & Hỗ Trợ

Nếu gặp vấn đề khi tích hợp API, vui lòng:

1. Kiểm tra kỹ thông báo lỗi trong response
2. Đảm bảo token đang valid và chưa hết hạn
3. Kiểm tra network connectivity
4. Xem log ở console/network tab trong DevTools

**Email hỗ trợ**: support@your-domain.com  
**API Documentation**: https://your-api-domain.com/docs

---

## Changelog

- **v1.0.0** (2026-01-25): Release đầu tiên
  - GET /api/payment/history - Lấy danh sách với pagination
  - GET /api/payment/history/{id} - Lấy chi tiết giao dịch
