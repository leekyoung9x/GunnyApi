# Gunny API - Base Architecture

## Cấu trúc dự án

Dự án sử dụng kiến trúc Repository Pattern và Service Pattern với các tính năng:

### 1. **Infrastructure Layer**

#### Database
- `IDbConnectionFactory`: Interface quản lý kết nối database
- `SqlConnectionFactory`: Implementation sử dụng Microsoft.Data.SqlClient

#### Security
- `SqlInjectionProtection`: Class bảo vệ chống SQL Injection
  - Kiểm tra các pattern nguy hiểm
  - Validate input parameters
  - Sanitize dữ liệu đầu vào

#### Repositories
- `IBaseRepository<T>`: Interface base cho tất cả repositories
- `BaseRepository<T>`: Abstract class với các method CRUD cơ bản
  - Tích hợp SQL Injection Protection
  - Sử dụng Dapper cho data access
  - Async/await pattern

#### Services
- `IBaseService<T>`: Interface base cho business logic
- `BaseService<T>`: Abstract class với validation và business rules

### 2. **Application Layer**

#### Models
- `User`: Model ví dụ

#### Repositories (Concrete)
- `IUserRepository`: Interface cho User repository
- `UserRepository`: Implementation với:
  - SQL Injection validation trên tất cả inputs
  - Custom queries với Dapper
  - Check duplicate username/email

#### Services (Concrete)
- `IUserService`: Interface cho User service
- `UserService`: Implementation với:
  - Business validation
  - SQL Injection protection
  - Duplicate checking

#### Controllers
- `UsersController`: RESTful API endpoints
  - GET all users
  - GET user by ID
  - GET user by username
  - GET active users
  - POST create user
  - PUT update user
  - DELETE user
  - Exception handling với SecurityException

### 3. **Dependency Injection**

Tất cả dependencies được đăng ký trong `Program.cs`:

```csharp
// Database
builder.Services.AddSingleton<IDbConnectionFactory, SqlConnectionFactory>();

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Services
builder.Services.AddScoped<IUserService, UserService>();
```

### 4. **SQL Injection Protection**

#### Các tính năng bảo vệ:

1. **Pattern Detection**: Phát hiện các mẫu SQL nguy hiểm
   - SQL keywords (SELECT, INSERT, UPDATE, DELETE, DROP, etc.)
   - SQL injection patterns (UNION, OR 1=1, etc.)
   - Script injection (script tags, javascript, etc.)
   - System tables (information_schema, sysobjects, etc.)

2. **Input Validation**: 
   - `ValidateInput(string input, string paramName)`: Validate từng input
   - `ValidateInputs(params (string, string)[])`: Validate nhiều inputs
   - Throw `SecurityException` khi phát hiện pattern nguy hiểm

3. **Multi-layer Protection**:
   - Repository layer: Validate trước khi thực thi SQL
   - Service layer: Validate business logic inputs
   - Controller layer: Catch SecurityException

4. **Parameterized Queries**: Dapper sử dụng parameters để tránh SQL injection

### 5. **Cấu hình Database**

Cập nhật connection string trong `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=GunnyDB;User Id=sa;Password=YourPassword123;TrustServerCertificate=True;"
  }
}
```

### 6. **Tạo Database**

Chạy script `Database/CreateDatabase.sql` để tạo:
- Database GunnyDB
- Table Users với các fields cần thiết
- Sample data
- Indexes

### 7. **Sử dụng**

#### Tạo Repository mới:

```csharp
public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    protected override string TableName => "Products";
    protected override string IdColumn => "Id";

    public ProductRepository(IDbConnectionFactory connectionFactory) 
        : base(connectionFactory) { }

    public override async Task<int> AddAsync(Product entity)
    {
        // Validate inputs
        SqlInjectionProtection.ValidateInputs(
            (entity.Name, nameof(entity.Name)),
            (entity.Description, nameof(entity.Description))
        );

        // Your SQL here...
    }
}
```

#### Tạo Service mới:

```csharp
public class ProductService : BaseService<Product>, IProductService
{
    public ProductService(IProductRepository repository) 
        : base(repository) { }

    protected override async Task ValidateEntityAsync(Product entity)
    {
        // Your validation logic
        SqlInjectionProtection.ValidateInput(entity.Name, nameof(entity.Name));
        
        await base.ValidateEntityAsync(entity);
    }
}
```

#### Đăng ký Dependency Injection:

```csharp
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
```

### 8. **Best Practices**

1. **Luôn validate inputs**: Sử dụng `SqlInjectionProtection.ValidateInput()` cho mọi user input
2. **Sử dụng Parameterized Queries**: Dapper tự động handle parameters
3. **Multi-layer validation**: Validate ở cả Repository và Service layer
4. **Exception handling**: Catch `SecurityException` ở Controller
5. **Async/Await**: Tất cả database operations đều async
6. **Dependency Injection**: Inject dependencies qua constructor

### 9. **Testing SQL Injection Protection**

Các test case nguy hiểm sẽ bị chặn:
- `' OR '1'='1`
- `admin'--`
- `'; DROP TABLE Users--`
- `UNION SELECT * FROM Users`
- `<script>alert('xss')</script>`

## Packages

- **Dapper** (2.1.66): Micro ORM
- **Microsoft.Data.SqlClient** (6.1.3): SQL Server client

## Yêu cầu

- .NET 8.0
- SQL Server 2016 trở lên
