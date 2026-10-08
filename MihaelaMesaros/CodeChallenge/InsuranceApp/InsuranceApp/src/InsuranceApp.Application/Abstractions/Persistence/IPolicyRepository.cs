using InsuranceApp.Application.DTOs.Policy;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Application.Abstractions.Persistence;

public interface IPolicyRepository
{
    Task<(IReadOnlyList<Policy> Items, int TotalCount)> SearchPoliciesAsync(PolicySearchDto policySearchDto, CancellationToken cancellationToken);

    Task<Policy?> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken);

    Task<bool> PolicyNumberExistsAsync(string policyNumber, CancellationToken cancellationToken);

    Task AddPolicyAsync(Policy policy, CancellationToken cancellationToken);
}
