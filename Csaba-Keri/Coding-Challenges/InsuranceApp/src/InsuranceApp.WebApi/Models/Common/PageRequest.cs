using InsuranceApp.Application.Common.Pagination;

namespace InsuranceApp.WebApi.Models.Common;

public record PageRequest(
    int PageNumber = PaginationDefaults.PageNumber,
    int PageSize = PaginationDefaults.PageSize
);
