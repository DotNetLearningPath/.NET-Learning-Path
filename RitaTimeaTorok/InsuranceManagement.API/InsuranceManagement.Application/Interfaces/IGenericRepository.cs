namespace InsuranceManagement.Application.Interfaces;

public interface IGenericRepository<TEntity> where TEntity : class
{
    Task<List<TEntity>> GetAllAsync();

    Task<TEntity?> GetAsync(Guid id);

    Task<TEntity> AddAsync(TEntity entity);

    Task UpdateAsync(TEntity entity);

    Task DeleteAsync(Guid id);
}
