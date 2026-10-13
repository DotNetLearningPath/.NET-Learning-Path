using InsuranceApp.Application.Policies.Commands;
using InsuranceApp.Application.Policies.Results;

namespace InsuranceApp.Application.Policies;

public interface IPolicyService
{
    Task<PolicyResult> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken);

    Task<PolicyResult> CreateDraftPolicyAsync(CreateDraftPolicyCommand command, CancellationToken cancellationToken);
}
