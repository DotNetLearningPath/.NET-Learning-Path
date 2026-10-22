using InsuranceApp.Domain.Buildings;
using InsuranceApp.Domain.RiskFactors;

namespace InsuranceApp.Application.RiskFactors.Commands;

public interface IRiskFactorDetailsCommand
{
    RiskFactorLevel Level { get; }
    Guid? CountryId { get; }
    Guid? CountyId { get; }
    Guid? CityId { get; }
    BuildingType? BuildingType { get; }
    decimal AdjustmentPercentage { get; }
    bool IsActive { get; }
}
