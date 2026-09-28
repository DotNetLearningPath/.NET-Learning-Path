using FluentValidation;
using InsuranceApp.Application.Clients.Commands;

namespace InsuranceApp.Application.Clients.Validation;

public class UpdateClientCommandValidator : ClientDetailsValidator<UpdateClientCommand>
{
    public UpdateClientCommandValidator()
    {
        RuleFor(command => command.ClientId)
            .NotEmpty().WithMessage("Client identifier must not be empty.");
    }
}
