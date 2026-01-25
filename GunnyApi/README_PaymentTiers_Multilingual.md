# Multilingual Payment Tiers - Hướng dẫn Đa ngôn ngữ

## Tổng quan

Payment Tiers API hiện đã hỗ trợ đa ngôn ngữ (Vietnamese và English). Tên gói và mô tả sẽ tự động được dịch theo ngôn ngữ mà người dùng yêu cầu.

## Cách hoạt động

### 1. Detection ngôn ngữ

API tự động phát hiện ngôn ngữ theo thứ tự ưu tiên:

1. **Query string**: `?lang=vi` hoặc `?lang=en`
2. **HTTP Header**: `Accept-Language: vi` hoặc `Accept-Language: en`
3. **Default**: `en` (theo config trong `appsettings.json`)

### 2. Translation Keys

Thay vì lưu text trực tiếp, các tier sử dụng **translation keys**:

```json
{
  "DisplayNameKey": "PaymentTier.Tier1.Name",
  "DescriptionKey": "PaymentTier.Tier1.Description"
}
```

### 3. Translations

Các bản dịch được quản lý trong `LocalizationService.cs`:

**Vietnamese:**
```csharp
["PaymentTier.Tier1.Name"] = "Gói Khởi Đầu"
["PaymentTier.Tier1.Description"] = "Gói nạp cơ bản cho người mới"
```

**English:**
```csharp
["PaymentTier.Tier1.Name"] = "Starter Pack"
["PaymentTier.Tier1.Description"] = "Basic top-up package for new players"
```

## API Endpoints

### GET /api/payment/tiers

Lấy danh sách tất cả tiers với tên và mô tả đã được dịch.

**Request Examples:**

```bash
# Vietnamese (default nếu không chỉ định)
curl -X GET "https://api.example.com/api/payment/tiers?lang=vi"

# English
curl -X GET "https://api.example.com/api/payment/tiers?lang=en"

# Using header
curl -X GET "https://api.example.com/api/payment/tiers" \
  -H "Accept-Language: en"
```

**Response Example (English):**

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
      "displayName": "Starter Pack",
      "description": "Basic top-up package for new players",
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
      "displayName": "Value Pack",
      "description": "Get extra 10% bonus and 50 Gift Tokens",
      "isActive": true,
      "sortOrder": 2
    }
  ]
}
```

**Response Example (Vietnamese):**

```json
{
  "success": true,
  "message": "Payment tiers retrieved successfully",
  "currency": "PHP",
  "data": [
    {
      "id": 1,
      "amount": 50,
      "displayName": "Gói Khởi Đầu",
      "description": "Gói nạp cơ bản cho người mới"
    }
  ]
}
```

### GET /api/payment/tiers/{id}

Lấy thông tin một tier cụ thể với ngôn ngữ tương ứng.

**Request Examples:**

```bash
# Vietnamese
curl -X GET "https://api.example.com/api/payment/tiers/3?lang=vi"

# English
curl -X GET "https://api.example.com/api/payment/tiers/3?lang=en"
```

## Translation Keys Reference

### Tier 1 - Gói Khởi Đầu / Starter Pack
- **Key**: `PaymentTier.Tier1.Name`, `PaymentTier.Tier1.Description`
- **VI**: Gói Khởi Đầu - Gói nạp cơ bản cho người mới
- **EN**: Starter Pack - Basic top-up package for new players

### Tier 2 - Gói Tiết Kiệm / Value Pack
- **Key**: `PaymentTier.Tier2.Name`, `PaymentTier.Tier2.Description`
- **VI**: Gói Tiết Kiệm - Nhận thêm 10% bonus và 50 Lễ Kim
- **EN**: Value Pack - Get extra 10% bonus and 50 Gift Tokens

### Tier 3 - Gói Phổ Biến / Popular Pack
- **Key**: `PaymentTier.Tier3.Name`, `PaymentTier.Tier3.Description`
- **VI**: Gói Phổ Biến - Nhận thêm 20% bonus và 150 Lễ Kim
- **EN**: Popular Pack - Get extra 20% bonus and 150 Gift Tokens

### Tier 4 - Gói Cao Cấp / Premium Pack
- **Key**: `PaymentTier.Tier4.Name`, `PaymentTier.Tier4.Description`
- **VI**: Gói Cao Cấp - Nhận thêm 30% bonus và 500 Lễ Kim
- **EN**: Premium Pack - Get extra 30% bonus and 500 Gift Tokens

### Tier 5 - Gói VIP / VIP Pack
- **Key**: `PaymentTier.Tier5.Name`, `PaymentTier.Tier5.Description`
- **VI**: Gói VIP - Nhận thêm 40% bonus và 1500 Lễ Kim
- **EN**: VIP Pack - Get extra 40% bonus and 1500 Gift Tokens

### Tier 6 - Gói Đại Gia / Tycoon Pack
- **Key**: `PaymentTier.Tier6.Name`, `PaymentTier.Tier6.Description`
- **VI**: Gói Đại Gia - Nhận thêm 50% bonus và 4000 Lễ Kim
- **EN**: Tycoon Pack - Get extra 50% bonus and 4000 Gift Tokens

## Frontend Integration Examples

### JavaScript / React

```javascript
// Fetch tiers with user's preferred language
const fetchPaymentTiers = async (language = 'en') => {
  const response = await fetch(`/api/payment/tiers?lang=${language}`);
  const data = await response.json();
  return data.data;
};

