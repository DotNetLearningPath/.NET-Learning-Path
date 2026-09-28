using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.WebApi.Models.Common;

namespace InsuranceApp.WebApi.Models.Clients;

public record SearchClientsRequest(
    string? Name,
    string? Identifier,
    int PageNumber = PaginationDefaults.PageNumber,
    int PageSize = PaginationDefaults.PageSize
) : PageRequest(PageNumber, PageSize);
