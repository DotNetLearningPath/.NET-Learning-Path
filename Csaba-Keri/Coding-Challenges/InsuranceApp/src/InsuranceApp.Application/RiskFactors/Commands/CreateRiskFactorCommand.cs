using InsuranceApp.Domain.Buildings;
using InsuranceApp.Domain.RiskFactors;

namespace InsuranceApp.Application.RiskFactors.Commands;

public record CreateRiskFactorCommand(
    RiskFactorLevel Level,
    Guid? CountryId,
    Guid? CountyId,
    Guid? CityId,
    BuildingType? BuildingType,
    decimal AdjustmentPercentage,
    bool IsActive
) : IRiskFactorDetailsCommand;
