namespace InsuranceApp.Application.Common.Pagination;

internal static class PagedResultExtensions
{
    public static PagedResult<TDestination> Map<TSource, TDestination>(
        this PagedResult<TSource> page,
        Func<TSource, TDestination> mapItem
    )
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(mapItem);

        return new(
            items: page.Items.Select(mapItem),
            pageNumber: page.PageNumber,
            pageSize: page.PageSize,
            totalCount: page.TotalCount
        );
    }
}
