using FluentValidation;
using InsuranceApp.Application.RiskFactors.Commands;
using InsuranceApp.Domain.Buildings;
using InsuranceApp.Domain.RiskFactors;

namespace InsuranceApp.Application.RiskFactors.Validation;

public abstract class RiskFactorDetailsValidator<TCommand> : AbstractValidator<TCommand>
    where TCommand : class, IRiskFactorDetailsCommand
{
    protected RiskFactorDetailsValidator()
    {
        RuleFor(command => command.Level)
            .IsInEnum()
            .WithMessage("Risk factor level is invalid.");

        RuleFor(command => command)
            .Must(command => IsValidTarget(
                level: command.Level,
                countryId: command.CountryId,
                countyId: command.CountyId,
                cityId: command.CityId,
                buildingType: command.BuildingType
            ))
            .OverridePropertyName("Target")
            .WithMessage("Specify exactly the target matching the selected level.");

        RuleFor(command => command.AdjustmentPercentage)
            .Must(RiskFactorRules.IsValidAdjustment)
            .WithMessage(
                $"Adjustment must be between {RiskFactorRules.MinAdjustment} and {RiskFactorRules.MaxAdjustment}" +
                $", with at most {RiskFactorRules.AdjustmentScale} decimal places."
            );
    }

    private static bool IsValidTarget(
        RiskFactorLevel level,
        Guid? countryId,
        Guid? countyId,
        Guid? cityId,
        BuildingType? buildingType
    )
    {
        return level switch
        {
            RiskFactorLevel.Country => countryId.HasValue && countryId != Guid.Empty
                && countyId is null && cityId is null && buildingType is null,
            
            RiskFactorLevel.County => countyId.HasValue && countyId != Guid.Empty
                && countryId is null && cityId is null && buildingType is null,
            
            RiskFactorLevel.City => cityId.HasValue && cityId != Guid.Empty
                && countryId is null && countyId is null && buildingType is null,
            
            RiskFactorLevel.BuildingType => buildingType.HasValue && Enum.IsDefined(buildingType.Value)
                && countryId is null && countyId is null && cityId is null,
            
            _ => false
        };
    }
}
