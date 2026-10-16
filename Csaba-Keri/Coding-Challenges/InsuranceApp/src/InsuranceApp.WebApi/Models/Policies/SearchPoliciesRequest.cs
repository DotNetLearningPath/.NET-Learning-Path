using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.WebApi.Models.Common;

namespace InsuranceApp.WebApi.Models.Policies;

public record SearchPoliciesRequest(
    Guid? ClientId = null,
    Guid? BrokerId = null,
    PolicyStatusDto? Status = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    int PageNumber = PaginationDefaults.PageNumber,
    int PageSize = PaginationDefaults.PageSize
) : PageRequest(PageNumber, PageSize);
