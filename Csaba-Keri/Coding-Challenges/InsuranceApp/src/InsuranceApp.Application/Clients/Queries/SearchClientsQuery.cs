using InsuranceApp.Application.Common.Pagination;

namespace InsuranceApp.Application.Clients.Queries;

public record SearchClientsQuery(
    string? Name = null,
    string? Identifier = null,
    int PageNumber = PaginationDefaults.PageNumber,
    int PageSize = PaginationDefaults.PageSize
) : PageQuery(PageNumber, PageSize);
