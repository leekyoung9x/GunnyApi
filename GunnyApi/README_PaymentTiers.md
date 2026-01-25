# Payment Tiers API Documentation

## Overview
API endpoints để quản lý và lấy thông tin về các mốc nạp tiền (payment tiers) và phần thưởng tương ứng.

## Endpoints

### 1. Get All Payment Tiers
Lấy danh sách tất cả các mốc nạp tiền đang hoạt động.

**Endpoint:** `GET /api/payment/tiers`

**Authorization:** Not required (Anonymous)

**Response Example:**
```json
{
  "success": true,
  "message": "Payment tiers retrieved successfully",
  "currency": "PHP",
  "minimumAmount": 10,
  "maximumAmount": 50000,
  "data": [
    {
      "id": 1,
      "amount": 50,
      "gold": 500,
      "money": 5000,
      "giftToken": 0,
      "bonusPercent": 0,
      "displayName": "Gói Khởi Đầu",
      "description": "Gói nạp cơ bản cho người mới",
      "isActive": true,
      "sortOrder": 1
    },
    {
      "id": 2,
      "amount": 100,
      "gold": 1100,
      "money": 11000,
      "giftToken": 50,
      "bonusPercent": 10,
      "displayName": "Gói Tiết Kiệm",
      "description": "Nhận thêm 10% bonus và 50 Lễ Kim",
      "isActive": true,
      "sortOrder": 2
    }
  ]
}
```

### 2. Get Payment Tier by ID
Lấy thông tin chi tiết của một mốc nạp tiền cụ thể.

**Endpoint:** `GET /api/payment/tiers/{id}`

**Authorization:** Not required (Anonymous)

**Parameters:**
- `id` (path parameter): ID của payment tier

**Response Example:**
```json
{
  "success": true,
  "message": "Payment tier retrieved successfully",
  "data": {
    "id": 3,
    "amount": 200,
    "gold": 2400,
    "money": 24000,
    "giftToken": 150,
    "bonusPercent": 20,
    "displayName": "Gói Phổ Biến",
    "description": "Nhận thêm 20% bonus và 150 Lễ Kim",
    "isActive": true,
    "sortOrder": 3
  }
}
```

**Error Response (Not Found):**
```json
{
  "success": false,
  "message": "Payment tier not found"
}
```

## Configuration

### appsettings.json
Configuration cho payment tiers được lưu trong `appsettings.json`:

```json
{
  "PaymentTiersSettings": {
    "Enabled": true,
    "Currency": "PHP",
    "MinimumAmount": 10,
    "MaximumAmount": 50000,
    "Tiers": [
      {
        "Id": 1,
        "Amount": 50,
        "Gold": 500,
        "Money": 5000,
        "GiftToken": 0,
        "BonusPercent": 0,
        "DisplayName": "Gói Khởi Đầu",
        "Description": "Gói nạp cơ bản cho người mới",
        "IsActive": true,
        "SortOrder": 1
      }
    ]
  }
}
```

### Configuration Properties

#### PaymentTiersSettings
- `Enabled` (bool): Bật/tắt tính năng payment tiers
- `Currency` (string): Mã tiền tệ (VD: "PHP", "VND", "USD")
- `MinimumAmount` (decimal): Số tiền tối thiểu cho phép
- `MaximumAmount` (decimal): Số tiền tối đa cho phép
- `Tiers` (array): Danh sách các mốc nạp

#### PaymentTier Properties
- `Id` (int): ID duy nhất của tier
- `Amount` (decimal): Số tiền cần nạp (theo đơn vị currency)
- `Gold` (int): Số vàng nhận được
- `Money` (int): Số Xu nhận được
- `GiftToken` (int): Số Lễ Kim nhận được
- `BonusPercent` (decimal): Phần trăm bonus
- `DisplayName` (string): Tên hiển thị của gói
- `Description` (string): Mô tả chi tiết
- `IsActive` (bool): Trạng thái hoạt động
- `SortOrder` (int): Thứ tự sắp xếp khi hiển thị

## Usage Examples

### Example 1: Lấy tất cả payment tiers
```bash
curl -X GET "https://your-api.com/api/payment/tiers"
```

### Example 2: Lấy tier cụ thể
```bash
curl -X GET "https://your-api.com/api/payment/tiers/3"
```

### Example 3: Sử dụng từ JavaScript
```javascript
// Lấy tất cả tiers
const response = await fetch('/api/payment/tiers');
const data = await response.json();
console.log('Available tiers:', data.data);

// Lấy tier cụ thể
const tier = await fetch('/api/payment/tiers/3');
const tierData = await tier.json();
console.log('Tier details:', tierData.data);
```

## Sample Payment Tiers

Dưới đây là các mốc nạp mẫu đã được cấu hình:

| ID | Amount | Gold | Money | Gift Token | Bonus % | Display Name |
|----|--------|------|-------|------------|---------|--------------|
| 1  | 50     | 500  | 5,000 | 0          | 0%      | Gói Khởi Đầu |
| 2  | 100    | 1,100| 11,000| 50         | 10%     | Gói Tiết Kiệm|
| 3  | 200    | 2,400| 24,000| 150        | 20%     | Gói Phổ Biến |
| 4  | 500    | 6,500| 65,000| 500        | 30%     | Gói Cao Cấp  |
| 5  | 1,000  |14,000|140,000|1,500       | 40%     | Gói VIP      |
| 6  | 2,000  |30,000|300,000|4,000       | 50%     | Gói Đại Gia  |

## Notes

- Tất cả endpoints đều là anonymous (không cần authentication)
- Chỉ các tiers có `IsActive = true` mới được trả về
- Tiers được sắp xếp theo `SortOrder` và `Amount`
- Currency mặc định là PHP (Philippine Peso)
- Có thể tắt tính năng này bằng cách set `Enabled = false` trong config

## Integration with Payment Flow

Khi tích hợp với flow thanh toán:
1. Client gọi `GET /api/payment/tiers` để hiển thị danh sách gói nạp
2. User chọn một gói
3. Client gọi `POST /api/payment/create-checkout-session` với amount tương ứng
4. Sau khi thanh toán thành công, server tự động cộng rewards (Gold, Money, GiftToken)

## Future Enhancements

Có thể mở rộng trong tương lai:
- API để admin thêm/sửa/xóa tiers
- API để activate/deactivate tiers
- Tracking số lần mua cho mỗi tier
- Special events với bonus tiers
- User-specific tiers dựa trên VIP level
