using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Policy;

namespace InsuranceApp.Application.Abstractions.Services;

public interface IPolicyService
{
    Task<Result<PagedResult<PolicyDto>>> SearchPoliciesAsync(PolicySearchDto policySearchDto, CancellationToken cancellationToken);

    Task<Result<PolicyDto>> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken);

    Task<Result<PolicyDto>> CreatePolicyAsync(CreatePolicyDto createPolicyDto, CancellationToken cancellationToken);

    Task<Result<PolicyDto>> ActivatePolicyAsync(Guid policyId, CancellationToken cancellationToken);

    Task<Result<PolicyDto>> CancelPolicyAsync(Guid policyId, CancelPolicyDto cancelPolicyDto, CancellationToken cancellationToken);
}
