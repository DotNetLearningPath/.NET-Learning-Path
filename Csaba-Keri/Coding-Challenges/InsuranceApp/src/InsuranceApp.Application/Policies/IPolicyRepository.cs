using InsuranceApp.Domain.Policies;

namespace InsuranceApp.Application.Policies;

public interface IPolicyRepository
{
    Task<Policy?> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken);

    Task AddPolicyAsync(Policy policy, CancellationToken cancellationToken);

    Task UpdatePolicyLifecycleAsync(Policy policy, PolicyStatus expectedCurrentStatus, CancellationToken cancellationToken);

    Task<int> ExpirePoliciesAsync(DateTimeOffset now, CancellationToken cancellationToken);
}
