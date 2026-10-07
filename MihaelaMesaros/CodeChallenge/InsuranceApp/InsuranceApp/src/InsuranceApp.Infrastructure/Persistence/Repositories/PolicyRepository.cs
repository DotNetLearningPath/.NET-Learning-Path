using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.DTOs.Policy;
using InsuranceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

internal sealed class PolicyRepository(InsuranceDbContext dbContext) : IPolicyRepository
{
    public async Task<(IReadOnlyList<Policy> Items, int TotalCount)> SearchPoliciesAsync(PolicySearchDto policySearchDto, CancellationToken cancellationToken)
    {
        var query = dbContext.Policies.AsNoTracking();

        if (policySearchDto.ClientId.HasValue)
        {
            query = query.Where(x => x.ClientId == policySearchDto.ClientId.Value);
        }

        if (policySearchDto.BrokerId.HasValue)
        {
            query = query.Where(x => x.BrokerId == policySearchDto.BrokerId.Value);
        }

        if (policySearchDto.Status.HasValue)
        {
            query = query.Where(x => x.Status == policySearchDto.Status.Value);
        }

        if (policySearchDto.StartDate.HasValue)
        {
            query = query.Where(x => x.StartDate >= policySearchDto.StartDate.Value);
        }

        if (policySearchDto.EndDate.HasValue)
        {
            query = query.Where(x => x.EndDate <= policySearchDto.EndDate.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var policies = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenBy(x => x.PolicyId)
            .Skip((policySearchDto.PageNumber - 1) * policySearchDto.PageSize)
            .Take(policySearchDto.PageSize)
            .ToListAsync(cancellationToken);

        return (policies, totalCount);
    }

    public Task<Policy?> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken)
    {
        return dbContext.Policies.AsNoTracking().FirstOrDefaultAsync(x => x.PolicyId == policyId, cancellationToken);
    }
}
