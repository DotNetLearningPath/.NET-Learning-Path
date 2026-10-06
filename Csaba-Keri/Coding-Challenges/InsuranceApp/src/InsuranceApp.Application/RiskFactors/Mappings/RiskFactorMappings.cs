using InsuranceApp.Application.RiskFactors.Commands;
using InsuranceApp.Application.RiskFactors.Results;
using InsuranceApp.Domain.Buildings;
using InsuranceApp.Domain.RiskFactors;
using InsuranceApp.Domain.RiskFactors.Targets;

namespace InsuranceApp.Application.RiskFactors.Mappings;

internal static class RiskFactorMappings
{
    public static RiskTarget ToTarget(this IRiskFactorDetailsCommand command)
    {
        return command.Level switch
        {
            RiskFactorLevel.Country => new CountryTarget(command.CountryId!.Value),
            RiskFactorLevel.County => new CountyTarget(command.CountyId!.Value),
            RiskFactorLevel.City => new CityTarget(command.CityId!.Value),
            RiskFactorLevel.BuildingType => new BuildingTypeTarget(command.BuildingType!.Value),

            _ => throw new ArgumentOutOfRangeException(nameof(command), command.Level, "Unknown risk factor level.")
        };
    }

    public static RiskFactorResult ToResult(this RiskFactorConfiguration riskFactor)
    {
        return riskFactor.Target switch
        {
            CountryTarget target => CreateResult(riskFactor, countryId: target.CountryId),
            CountyTarget target => CreateResult(riskFactor, countyId: target.CountyId),
            CityTarget target => CreateResult(riskFactor, cityId: target.CityId),
            BuildingTypeTarget target => CreateResult(riskFactor, buildingType: target.Type),
            
            _ => throw new InvalidOperationException("Unsupported risk factor target.")
        };
    }

    private static RiskFactorResult CreateResult(
        RiskFactorConfiguration riskFactor,
        Guid? countryId = null,
        Guid? countyId = null,
        Guid? cityId = null,
        BuildingType? buildingType = null
    )
    {
        return new(
            Id: riskFactor.Id,
            Level: riskFactor.Target.Level,
            CountryId: countryId,
            CountyId: countyId,
            CityId: cityId,
            BuildingType: buildingType,
            AdjustmentPercentage: riskFactor.AdjustmentPercentage,
            IsActive: riskFactor.IsActive
        );
    }
}
