using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Infrastructure.Extensions;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories;

public sealed class FeeConfigurationRepository : IFeeConfigurationRepository
{
    private readonly InsuranceDbContext _dbContext;

    public FeeConfigurationRepository(InsuranceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddFeeConfigurationAsync(
        FeeConfiguration configuration,
        CancellationToken cancellationToken)
    {
        await _dbContext.FeeConfigurations.AddAsync(configuration, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<FeeConfiguration?> GetActiveFeeConfigurationAsync(
        FeeType type,
        DateTime effectiveAt,
        CancellationToken cancellationToken) =>
        _dbContext.FeeConfigurations
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
        await _dbContext.FeeConfigurations
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
        _dbContext.FeeConfigurations
            .FirstOrDefaultAsync(configuration => configuration.Id == id, cancellationToken);

    public async Task UpdateFeeConfigurationAsync(
        FeeConfiguration configuration,
        CancellationToken cancellationToken)
    {
        _dbContext.FeeConfigurations.Update(configuration);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateFeeConfigurationAsync(
        FeeConfiguration configuration,
        CancellationToken cancellationToken)
    {
        configuration.Deactivate();
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<FeeConfiguration>> ListFeeConfigurationsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.FeeConfigurations.AsNoTracking();
        return await query
            .OrderBy(configuration => configuration.Name)
            .ThenByDescending(configuration => configuration.EffectiveFrom)
            .ThenBy(configuration => configuration.Id)
            .ToPagedResultAsync(pagination, cancellationToken);
    }
}
