namespace InsuranceApp.Application.Common.Pagination;

public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; }
    public int PageNumber { get; }
    public int PageSize { get; }
    public long TotalCount { get; }

    public PagedResult(IEnumerable<T> items, int pageNumber, int pageSize, long totalCount)
    {
        ArgumentNullException.ThrowIfNull(items);

        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);

        if (pageSize is < 1 or > PaginationDefaults.MaxPageSize)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        ArgumentOutOfRangeException.ThrowIfNegative(totalCount);

        Items = Array.AsReadOnly(items.ToArray());
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}
