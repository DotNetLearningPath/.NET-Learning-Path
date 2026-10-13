using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Domain.Policies;

namespace InsuranceApp.Application.Policies.Queries;

public record SearchPoliciesQuery(
    Guid? ClientId = null,
    Guid? BrokerId = null,
    PolicyStatus? Status = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    int PageNumber = PaginationDefaults.PageNumber,
    int PageSize = PaginationDefaults.PageSize
) : PageQuery(PageNumber, PageSize);
