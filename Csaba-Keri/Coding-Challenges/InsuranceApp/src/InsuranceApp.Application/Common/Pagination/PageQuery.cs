namespace InsuranceApp.Application.Common.Pagination;

public record PageQuery(
    int PageNumber = PaginationDefaults.PageNumber,
    int PageSize = PaginationDefaults.PageSize
);
