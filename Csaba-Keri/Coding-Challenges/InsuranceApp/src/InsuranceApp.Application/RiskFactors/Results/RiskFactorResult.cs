using InsuranceApp.Domain.Buildings;
using InsuranceApp.Domain.RiskFactors;

namespace InsuranceApp.Application.RiskFactors.Results;

public record RiskFactorResult(
    Guid Id,
    RiskFactorLevel Level,
    Guid? CountryId,
    Guid? CountyId,
    Guid? CityId,
    BuildingType? BuildingType,
    decimal AdjustmentPercentage,
    bool IsActive
);
