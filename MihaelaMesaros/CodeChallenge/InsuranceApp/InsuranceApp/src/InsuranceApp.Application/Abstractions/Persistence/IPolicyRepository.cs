using InsuranceApp.Application.DTOs.Policy;
using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Application.Abstractions.Persistence;

public interface IPolicyRepository
{
    Task<(IReadOnlyList<Policy> Items, int TotalCount)> SearchPoliciesAsync(PolicySearchDto policySearchDto, Guid brokerId, CancellationToken cancellationToken);

    Task<Policy?> GetPolicyByIdAsync(Guid policyId, Guid brokerId, CancellationToken cancellationToken);

    Task<Policy?> GetPolicyForUpdateAsync(Guid policyId, Guid brokerId, CancellationToken cancellationToken);

    Task<bool> PolicyNumberExistsAsync(string policyNumber, CancellationToken cancellationToken);

    Task AddPolicyAsync(Policy policy, CancellationToken cancellationToken);

    Task SavePolicyChangesAsync(CancellationToken cancellationToken);
    
}
