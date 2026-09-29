using Application.DTO.Common;
using Domain.Entities;
using Domain.Enums;

namespace Application.Abstractions;

public interface IPolicyRepository
{
    Task AddPolicyAsync(Policy policy, CancellationToken cancellationToken = default);

    Task<Policy?> GetPolicyByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Policy?> GetPolicyByNumberAsync(
        string policyNumber,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Policy>> SearchPoliciesAsync(
        string? policyNumber,
        PolicyStatus? status,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);

    Task UpdatePolicyAsync(
        Policy policy,
        CancellationToken cancellationToken = default);
}
