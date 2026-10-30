using InsuranceManagement.Application.Interfaces;
using InsuranceManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Infrastructure.Repositories;

public class GenericRepository<TEntity>(InsuranceDbContext context)
    : IGenericRepository<TEntity> where TEntity : class
{
    public async Task<List<TEntity>> GetAllAsync() =>
        await context.Set<TEntity>().AsNoTracking().ToListAsync();

    public Task<TEntity?> GetAsync(Guid id) =>
        context.Set<TEntity>().AsNoTracking().FirstOrDefaultAsync(entity =>
            EF.Property<Guid>(entity, "Id") == id);

    public async Task<TEntity> AddAsync(TEntity entity)
    {
        await context.Set<TEntity>().AddAsync(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(TEntity entity)
    {
        context.Set<TEntity>().Update(entity);
        await context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await context.Set<TEntity>().FindAsync(id);
        if (entity is null)
        {
            return;
        }

        context.Set<TEntity>().Remove(entity);
        await context.SaveChangesAsync();
    }
}
