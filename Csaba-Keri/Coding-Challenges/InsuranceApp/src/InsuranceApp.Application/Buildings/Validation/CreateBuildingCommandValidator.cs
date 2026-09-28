using FluentValidation;
using InsuranceApp.Application.Buildings.Commands;

namespace InsuranceApp.Application.Buildings.Validation;

public class CreateBuildingCommandValidator : BuildingDetailsValidator<CreateBuildingCommand>
{
    public CreateBuildingCommandValidator(IValidator<BuildingAddressCommand> addressValidator)
        : base(addressValidator)
    {
        RuleFor(command => command.ClientId)
            .NotEmpty();
    }
}
