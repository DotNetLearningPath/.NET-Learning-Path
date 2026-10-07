using InsuranceApp.Domain.Buildings;
using InsuranceApp.Domain.RiskFactors;

namespace InsuranceApp.Infrastructure.Persistence.Entities;

public class RiskFactorConfigurationEntity(
    Guid id,
    RiskFactorLevel level,
    Guid? countryId,
    Guid? countyId,
    Guid? cityId,
    BuildingType? buildingType,
    decimal adjustmentPercentage,
    bool isActive
)
{
    public Guid Id { get; private set; } = id;
    public RiskFactorLevel Level { get; private set; } = level;
    public Guid? CountryId { get; private set; } = countryId;
    public Guid? CountyId { get; private set; } = countyId;
    public Guid? CityId { get; private set; } = cityId;
    public BuildingType? BuildingType { get; private set; } = buildingType;
    public decimal AdjustmentPercentage { get; private set; } = adjustmentPercentage;
    public bool IsActive { get; private set; } = isActive;
}
