using InsuranceApp.Application.Common.Pagination;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

internal static class PagingExtensions
{
    public static async Task<PagedResult<TResult>> ToPageAsync<TSource, TResult>(
        this IOrderedQueryable<TSource> query,
        int pageNumber,
        int pageSize,
        Func<TSource, TResult> mapItem,
        CancellationToken cancellationToken = default
    ) where TSource : class
    {
        var totalCount = await query.LongCountAsync(cancellationToken);
        var offset = ((long)pageNumber - 1) * pageSize;

        if (offset >= totalCount)
        {
            return new([], pageNumber, pageSize, totalCount);
        }

        var items = await query
            .Skip((int)offset)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new(
            items: items.Select(mapItem),
            pageNumber: pageNumber,
            pageSize: pageSize,
            totalCount: totalCount
        );
    }
}
