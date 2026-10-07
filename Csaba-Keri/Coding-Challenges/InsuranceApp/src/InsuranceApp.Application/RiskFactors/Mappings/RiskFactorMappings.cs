using InsuranceApp.Application.RiskFactors.Commands;
using InsuranceApp.Application.RiskFactors.Results;
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
        return new(
            Id: riskFactor.Id,
            Target: riskFactor.Target,
            AdjustmentPercentage: riskFactor.AdjustmentPercentage,
            IsActive: riskFactor.IsActive
        );
    }
}
