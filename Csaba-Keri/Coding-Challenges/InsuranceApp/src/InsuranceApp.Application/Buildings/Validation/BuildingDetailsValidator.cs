using FluentValidation;
using InsuranceApp.Application.Buildings.Commands;
using InsuranceApp.Domain.Buildings;

namespace InsuranceApp.Application.Buildings.Validation;

public abstract class BuildingDetailsValidator<TCommand> : AbstractValidator<TCommand>
    where TCommand : class, IBuildingDetailsCommand
{
    protected BuildingDetailsValidator(IValidator<BuildingAddressCommand> addressValidator)
    {
        ArgumentNullException.ThrowIfNull(addressValidator);

        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Continue;

        RuleFor(command => command.Type)
            .IsInEnum();

        RuleFor(command => command.Address!)
            .NotNull()
            .SetValidator(addressValidator);

        RuleFor(command => command.ConstructionYear)
            .InclusiveBetween(Building.MinConstructionYear, Building.MaxConstructionYear);

        RuleFor(command => command.NumberOfFloors)
            .GreaterThanOrEqualTo(1);

        RuleFor(command => command.SurfaceArea)
            .GreaterThan(0m)
            .LessThanOrEqualTo(Building.MaxSurfaceArea)
            .Must(value => decimal.Round(value, Building.DecimalPlaces) == value)
            .WithMessage($"Surface area must have at most {Building.DecimalPlaces} decimal places.");

        RuleFor(command => command.InsuredValue)
            .GreaterThan(0m)
            .LessThanOrEqualTo(Building.MaxInsuredValue)
            .Must(value => decimal.Round(value, Building.DecimalPlaces) == value)
            .WithMessage($"Insured value must have at most {Building.DecimalPlaces} decimal places.");
    }
}
