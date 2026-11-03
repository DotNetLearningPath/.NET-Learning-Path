using InsuranceManagement.Domain.Entities;

namespace InsuranceManagement.Application.Interfaces.Repositories;

public interface IGenericRepository<TEntity> 
    where TEntity : class, IEntity
{
    Task<List<TEntity>> GetAllAsync();

    Task<TEntity?> GetAsync(Guid id);

    Task<TEntity> AddAsync(TEntity entity);

    Task UpdateAsync(TEntity entity);

    Task DeleteAsync(TEntity entity);
}
