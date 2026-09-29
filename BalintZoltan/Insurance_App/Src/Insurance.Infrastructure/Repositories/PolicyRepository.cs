using Application.Abstractions;
using Application.DTO.Common;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class PolicyRepository : IPolicyRepository
{
    private readonly InsuranceDbContext _dbContext;

    public PolicyRepository(InsuranceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddPolicyAsync(
        Policy policy,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Policies.AddAsync(policy, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Policy?> GetPolicyByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        _dbContext.Policies
            .AsNoTracking()
            .FirstOrDefaultAsync(policy => policy.Id == id, cancellationToken);

    public Task<Policy?> GetPolicyByNumberAsync(
        string policyNumber,
        CancellationToken cancellationToken = default) =>
        _dbContext.Policies
            .AsNoTracking()
            .FirstOrDefaultAsync(
                policy => policy.PolicyNumber == policyNumber,
                cancellationToken);

    public async Task<PagedResult<Policy>> SearchPoliciesAsync(
        string? policyNumber,
        PolicyStatus? status,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Policies.AsNoTracking().AsQueryable();

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
        CancellationToken cancellationToken = default)
    {
        _dbContext.Policies.Update(policy);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
