# SendMoney API

## Mô tả
API để gửi tiền, vàng và lễ kim cho người chơi trong hệ thống Tank.

## Endpoint
```
POST /api/users/send-money
```

## Request Body
```json
{
  "userName": "tên_người_chơi",
  "gold": 1000,
  "money": 5000,
  "giftToken": 100
}
```

### Parameters
- `userName` (string, required): Tên người chơi (NickName)
- `gold` (int): Số vàng muốn gửi (default: 0)
- `money` (int): Số xu muốn gửi (default: 0)
- `giftToken` (int): Số lễ kim muốn gửi (default: 0)

## Response

### Success (200 OK)
```json
{
  "success": true,
  "message": "Đã chuyển thành công <br /> 5000 Xu <br /> 1000 Vàng <br /> 100 Lễ kim <br /> cho tài khoản <strong>tên_người_chơi</strong>"
}
```

### Error - User not found (400 Bad Request)
```json
{
  "success": false,
  "message": "Tài khoản <strong>tên_người_chơi</strong> không tồn tại."
}
```

### Error - Invalid input (400 Bad Request)
```json
{
  "success": false,
  "message": "Tên tài khoản không được để trống"
}
```

## Logic Flow

1. **Validate Input**: Kiểm tra userName và các giá trị money, gold, giftToken
2. **Get Player**: Lấy thông tin người chơi từ database Db_Tank bằng Stored Procedure `SP_Users_SingleByNickName`
3. **Send Mail**: Gửi mail vào database bằng Stored Procedure `SP_Mail_Send` với các thông tin:
   - Title: "Send Money, Gold, Gift Token"
   - Content: "Bạn nhận được X Xu, Y Vàng, Z Lễ kim"
   - Sender: "Administrators"
   - ReceiverID: Player ID
4. **Notify Player**: Gửi thông báo qua CenterService để người chơi nhận mail realtime

## Configuration

Cấu hình CenterService trong [appsettings.json](../appsettings.json):
```json
{
  "CenterServiceSettings": {
    "ServerAddress": "net.tcp://114.29.239.53:2109/"
  }
}
```

## Database

### Connection String
```json
{
  "ConnectionStrings": {
    "TankConnection": "Server=localhost\\MSSQLSERVER01;Database=Db_Tank;User Id=sa;Password=***;TrustServerCertificate=True;"
  }
}
```

### Stored Procedures

#### SP_Users_SingleByNickName
Lấy thông tin người chơi theo nickname.

**Parameters:**
- `@NickName` (nvarchar(200)): Tên người chơi

**Returns:** Player information

#### SP_Mail_Send
Gửi mail cho người chơi.

**Parameters:**
- `@ID` (int, OUTPUT): Mail ID
- `@Title` (nvarchar): Tiêu đề mail
- `@Content` (nvarchar): Nội dung mail
- `@ReceiverID` (int): ID người nhận
- `@Sender` (nvarchar): Người gửi
- `@SenderID` (int): ID người gửi
- `@Gold` (int): Số vàng
- `@Money` (int): Số xu
- `@GiftToken` (int): Số lễ kim
- ... và các parameters khác

## Example Request

### Using cURL
```bash
curl -X POST https://api.example.com/api/users/send-money \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -d '{
    "userName": "player123",
    "gold": 1000,
    "money": 5000,
    "giftToken": 100
  }'
```

### Using C#
```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Authorization = 
    new AuthenticationHeaderValue("Bearer", "YOUR_TOKEN");

var request = new SendMoneyRequest
{
    UserName = "player123",
    Gold = 1000,
    Money = 5000,
    GiftToken = 100
};

var response = await client.PostAsJsonAsync(
    "https://api.example.com/api/users/send-money", 
    request
);

var result = await response.Content.ReadFromJsonAsync<SendMoneyResponse>();
```

## Security
- API yêu cầu xác thực JWT Bearer Token
- Input được validate để chống SQL Injection
- Sử dụng Stored Procedures để bảo mật database

## Dependencies
- Dapper: Database access
- CenterService.Client: Notification service
- SQL Server: Database

## Notes
- Mail sẽ được lưu vào database và người chơi có thể xem trong game
- Notification sẽ được gửi realtime qua CenterService (nếu có)
- Nếu CenterService không hoạt động, mail vẫn được lưu nhưng không có thông báo realtime
