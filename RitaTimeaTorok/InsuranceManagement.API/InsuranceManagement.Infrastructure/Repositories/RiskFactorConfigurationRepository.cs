using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Domain.Entities.Metadata;
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
    }
}
