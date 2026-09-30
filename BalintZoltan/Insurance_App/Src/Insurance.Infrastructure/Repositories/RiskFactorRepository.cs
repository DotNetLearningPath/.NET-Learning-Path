using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Infrastructure.Extensions;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories;

public sealed class RiskFactorRepository : IRiskFactorRepository
{
    private readonly InsuranceDbContext _dbContext;

    public RiskFactorRepository(InsuranceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddRiskFactorAsync(
        RiskFactorConfiguration configuration,
        CancellationToken cancellationToken)
    {
        await _dbContext.RiskFactorConfigurations
            .AddAsync(configuration, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<RiskFactorConfiguration?> GetByLevelAndReferenceAsync(
        RiskFactorLevel level,
        string reference,
        CancellationToken cancellationToken) =>
        _dbContext.RiskFactorConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(configuration =>
                configuration.Level == level
                && configuration.Reference == reference
                && configuration.IsActive,
                cancellationToken);

    public Task<RiskFactorConfiguration?> GetByBuildingTypeAsync(
        BuildingType buildingType,
        CancellationToken cancellationToken) =>
        _dbContext.RiskFactorConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(configuration =>
                configuration.Level == RiskFactorLevel.BuildingType
                && configuration.Reference == buildingType.ToString()
                && configuration.IsActive,
                cancellationToken);

    public async Task<PagedResult<RiskFactorConfiguration>> ListRiskFactorsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.RiskFactorConfigurations.AsNoTracking();
        return await query
            .OrderBy(configuration => configuration.Level)
            .ThenBy(configuration => configuration.Reference)
            .ThenBy(configuration => configuration.Id)
            .ToPagedResultAsync(pagination, cancellationToken);
    }
}
