using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Insurance.Infrastructure.Extensions;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories;

public sealed class RiskFactorRepository(
    InsuranceDbContext dbContext) : IRiskFactorRepository
{
    public async Task AddRiskFactorAsync(
        RiskFactorConfiguration configuration,
        CancellationToken cancellationToken)
    {
        await dbContext.RiskFactorConfigurations
            .AddAsync(configuration, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<RiskFactorConfiguration?> GetByLevelAndReferenceAsync(
        RiskFactorLevel level,
        string reference,
        CancellationToken cancellationToken) =>
        dbContext.RiskFactorConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(configuration =>
                configuration.Level == level
                && configuration.Reference == reference
                && configuration.IsActive,
                cancellationToken);

    public Task<RiskFactorConfiguration?> GetByBuildingTypeAsync(
        BuildingType buildingType,
        CancellationToken cancellationToken) =>
        dbContext.RiskFactorConfigurations
            .AsNoTracking()
            .FirstOrDefaultAsync(configuration =>
                configuration.Level == RiskFactorLevel.BuildingType
                && configuration.Reference == buildingType.ToString()
                && configuration.IsActive,
                cancellationToken);

    public async Task<List<RiskFactorConfiguration>> GetApplicableRiskFactorsAsync(
        Guid? countryId,
        Guid? countyId,
        Guid? cityId,
        BuildingType? buildingType,
        CancellationToken cancellationToken)
    {
        var countryReference = countryId?.ToString();
        var countyReference = countyId?.ToString();
        var cityReference = cityId?.ToString();
        var buildingTypeReference = buildingType?.ToString();

        return await dbContext.RiskFactorConfigurations
            .AsNoTracking()
            .Where(configuration => configuration.IsActive
                && ((countryReference != null
                        && configuration.Level == RiskFactorLevel.Country
                        && configuration.Reference == countryReference)
                    || (countyReference != null
                        && configuration.Level == RiskFactorLevel.County
                        && configuration.Reference == countyReference)
                    || (cityReference != null
                        && configuration.Level == RiskFactorLevel.City
                        && configuration.Reference == cityReference)
                    || (buildingTypeReference != null
                        && configuration.Level == RiskFactorLevel.BuildingType
                        && configuration.Reference == buildingTypeReference)))
            .OrderBy(configuration => configuration.Level)
            .ThenBy(configuration => configuration.Reference)
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<RiskFactorConfiguration>> ListRiskFactorsAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var query = dbContext.RiskFactorConfigurations.AsNoTracking();
        return await query
            .OrderBy(configuration => configuration.Level)
            .ThenBy(configuration => configuration.Reference)
            .ThenBy(configuration => configuration.Id)
            .ToPagedResultAsync(pagination, cancellationToken);
    }
}
