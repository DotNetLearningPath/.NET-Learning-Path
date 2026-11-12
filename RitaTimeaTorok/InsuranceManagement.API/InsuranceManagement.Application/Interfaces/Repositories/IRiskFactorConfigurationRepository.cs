using InsuranceManagement.Domain.Entities.Metadata;
using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.Application.Interfaces.Repositories
{
    public interface IRiskFactorConfigurationRepository : IGenericRepository<RiskFactorConfiguration>
    {
        Task<bool> ExistsByCountryIdAsync(Guid countryId);
        Task<bool> ExistsByCountyIdAsync(Guid countyId);
        Task<bool> ExistsByCityIdAsync(Guid cityId);
        Task<IReadOnlyList<RiskFactorConfiguration>> GetApplicableRiskFactorConfigurationsAsync(
            Guid countryId,
            Guid countyId,
            Guid cityId,
            BuildingType buildingType);
    }
}
