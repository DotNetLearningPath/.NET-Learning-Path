using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Domain.Entities.Metadata;
using InsuranceManagement.Domain.Enums;
using InsuranceManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Infrastructure.Repositories
{
    public class RiskFactorConfigurationRepository(InsuranceDbContext context)
        : GenericRepository<RiskFactorConfiguration>(context), IRiskFactorConfigurationRepository
    {
        public Task<bool> ExistsByCountryIdAsync(Guid countryId) =>
            _context.RiskFactorConfigurations.AnyAsync(configuration => configuration.CountryId == countryId);

        public Task<bool> ExistsByCountyIdAsync(Guid countyId) =>
            _context.RiskFactorConfigurations.AnyAsync(configuration => configuration.CountyId == countyId);

        public Task<bool> ExistsByCityIdAsync(Guid cityId) =>
            _context.RiskFactorConfigurations.AnyAsync(configuration => configuration.CityId == cityId);

        public async Task<IReadOnlyList<RiskFactorConfiguration>> GetApplicableRiskFactorConfigurationsAsync(
            Guid countryId,
            Guid countyId,
            Guid cityId,
            BuildingType buildingType) =>
            await _context.RiskFactorConfigurations
                .AsNoTracking()
                .Where(configuration => configuration.IsActive &&
                    ((configuration.Level == RiskFactorLevel.Country && configuration.CountryId == countryId) ||
                     (configuration.Level == RiskFactorLevel.County && configuration.CountyId == countyId) ||
                     (configuration.Level == RiskFactorLevel.City && configuration.CityId == cityId) ||
                     (configuration.Level == RiskFactorLevel.BuildingType && configuration.BuildingType == buildingType)))
                .OrderBy(configuration => configuration.Level)
                .ThenBy(configuration => configuration.Id)
                .ToListAsync();
    }
}
