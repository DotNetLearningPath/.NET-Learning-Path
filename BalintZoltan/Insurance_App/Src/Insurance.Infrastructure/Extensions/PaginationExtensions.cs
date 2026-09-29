using Application.DTO.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions;

public static class PaginationExtensions
{
    private const int DefaultMaxPageSize = 100;

    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = Math.Max(pagination.PageNumber, 1);
        var pageSize = Math.Min(Math.Max(pagination.PageSize, 1), DefaultMaxPageSize);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}
