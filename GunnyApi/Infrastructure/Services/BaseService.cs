using GunnyApi.Infrastructure.Repositories;

namespace GunnyApi.Infrastructure.Services;

/// <summary>
/// Base Service chứa business logic chung
/// </summary>
public abstract class BaseService<T> : IBaseService<T> where T : class
{
    protected readonly IBaseRepository<T> _repository;

    protected BaseService(IBaseRepository<T> repository)
    {
        _repository = repository;
    }

    public virtual async Task<T?> GetByIdAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
        }

        return await _repository.GetByIdAsync(id);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public virtual async Task<int> CreateAsync(T entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        // Validate entity trước khi tạo
        await ValidateEntityAsync(entity);

        return await _repository.AddAsync(entity);
    }

    public virtual async Task<bool> UpdateAsync(T entity)
    {
        if (entity == null)
        {
            throw new ArgumentNullException(nameof(entity));
        }

        // Validate entity trước khi update
        await ValidateEntityAsync(entity);

        return await _repository.UpdateAsync(entity);
    }

    public virtual async Task<bool> DeleteAsync(int id)
    {
        if (id <= 0)
        {
            throw new ArgumentException("ID phải lớn hơn 0", nameof(id));
        }

        // Kiểm tra entity có tồn tại không
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null)
        {
            return false;
        }

        return await _repository.DeleteAsync(id);
    }

    /// <summary>
    /// Validate entity - override trong derived class để add thêm validation
    /// </summary>
    protected virtual Task ValidateEntityAsync(T entity)
    {
        // Override trong derived class để implement validation logic
        return Task.CompletedTask;
    }
}
