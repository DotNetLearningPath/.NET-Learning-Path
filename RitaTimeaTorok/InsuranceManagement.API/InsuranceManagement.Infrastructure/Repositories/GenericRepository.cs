using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Domain.Entities;
using InsuranceManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Infrastructure.Repositories
{
    public class GenericRepository<TEntity>(InsuranceDbContext context) : IGenericRepository<TEntity>
        where TEntity : class, IEntity
    {
        protected readonly InsuranceDbContext _context = context;

        public async Task<IReadOnlyList<TEntity>> GetAllAsync() =>
            await _context.Set<TEntity>().AsNoTracking().ToListAsync();

        public async Task<PagedResult<TEntity>> GetPageAsync(int pageNumber, int pageSize)
        {
            var query = _context.Set<TEntity>().AsNoTracking();
            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(entity => entity.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<TEntity>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }

        public Task<TEntity?> GetAsync(Guid id) =>
            _context.Set<TEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.Id == id);

        public async Task<TEntity> AddAsync(TEntity entity)
        {
            await _context.Set<TEntity>().AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(TEntity entity)
        {
            _context.Set<TEntity>().Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TEntity entity)
        {
            _context.Set<TEntity>().Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}
