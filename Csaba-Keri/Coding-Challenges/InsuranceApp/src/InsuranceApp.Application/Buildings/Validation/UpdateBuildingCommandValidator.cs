using FluentValidation;
using InsuranceApp.Application.Buildings.Commands;

namespace InsuranceApp.Application.Buildings.Validation;

public class UpdateBuildingCommandValidator : BuildingDetailsValidator<UpdateBuildingCommand>
{
    public UpdateBuildingCommandValidator(IValidator<BuildingAddressCommand> addressValidator)
        : base(addressValidator)
    {
        RuleFor(command => command.BuildingId)
            .NotEmpty();
    }
}
