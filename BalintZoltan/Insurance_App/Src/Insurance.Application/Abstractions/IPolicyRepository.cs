using Insurance.Application.DTO.Common;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;

namespace Insurance.Application.Abstractions;

public interface IPolicyRepository
{
    Task AddPolicyAsync(Policy policy, CancellationToken cancellationToken);

    Task<Policy?> GetPolicyByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<Policy?> GetPolicyByNumberAsync(
        string policyNumber,
        CancellationToken cancellationToken);

    Task<PagedResult<Policy>> SearchPoliciesAsync(
        string? policyNumber,
        PolicyStatus? status,
        PaginationRequest pagination,
        CancellationToken cancellationToken);

    Task UpdatePolicyAsync(
        Policy policy,
        CancellationToken cancellationToken);
}
