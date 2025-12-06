using Dapper;
using GunnyApi.Infrastructure.Database;
using GunnyApi.Infrastructure.Security;
using System.Data;
using System.Reflection;

namespace GunnyApi.Infrastructure.Repositories;

/// <summary>
/// Base Repository với Dapper và bảo vệ SQL Injection
/// </summary>
public abstract class BaseRepository<T> : IBaseRepository<T> where T : class
{
    protected readonly IDbConnectionFactory _connectionFactory;
    protected abstract string TableName { get; }
    protected abstract string IdColumn { get; }

    protected BaseRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Thực thi query với SQL Injection protection
    /// </summary>
    protected async Task<IEnumerable<TResult>> ExecuteQueryAsync<TResult>(
        string sql, 
        object? param = null,
        bool validateSql = true)
    {
        if (validateSql)
        {
            ValidateSqlQuery(sql);
        }

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryAsync<TResult>(sql, param);
    }

    /// <summary>
    /// Thực thi command với SQL Injection protection
    /// </summary>
    protected async Task<int> ExecuteCommandAsync(
        string sql, 
        object? param = null,
        bool validateSql = true)
    {
        if (validateSql)
        {
            ValidateSqlQuery(sql);
        }

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteAsync(sql, param);
    }

    /// <summary>
    /// Query single record với SQL Injection protection
    /// </summary>
    protected async Task<TResult?> ExecuteQueryFirstOrDefaultAsync<TResult>(
        string sql, 
        object? param = null,
        bool validateSql = true)
    {
        if (validateSql)
        {
            ValidateSqlQuery(sql);
        }

        using var connection = _connectionFactory.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<TResult>(sql, param);
    }

    /// <summary>
    /// Validate SQL query để phát hiện SQL injection
    /// </summary>
    private void ValidateSqlQuery(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
        {
            throw new ArgumentException("SQL query không được rỗng", nameof(sql));
        }

        // Kiểm tra các pattern nguy hiểm trong SQL query
        if (SqlInjectionProtection.IsSuspicious(sql))
        {
            throw new SecurityException(
                "SQL query chứa mẫu nguy hiểm và đã bị chặn để bảo vệ hệ thống.");
        }
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        var sql = $"SELECT * FROM {TableName} WHERE {IdColumn} = @Id";
        return await ExecuteQueryFirstOrDefaultAsync<T>(sql, new { Id = id }, validateSql: false);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        var sql = $"SELECT * FROM {TableName}";
        return await ExecuteQueryAsync<T>(sql, validateSql: false);
    }

    public virtual async Task<int> AddAsync(T entity)
    {
        // Override trong derived class để implement logic cụ thể
        throw new NotImplementedException("Override AddAsync trong derived repository");
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        // Override trong derived class để implement logic cụ thể
        throw new NotImplementedException("Override UpdateAsync trong derived repository");
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        var sql = $"DELETE FROM {TableName} WHERE {IdColumn} = @Id";
        var affectedRows = await ExecuteCommandAsync(sql, new { Id = id }, validateSql: false);
        return affectedRows > 0;
    }

    public async Task<IEnumerable<T>> QueryAsync(string sql, object? param = null)
    {
        return await ExecuteQueryAsync<T>(sql, param);
    }

    public async Task<T?> QueryFirstOrDefaultAsync(string sql, object? param = null)
    {
        return await ExecuteQueryFirstOrDefaultAsync<T>(sql, param);
    }

    public async Task<int> ExecuteAsync(string sql, object? param = null)
    {
        return await ExecuteCommandAsync(sql, param);
    }

    /// <summary>
    /// Execute stored procedure và trả về DynamicParameters chứa tất cả output values
    /// Linh hoạt nhất - không cần tạo class, lấy output trực tiếp từ DynamicParameters
    /// </summary>
    /// <param name="storeName">Tên stored procedure</param>
    /// <param name="inputParams">Object chứa input parameters (anonymous object hoặc class)</param>
    /// <param name="outputParamsDef">Dictionary định nghĩa output parameters: Key = tên param, Value = DbType</param>
    /// <returns>DynamicParameters chứa tất cả giá trị output, dùng .Get&lt;T&gt;("@ParamName") để lấy giá trị</returns>
    protected async Task<DynamicParameters> ExecuteStoredProcedureAsync(
        string storeName,
        object? inputParams = null,
        Dictionary<string, DbType>? outputParamsDef = null)
    {
        // Validate store name để chống SQL injection
        SqlInjectionProtection.ValidateInput(storeName, nameof(storeName));

        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();

        // Thêm input parameters từ object
        if (inputParams != null)
        {
            parameters.AddDynamicParams(inputParams);
        }

        // Thêm output parameters từ dictionary
        if (outputParamsDef != null)
        {
            foreach (var (paramName, dbType) in outputParamsDef)
            {
                parameters.Add(paramName, dbType: dbType, direction: ParameterDirection.Output);
            }
        }

        // Execute stored procedure
        await connection.ExecuteAsync(
            storeName,
            parameters,
            commandType: CommandType.StoredProcedure
        );

        return parameters;
    }

