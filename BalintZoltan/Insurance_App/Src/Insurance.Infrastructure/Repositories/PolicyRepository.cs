using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Infrastructure.Extensions;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories;

public sealed class PolicyRepository(
    InsuranceDbContext dbContext) : IPolicyRepository
{
    public async Task AddPolicyAsync(
        Policy policy,
        CancellationToken cancellationToken)
    {
        var brokerStatus = await dbContext.Brokers
            .Where(broker => broker.Id == policy.BrokerId)
            .Select(broker => (BrokerStatus?)broker.Status)
            .FirstOrDefaultAsync(cancellationToken)
            ??
            throw new InvalidOperationException("The selected broker was not found.");

        if (brokerStatus != BrokerStatus.Active)
        {
            throw new InvalidOperationException("Inactive brokers cannot create policies.");
        }

        await dbContext.Policies.AddAsync(policy, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Policy?> GetPolicyByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.Policies
            .AsNoTracking()
            .FirstOrDefaultAsync(policy => policy.Id == id, cancellationToken);

    public Task<Policy?> GetPolicyByNumberAsync(
        string policyNumber,
        CancellationToken cancellationToken) =>
        dbContext.Policies
            .AsNoTracking()
            .FirstOrDefaultAsync(
                policy => policy.PolicyNumber == policyNumber,
                cancellationToken);

    public async Task<PagedResult<Policy>> SearchPoliciesAsync(
        string? policyNumber,
        PolicyStatus? status,
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Policies.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(policyNumber))
        {
            var normalizedPolicyNumber = policyNumber.Trim();
            query = query.Where(policy =>
                policy.PolicyNumber.Contains(normalizedPolicyNumber));
        }

        if (status.HasValue)
        {
            query = query.Where(policy => policy.Status == status.Value);
        }

        return await query
            .OrderBy(policy => policy.PolicyNumber)
            .ThenBy(policy => policy.Id)
            .ToPagedResultAsync(pagination, cancellationToken);
    }

    public async Task UpdatePolicyAsync(
        Policy policy,
        CancellationToken cancellationToken)
    {
        dbContext.Policies.Update(policy);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