// Or use browser's language
const userLang = navigator.language.split('-')[0]; // 'en' or 'vi'
const tiers = await fetchPaymentTiers(userLang);
```

### Using Headers

```javascript
const fetchPaymentTiers = async () => {
  const response = await fetch('/api/payment/tiers', {
    headers: {
      'Accept-Language': 'vi-VN'
    }
  });
  const data = await response.json();
  return data.data;
};
```

### Display Example

```jsx
function PaymentTierList() {
  const [tiers, setTiers] = useState([]);
  const [language, setLanguage] = useState('en');

  useEffect(() => {
    fetch(`/api/payment/tiers?lang=${language}`)
      .then(res => res.json())
      .then(data => setTiers(data.data));
  }, [language]);

  return (
    <div>
      <button onClick={() => setLanguage('vi')}>Tiếng Việt</button>
      <button onClick={() => setLanguage('en')}>English</button>
      
      {tiers.map(tier => (
        <div key={tier.id}>
          <h3>{tier.displayName}</h3>
          <p>{tier.description}</p>
          <p>Amount: {tier.amount} {data.currency}</p>
          <p>Gold: {tier.gold} | Money: {tier.money} | Gift Token: {tier.giftToken}</p>
          {tier.bonusPercent > 0 && <span>+{tier.bonusPercent}% Bonus!</span>}
        </div>
      ))}
    </div>
  );
}
```

## Thêm ngôn ngữ mới

Để thêm ngôn ngữ mới (ví dụ: Tagalog - `tl`):

### 1. Thêm vào LocalizationService.cs

```csharp
// Tagalog translations
_translations["tl"] = new Dictionary<string, string>
{
    ["PaymentTier.Tier1.Name"] = "Starter Pack",
    ["PaymentTier.Tier1.Description"] = "Basic na package para sa mga baguhan",
    // ... thêm các keys khác
};
```

### 2. Cập nhật appsettings.json

```json
{
  "LocalizationSettings": {
    "DefaultLanguage": "en",
    "SupportedLanguages": ["vi", "en", "tl"]
  }
}
```

### 3. Sử dụng

```bash
curl -X GET "/api/payment/tiers?lang=tl"
```

## Thêm Tier mới

Để thêm tier mới với đa ngôn ngữ:

### 1. Thêm vào appsettings.json

```json
{
  "Id": 7,
  "Amount": 5000,
  "Gold": 75000,
  "Money": 750000,
  "GiftToken": 10000,
  "BonusPercent": 60,
  "DisplayNameKey": "PaymentTier.Tier7.Name",
  "DescriptionKey": "PaymentTier.Tier7.Description",
  "IsActive": true,
  "SortOrder": 7
}
```

### 2. Thêm translations vào LocalizationService.cs

**Vietnamese:**
```csharp
["PaymentTier.Tier7.Name"] = "Gói Siêu VIP",
["PaymentTier.Tier7.Description"] = "Nhận thêm 60% bonus và 10000 Lễ Kim",
```

**English:**
```csharp
["PaymentTier.Tier7.Name"] = "Ultra VIP Pack",
["PaymentTier.Tier7.Description"] = "Get extra 60% bonus and 10000 Gift Tokens",
```

## Best Practices

1. **Luôn sử dụng translation keys** thay vì hardcode text
2. **Cung cấp fallback** - nếu thiếu translation, hiển thị default language
3. **Test với cả 2 ngôn ngữ** trước khi deploy
4. **Consistent naming** - dùng convention `PaymentTier.TierX.Name/Description`
5. **Document changes** - cập nhật docs khi thêm tier/language mới

## Testing

```bash
# Test Vietnamese
curl -X GET "http://localhost:5000/api/payment/tiers?lang=vi"

# Test English
curl -X GET "http://localhost:5000/api/payment/tiers?lang=en"

# Test default (should be English based on config)
curl -X GET "http://localhost:5000/api/payment/tiers"

# Test with header
curl -X GET "http://localhost:5000/api/payment/tiers" \
  -H "Accept-Language: vi-VN,vi;q=0.9,en;q=0.8"
```

## Troubleshooting

### Vẫn hiển thị tiếng Việt dù config là English

**Nguyên nhân**: Browser gửi `Accept-Language: vi` trong header

**Giải pháp**: 
- Gửi explicit `?lang=en` trong query string
- Hoặc override header: `Accept-Language: en`

### Thiếu translation key

**Triệu chứng**: API trả về key thay vì text (vd: "PaymentTier.Tier1.Name")

**Giải pháp**: Kiểm tra `LocalizationService.cs` đã có key cho cả vi và en

### Không detect được language từ header

**Kiểm tra**: 
- `LocalizationSettings.EnableHeaderDetection` phải là `true`
- `LanguageDetectionMiddleware` phải được register trong `Program.cs`

## Summary

✅ Tất cả payment tiers đã hỗ trợ đa ngôn ngữ
✅ Tự động detect language từ query string hoặc header
✅ Dễ dàng thêm ngôn ngữ mới
✅ Dễ dàng thêm tier mới với translations
✅ Fallback mechanism đảm bảo luôn có text hiển thị
