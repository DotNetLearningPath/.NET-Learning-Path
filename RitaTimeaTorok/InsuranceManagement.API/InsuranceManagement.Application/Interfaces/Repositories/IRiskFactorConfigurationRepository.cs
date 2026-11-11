using InsuranceManagement.Domain.Entities.Metadata;

namespace InsuranceManagement.Application.Interfaces.Repositories
{
    public interface IRiskFactorConfigurationRepository : IGenericRepository<RiskFactorConfiguration>
    {
        Task<bool> ExistsByCountryIdAsync(Guid countryId);
        Task<bool> ExistsByCountyIdAsync(Guid countyId);
        Task<bool> ExistsByCityIdAsync(Guid cityId);
    }
}
