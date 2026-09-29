using Application.Abstractions;
using Application.DTO.Common;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class RiskFactorRepository : IRiskFactorRepository
{
    private readonly InsuranceDbContext _dbContext;

    public RiskFactorRepository(InsuranceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddRiskFactorAsync(
        RiskFactorConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.RiskFactorConfigurations
            .AddAsync(configuration, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<RiskFactorConfiguration?> GetByLevelAndReferenceAsync(
        RiskFactorLevel level,
        string reference,
        CancellationToken cancellationToken = default) =>
        _dbContext.RiskFactorConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(configuration =>
                configuration.Level == level
                && configuration.Reference == reference
                && configuration.IsActive,
                cancellationToken);

    public Task<RiskFactorConfiguration?> GetByBuildingTypeAsync(
        BuildingType buildingType,
        CancellationToken cancellationToken = default) =>
        _dbContext.RiskFactorConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(configuration =>
                configuration.Level == RiskFactorLevel.BuildingType
                && configuration.Reference == buildingType.ToString()
                && configuration.IsActive,
                cancellationToken);

    public async Task<PagedResult<RiskFactorConfiguration>> ListRiskFactorsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.RiskFactorConfigurations.AsNoTracking();
        return await query
            .OrderBy(configuration => configuration.Level)
            .ThenBy(configuration => configuration.Reference)
            .ThenBy(configuration => configuration.Id)
            .ToPagedResultAsync(pagination, cancellationToken);
    }
}
