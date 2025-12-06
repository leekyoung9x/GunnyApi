namespace GunnyApi.Infrastructure.Repositories;

/// <summary>
/// Interface base cho tất cả repositories
/// </summary>
public interface IBaseRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<int> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<T>> QueryAsync(string sql, object? param = null);
    Task<T?> QueryFirstOrDefaultAsync(string sql, object? param = null);
    Task<int> ExecuteAsync(string sql, object? param = null);
}
