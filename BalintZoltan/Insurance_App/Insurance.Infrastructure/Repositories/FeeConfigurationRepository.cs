using Application.Abstractions;
using Application.DTO.Common;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class FeeConfigurationRepository : IFeeConfigurationRepository
{
    private readonly InsuranceDbContext _dbContext;

    public FeeConfigurationRepository(InsuranceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddFeeConfigurationAsync(
        FeeConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.FeeConfigurations.AddAsync(configuration, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<FeeConfiguration?> GetActiveFeeConfigurationAsync(
        FeeType type,
        DateTime effectiveAt,
        CancellationToken cancellationToken = default) =>
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

    public async Task<PagedResult<FeeConfiguration>> ListFeeConfigurationsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.FeeConfigurations.AsNoTracking();
        return await query
            .OrderBy(configuration => configuration.Name)
            .ThenByDescending(configuration => configuration.EffectiveFrom)
            .ThenBy(configuration => configuration.Id)
            .ToPagedResultAsync(pagination, cancellationToken);
    }
}
