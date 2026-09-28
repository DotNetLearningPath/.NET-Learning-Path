namespace InsuranceApp.WebApi.Models.Common;

public class PagedResponse<T>(
    IReadOnlyList<T> items,
    int pageNumber,
    int pageSize,
    long totalCount
)
{
    public IReadOnlyList<T> Items { get; } = items;

    public int PageNumber { get; } = pageNumber;

    public int PageSize { get; } = pageSize;

    public long TotalCount { get; } = totalCount;
}
