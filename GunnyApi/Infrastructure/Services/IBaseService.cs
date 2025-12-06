namespace GunnyApi.Infrastructure.Services;

/// <summary>
/// Interface base cho tất cả services
/// </summary>
public interface IBaseService<T> where T : class
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<int> CreateAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}
