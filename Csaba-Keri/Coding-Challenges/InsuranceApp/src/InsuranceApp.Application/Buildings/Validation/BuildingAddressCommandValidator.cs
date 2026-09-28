using FluentValidation;
using InsuranceApp.Application.Buildings.Commands;
using InsuranceApp.Domain.Buildings;

namespace InsuranceApp.Application.Buildings.Validation;

public class BuildingAddressCommandValidator : AbstractValidator<BuildingAddressCommand>
{
    public BuildingAddressCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Continue;

        RuleFor(address => address.CityId)
            .NotEmpty();

        RuleFor(address => address.Street)
            .NotEmpty()
            .Must(value => value!.Trim().Length <= BuildingAddress.MaxStreetLength)
            .WithMessage($"Street must not exceed {BuildingAddress.MaxStreetLength} characters.");

        RuleFor(address => address.Number)
            .NotEmpty()
            .Must(value => value!.Trim().Length <= BuildingAddress.MaxNumberLength)
            .WithMessage($"Street number must not exceed {BuildingAddress.MaxNumberLength} characters.");
    }
}
