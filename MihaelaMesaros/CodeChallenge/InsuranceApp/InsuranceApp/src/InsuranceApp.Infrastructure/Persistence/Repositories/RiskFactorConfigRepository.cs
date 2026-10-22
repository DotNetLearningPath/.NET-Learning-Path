using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Exceptions;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

internal sealed class RiskFactorConfigRepository(InsuranceDbContext dbContext) : IRiskFactorConfigRepository
{
    public async Task<IReadOnlyList<RiskFactorConfig>> GetRiskFactorConfigsAsync(CancellationToken cancellationToken)
    {
        return await dbContext.RiskFactorConfigs
            .AsNoTracking()
            .OrderBy(x => x.Level)
            .ThenBy(x => x.ReferenceId)
            .ToListAsync(cancellationToken);
    }

    public Task<RiskFactorConfig?> GetRiskFactorConfigByIdAsync(Guid riskFactorConfigId, CancellationToken cancellationToken)
    {
        return dbContext.RiskFactorConfigs.AsNoTracking().FirstOrDefaultAsync(x => x.RiskFactorConfigId == riskFactorConfigId, cancellationToken);
    }

    public Task<RiskFactorConfig?> GetRiskFactorConfigForUpdateAsync(Guid riskFactorConfigId, CancellationToken cancellationToken)
    {
        return dbContext.RiskFactorConfigs.FirstOrDefaultAsync(x => x.RiskFactorConfigId == riskFactorConfigId, cancellationToken);
    }

    public async Task<IReadOnlyList<RiskFactorConfig>> GetActiveRiskFactorConfigsAsync(Guid countryId, Guid countyId, Guid cityId, Guid buildingTypeId, CancellationToken cancellationToken)
    {
        return await dbContext.RiskFactorConfigs
            .AsNoTracking()
            .Where(x => x.IsActive
                && (
                    (x.Level == RiskFactorLevel.Country && x.ReferenceId == countryId) ||
                    (x.Level == RiskFactorLevel.County && x.ReferenceId == countyId) ||
                    (x.Level == RiskFactorLevel.City && x.ReferenceId == cityId) ||
                    (x.Level == RiskFactorLevel.BuildingType && x.ReferenceId == buildingTypeId)
                )
            ).ToListAsync(cancellationToken);
    }

    public Task<bool> RiskFactorConfigExistsAsync(RiskFactorLevel level, Guid referenceId, Guid? excludeRiskFactorConfigId, CancellationToken cancellationToken)
    {
        return dbContext.RiskFactorConfigs
            .AsNoTracking()
            .AnyAsync(
                x => x.Level == level &&
                     x.ReferenceId == referenceId &&
                     (!excludeRiskFactorConfigId.HasValue ||
                      x.RiskFactorConfigId != excludeRiskFactorConfigId.Value),
                cancellationToken);
    }

    public async Task<bool> ReferenceExistsAsync(RiskFactorLevel level, Guid referenceId, CancellationToken cancellationToken)
    {
        return level switch
        {
            RiskFactorLevel.Country => await dbContext.Countries.AsNoTracking().AnyAsync(x => x.CountryId == referenceId, cancellationToken),

            RiskFactorLevel.County => await dbContext.Counties.AsNoTracking().AnyAsync(x => x.CountyId == referenceId, cancellationToken),

            RiskFactorLevel.City => await dbContext.Cities.AsNoTracking().AnyAsync(x => x.CityId == referenceId,cancellationToken),

            RiskFactorLevel.BuildingType => await dbContext.BuildingTypes.AsNoTracking().AnyAsync(x => x.BuildingTypeId == referenceId, cancellationToken),

            _ => false
        };
    }

    public async Task AddRiskFactorConfigAsync(RiskFactorConfig riskFactorConfig, CancellationToken cancellationToken)
    {
        dbContext.RiskFactorConfigs.Add(riskFactorConfig);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (DbExceptionHelper.IsUniqueConstraintViolation(ex))
        {
            throw new DuplicateEntityException(nameof(RiskFactorConfig));
        }
    }

    public async Task SaveRiskFactorConfigChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (DbExceptionHelper.IsUniqueConstraintViolation(ex))
        {
            throw new DuplicateEntityException(nameof(RiskFactorConfig));
        }
    }
}