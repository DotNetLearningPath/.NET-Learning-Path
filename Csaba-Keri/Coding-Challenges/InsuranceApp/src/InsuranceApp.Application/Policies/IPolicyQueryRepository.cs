using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Policies.Queries;
using InsuranceApp.Application.Policies.Results;

namespace InsuranceApp.Application.Policies;

public interface IPolicyQueryRepository
{
    Task<PagedResult<PolicyListItemResult>> SearchPoliciesAsync(SearchPoliciesQuery query, CancellationToken cancellationToken);
    
    Task<PolicyDetailsResult?> GetPolicyDetailsAsync(Guid policyId, CancellationToken cancellationToken);
}
