using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.DTOs.Policy;
using InsuranceApp.Application.Exceptions;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Helpers;
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

    public async Task<Policy?> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken)
    {
        return await dbContext.Policies.AsNoTracking().FirstOrDefaultAsync(x => x.PolicyId == policyId, cancellationToken);
    }

    public async Task<Policy?> GetPolicyForUpdateAsync(Guid policyId, CancellationToken cancellationToken)
    {
        return await dbContext.Policies.FirstOrDefaultAsync(x => x.PolicyId == policyId, cancellationToken);
    }

    public async Task<bool> PolicyNumberExistsAsync(string policyNumber, CancellationToken cancellationToken)
    {
        return await dbContext.Policies.AnyAsync(x => x.PolicyNumber == policyNumber, cancellationToken);
    }

    public async Task AddPolicyAsync(Policy policy, CancellationToken cancellationToken)
    {
        dbContext.Policies.Add(policy);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (DbExceptionHelper.IsUniqueConstraintViolation(ex))
        {
            throw new DuplicateEntityException(nameof(Policy));
        }

    }

    public async Task SavePolicyChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> CheckOverlappingPolicyExistsAsync(Guid buildingId, PolicyStatus status, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        return await dbContext.Policies.AnyAsync(x => x.BuildingId == buildingId && x.Status == status && x.StartDate <= endDate && x.EndDate >= startDate, cancellationToken);
    }

}