    /// <summary>
    /// Execute stored procedure với tự động convert object sang DynamicParameters
    /// Hỗ trợ phân biệt input và output parameters - trả về strongly-typed object
    /// </summary>
    /// <typeparam name="TInput">Type của object input</typeparam>
    /// <typeparam name="TOutput">Type của object output</typeparam>
    /// <param name="storeName">Tên stored procedure</param>
    /// <param name="inputParams">Object chứa input parameters</param>
    /// <param name="outputParams">Object chứa output parameters (các property sẽ được map sang output)</param>
    /// <returns>Object chứa giá trị của output parameters</returns>
    protected async Task<TOutput?> ExecuteStoredProcedureAsync<TInput, TOutput>(
        string storeName, 
        TInput? inputParams = null,
        TOutput? outputParams = null) 
        where TInput : class
        where TOutput : class, new()
    {
        // Validate store name để chống SQL injection
        SqlInjectionProtection.ValidateInput(storeName, nameof(storeName));

        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();

        // Thêm input parameters
        if (inputParams != null)
        {
            var inputProperties = typeof(TInput).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in inputProperties)
            {
                var value = prop.GetValue(inputParams);
                parameters.Add($"@{prop.Name}", value, GetDbType(prop.PropertyType), ParameterDirection.Input);
            }
        }

        // Thêm output parameters
        var outputProperties = typeof(TOutput).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in outputProperties)
        {
            parameters.Add($"@{prop.Name}", dbType: GetDbType(prop.PropertyType), direction: ParameterDirection.Output);
        }

        // Execute stored procedure
        await connection.ExecuteAsync(
            storeName,
            parameters,
            commandType: CommandType.StoredProcedure
        );

        // Map output parameters về object
        var result = new TOutput();
        foreach (var prop in outputProperties)
        {
            var value = parameters.Get<object>($"@{prop.Name}");
            if (value != null && value != DBNull.Value)
            {
                // Convert về đúng type nếu cần
                var convertedValue = Convert.ChangeType(value, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                prop.SetValue(result, convertedValue);
            }
        }

        return result;
    }

    /// <summary>
    /// Execute stored procedure chỉ với input parameters
    /// </summary>
    protected async Task ExecuteStoredProcedureAsync<TInput>(
        string storeName, 
        TInput? inputParams = null) 
        where TInput : class
    {
        // Validate store name để chống SQL injection
        SqlInjectionProtection.ValidateInput(storeName, nameof(storeName));

        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();

        // Thêm input parameters
        if (inputParams != null)
        {
            var inputProperties = typeof(TInput).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in inputProperties)
            {
                var value = prop.GetValue(inputParams);
                parameters.Add($"@{prop.Name}", value, GetDbType(prop.PropertyType), ParameterDirection.Input);
            }
        }

        // Execute stored procedure
        await connection.ExecuteAsync(
            storeName,
            parameters,
            commandType: CommandType.StoredProcedure
        );
    }

    /// <summary>
    /// Execute stored procedure chỉ với output parameters
    /// </summary>
    protected async Task<TOutput?> ExecuteStoredProcedureAsync<TOutput>(
        string storeName) 
        where TOutput : class, new()
    {
        // Validate store name để chống SQL injection
        SqlInjectionProtection.ValidateInput(storeName, nameof(storeName));

        using var connection = _connectionFactory.CreateConnection();
        var parameters = new DynamicParameters();

        // Thêm output parameters
        var outputProperties = typeof(TOutput).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in outputProperties)
        {
            parameters.Add($"@{prop.Name}", dbType: GetDbType(prop.PropertyType), direction: ParameterDirection.Output);
        }

        // Execute stored procedure
        await connection.ExecuteAsync(
            storeName,
            parameters,
            commandType: CommandType.StoredProcedure
        );

        // Map output parameters về object
        var result = new TOutput();
        foreach (var prop in outputProperties)
        {
            var value = parameters.Get<object>($"@{prop.Name}");
            if (value != null && value != DBNull.Value)
            {
                var convertedValue = Convert.ChangeType(value, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
                prop.SetValue(result, convertedValue);
            }
        }

        return result;
    }

    /// <summary>
    /// Map C# type sang DbType
    /// </summary>
    private DbType GetDbType(Type type)
    {
        // Handle nullable types
        var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        return underlyingType.Name switch
        {
            nameof(Int32) => DbType.Int32,
            nameof(Int64) => DbType.Int64,
            nameof(String) => DbType.String,
            nameof(Boolean) => DbType.Boolean,
            nameof(DateTime) => DbType.DateTime,
            nameof(Decimal) => DbType.Decimal,
            nameof(Double) => DbType.Double,
            nameof(Single) => DbType.Single,
            nameof(Byte) => DbType.Byte,
            nameof(Int16) => DbType.Int16,
            nameof(Guid) => DbType.Guid,
            _ => DbType.Object
        };
    }
}
