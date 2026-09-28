using InsuranceApp.Application.Common.Pagination;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

internal static class PagingExtensions
{
    public static async Task<PagedResult<TDomain>> ToDomainPageAsync<TEntity, TDomain>(
        this IOrderedQueryable<TEntity> query,
        int pageNumber,
        int pageSize,
        Func<TEntity, TDomain> toDomain,
        CancellationToken cancellationToken = default
    ) where TEntity : class
    {
        var totalCount = await query.LongCountAsync(cancellationToken);
        var offset = ((long)pageNumber - 1) * pageSize;

        if (offset >= totalCount)
        {
            return new([], pageNumber, pageSize, totalCount);
        }

        var entities = await query
            .Skip((int)offset)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new(entities.Select(toDomain), pageNumber, pageSize, totalCount);
    }
}
