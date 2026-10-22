using InsuranceApp.Application.Policies.Commands;
using InsuranceApp.Application.Policies.Results;

namespace InsuranceApp.Application.Policies;

public interface IPolicyService
{
    Task<PolicyResult> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken);

    Task<PolicyResult> CreateDraftPolicyAsync(CreateDraftPolicyCommand command, CancellationToken cancellationToken);

    Task<PolicyResult> ActivatePolicyAsync(Guid policyId, CancellationToken cancellationToken);

    Task<PolicyResult> CancelPolicyAsync(CancelPolicyCommand command, CancellationToken cancellationToken);

    Task<ExpirePoliciesResult> ExpirePoliciesAsync(CancellationToken cancellationToken);
}
