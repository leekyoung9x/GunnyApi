using Dapper;
using GunnyApi.Infrastructure.Database;
using GunnyApi.Infrastructure.Security;
using System.Data;

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
}
