using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;

namespace Insurance.UnitTest.Application.Fakes;

public sealed class FakePolicyRepository : IPolicyRepository
{
    public List<Policy> AddedPolicies { get; } = [];
    public Policy? Item { get; set; }
    public Policy? UpdatedPolicy { get; private set; }
    public PagedResult<Policy> SearchResult { get; set; } = new();

    public Task AddPolicyAsync(Policy policy, CancellationToken cancellationToken)
    {
        AddedPolicies.Add(policy);
        Item = policy;
        return Task.CompletedTask;
    }

    public Task<Policy?> GetPolicyByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Item?.Id == id ? Item : null);

    public Task<Policy?> GetPolicyByNumberAsync(string policyNumber, CancellationToken cancellationToken) =>
        Task.FromResult(Item?.PolicyNumber == policyNumber ? Item : null);

    public Task<PagedResult<Policy>> SearchPoliciesAsync(
        string? policyNumber,
        Guid? clientId,
        Guid? brokerId,
        PolicyStatus? status,
        DateTime? startDateFrom,
        DateTime? startDateTo,
        PaginationRequest pagination,
        CancellationToken cancellationToken) =>
        Task.FromResult(SearchResult);

    public Task UpdatePolicyAsync(Policy policy, CancellationToken cancellationToken)
    {
        UpdatedPolicy = policy;
        Item = policy;
        return Task.CompletedTask;
    }
}
