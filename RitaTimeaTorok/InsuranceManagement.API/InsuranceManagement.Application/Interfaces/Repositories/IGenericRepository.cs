using InsuranceManagement.Domain.Entities;
using InsuranceManagement.Application.DTOs.Pagination;

namespace InsuranceManagement.Application.Interfaces.Repositories
{
    public interface IGenericRepository<TEntity>
        where TEntity : class, IEntity
    {
        Task<IReadOnlyList<TEntity>> GetAllAsync();

        Task<PagedResult<TEntity>> GetPageAsync(int pageNumber, int pageSize);

        Task<TEntity?> GetAsync(Guid id);

        Task<TEntity> AddAsync(TEntity entity);

        Task UpdateAsync(TEntity entity);

        Task DeleteAsync(TEntity entity);
    }
}
