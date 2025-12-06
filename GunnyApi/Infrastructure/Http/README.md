# HttpClientService - Hướng dẫn sử dụng

## 1. Cách sử dụng cơ bản

### GET Request - Trả về string
```csharp
// Inject service vào constructor
private readonly IHttpClientService _httpClient;

public MyController(IHttpClientService httpClient)
{
    _httpClient = httpClient;
}

// Gửi GET request
var response = await _httpClient.GetAsync("https://api.example.com/users");
```

### GET Request - Trả về Object
```csharp
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}

// GET và deserialize sang object
var user = await _httpClient.GetAsync<User>("https://api.example.com/users/1");
```

### GET Request với Headers
```csharp
var headers = new Dictionary<string, string>
{
    { "Authorization", "Bearer your-token" },
    { "X-Custom-Header", "value" }
};

var response = await _httpClient.GetAsync("https://api.example.com/users", headers);
```

## 2. POST Request

### POST với JSON Body
```csharp
var userData = new 
{
    Name = "John Doe",
    Email = "john@example.com"
};

// POST và nhận response string
var response = await _httpClient.PostAsync("https://api.example.com/users", userData);

// POST và nhận response object
var createdUser = await _httpClient.PostAsync<User>("https://api.example.com/users", userData);
```

### POST với Form Data
```csharp
var formData = new Dictionary<string, string>
{
    { "username", "john" },
    { "password", "123456" },
    { "remember", "true" }
};

var response = await _httpClient.PostFormAsync("https://api.example.com/login", formData);
```

### POST với Headers
```csharp
var headers = new Dictionary<string, string>
{
    { "Authorization", "Bearer token" },
    { "Content-Type", "application/json" }
};

var response = await _httpClient.PostAsync("https://api.example.com/users", userData, headers);
```

## 3. PUT Request

```csharp
var updateData = new 
{
    Id = 1,
    Name = "Jane Doe",
    Email = "jane@example.com"
};

// PUT và nhận string
var response = await _httpClient.PutAsync("https://api.example.com/users/1", updateData);

// PUT và nhận object
var updatedUser = await _httpClient.PutAsync<User>("https://api.example.com/users/1", updateData);
```

## 4. DELETE Request

```csharp
// DELETE và nhận string
var response = await _httpClient.DeleteAsync("https://api.example.com/users/1");

// DELETE và nhận object
var result = await _httpClient.DeleteAsync<DeleteResult>("https://api.example.com/users/1");
```

## 5. Download File

```csharp
// Download file dưới dạng byte array
var fileBytes = await _httpClient.DownloadFileAsync("https://example.com/files/document.pdf");

// Lưu file
await File.WriteAllBytesAsync("document.pdf", fileBytes);
```

## 6. Ví dụ thực tế

### Example 1: Login và lấy token
```csharp
public class AuthService
{
    private readonly IHttpClientService _httpClient;
    
    public AuthService(IHttpClientService httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<string> LoginAsync(string username, string password)
    {
        var loginData = new 
        {
            Username = username,
            Password = password
        };
        
        var response = await _httpClient.PostAsync<LoginResponse>(
            "https://api.example.com/auth/login", 
            loginData
        );
        
        return response?.Token ?? string.Empty;
    }
}
```

### Example 2: Call API với Authentication
```csharp
public async Task<List<User>> GetUsersAsync(string token)
{
    var headers = new Dictionary<string, string>
    {
        { "Authorization", $"Bearer {token}" }
    };
    
    var users = await _httpClient.GetAsync<List<User>>(
        "https://api.example.com/users", 
        headers
    );
    
    return users ?? new List<User>();
}
```

### Example 3: Upload dữ liệu phức tạp
```csharp
public async Task<bool> UpdateUserProfileAsync(int userId, UserProfile profile, string token)
{
    var headers = new Dictionary<string, string>
    {
        { "Authorization", $"Bearer {token}" }
    };
    
    var response = await _httpClient.PutAsync<ApiResponse>(
        $"https://api.example.com/users/{userId}/profile",
        profile,
        headers
    );
    
    return response?.Success ?? false;
}
```

### Example 4: Xử lý lỗi
```csharp
public async Task<User?> GetUserSafeAsync(int userId)
{
    try
    {
        var user = await _httpClient.GetAsync<User>(
            $"https://api.example.com/users/{userId}"
        );
        
        return user;
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Không thể lấy thông tin user {UserId}", userId);
        return null;
    }
    catch (JsonException ex)
    {
        _logger.LogError(ex, "Lỗi deserialize response cho user {UserId}", userId);
        return null;
    }
}
```

## 7. Tính năng

✅ **Dễ sử dụng**: Interface đơn giản, rõ ràng  
✅ **Type-safe**: Hỗ trợ generic để deserialize tự động  
✅ **Logging**: Tự động log request/response  
✅ **Headers**: Dễ dàng thêm custom headers  
✅ **Error handling**: Xử lý lỗi tập trung  
✅ **Timeout**: Mặc định 30 giây (có thể custom)  
✅ **JSON**: Tự động serialize/deserialize JSON  
✅ **Form data**: Hỗ trợ gửi form data  
✅ **File download**: Download file dễ dàng  

## 8. Cấu hình nâng cao

Nếu cần custom timeout hoặc các settings khác, có thể mở rộng:

```csharp
// Trong HttpClientService.cs
private HttpClient CreateClient(Dictionary<string, string>? headers = null, int timeoutSeconds = 30)
{
    var client = _httpClientFactory.CreateClient();
    client.Timeout = TimeSpan.FromSeconds(timeoutSeconds);
    // ...
}
```

Hoặc tạo overload method:

```csharp
public async Task<string> GetAsync(string url, Dictionary<string, string>? headers = null, int timeoutSeconds = 30)
{
    var client = CreateClient(headers, timeoutSeconds);
    // ...
}
```
