using FluentValidation;
using InsuranceApp.Application.Clients.Commands;
using InsuranceApp.Domain.Clients;
using InsuranceApp.Domain.Common.Validation;

namespace InsuranceApp.Application.Clients.Validation;

public abstract class ClientDetailsValidator<TCommand> : AbstractValidator<TCommand>
    where TCommand : class, IClientDetailsCommand
{
    protected ClientDetailsValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Continue;

        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Client name is required.")
            .Must(value => value!.Trim().Length <= Client.MaxNameLength)
            .WithMessage($"Client name must not exceed {Client.MaxNameLength} characters.");

        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("Email is required.")
            .Must(value => value!.Trim().Length <= Client.MaxEmailLength)
            .WithMessage($"Email must not exceed {Client.MaxEmailLength} characters.")
            .Must(value => EmailAddressRules.IsSingleAddress(value!.Trim()))
            .WithMessage("A single email address without a display name is required.");

        RuleFor(command => command.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .Must(value => value!.Trim().Length <= Client.MaxPhoneLength)
            .WithMessage($"Phone must not exceed {Client.MaxPhoneLength} characters.");

        RuleFor(command => command.PrimaryAddress)
            .Must(value => string.IsNullOrWhiteSpace(value) || value.Trim().Length <= Client.MaxPrimaryAddressLength)
            .WithMessage($"Primary address must not exceed {Client.MaxPrimaryAddressLength} characters.");
    }
}
