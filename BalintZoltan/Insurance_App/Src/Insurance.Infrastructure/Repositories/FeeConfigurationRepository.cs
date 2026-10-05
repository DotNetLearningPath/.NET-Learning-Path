using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Insurance.Infrastructure.Extensions;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories;

public sealed class FeeConfigurationRepository(
    InsuranceDbContext dbContext) : IFeeConfigurationRepository
{
    public async Task AddFeeConfigurationAsync(
        FeeConfiguration configuration,
        CancellationToken cancellationToken)
    {
        await dbContext.FeeConfigurations.AddAsync(configuration, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<FeeConfiguration?> GetActiveFeeConfigurationAsync(
        FeeType type,
        DateTime effectiveAt,
        CancellationToken cancellationToken) =>
        dbContext.FeeConfigurations
            .AsNoTracking()
            .Where(configuration =>
                configuration.Type == type
                && configuration.IsActive
                && configuration.EffectiveFrom <= effectiveAt
                && (!configuration.EffectiveTo.HasValue
                    || configuration.EffectiveTo.Value >= effectiveAt))
            .OrderByDescending(configuration => configuration.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IReadOnlyCollection<FeeConfiguration>> GetActiveFeeConfigurationsAsync(
        DateTime effectiveAt,
        CancellationToken cancellationToken) =>
        await dbContext.FeeConfigurations
            .AsNoTracking()
            .Where(configuration =>
                configuration.IsActive
                && configuration.EffectiveFrom <= effectiveAt
                && (!configuration.EffectiveTo.HasValue
                    || configuration.EffectiveTo.Value >= effectiveAt))
            .OrderBy(configuration => configuration.Type)
            .ThenBy(configuration => configuration.Name)
            .ToListAsync(cancellationToken);

    public Task<FeeConfiguration?> GetFeeConfigurationByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.FeeConfigurations
            .FirstOrDefaultAsync(configuration => configuration.Id == id, cancellationToken);

    public async Task UpdateFeeConfigurationAsync(
        FeeConfiguration configuration,
        CancellationToken cancellationToken)
    {
        dbContext.FeeConfigurations.Update(configuration);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateFeeConfigurationAsync(
        FeeConfiguration configuration,
        CancellationToken cancellationToken)
    {
        configuration.Deactivate();
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<FeeConfiguration>> ListFeeConfigurationsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var query = dbContext.FeeConfigurations.AsNoTracking();
        return await query
            .OrderBy(configuration => configuration.Name)
            .ThenByDescending(configuration => configuration.EffectiveFrom)
            .ThenBy(configuration => configuration.Id)
            .ToPagedResultAsync(pagination, cancellationToken);
    }
}
