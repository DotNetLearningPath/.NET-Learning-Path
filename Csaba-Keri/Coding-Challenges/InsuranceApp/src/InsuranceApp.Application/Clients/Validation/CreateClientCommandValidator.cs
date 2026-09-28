using FluentValidation;
using InsuranceApp.Application.Clients.Commands;
using InsuranceApp.Domain.Clients;

namespace InsuranceApp.Application.Clients.Validation;

public class CreateClientCommandValidator : ClientDetailsValidator<CreateClientCommand>
{
    public CreateClientCommandValidator()
    {
        RuleFor(command => command.Type)
            .IsInEnum().WithMessage("Client type is invalid.");

        RuleFor(command => command.IdentificationNumber)
            .NotEmpty().WithMessage("Identification number is required.")
            .Must(value => value!.Trim().Length <= Client.MaxIdentificationNumberLength)
            .WithMessage($"Identification number must not exceed {Client.MaxIdentificationNumberLength} characters.");
    }
}
