using InsuranceApp.Domain.Policies;

namespace InsuranceApp.Application.Policies;

public interface IPolicyRepository
{
    Task<Policy?> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken);

    Task AddPolicyAsync(Policy policy, CancellationToken cancellationToken);
}
