using FluentValidation;
using InsuranceApp.Application.Brokers.Commands;
using InsuranceApp.Domain.Brokers;
using InsuranceApp.Domain.Common.Validation;

namespace InsuranceApp.Application.Brokers.Validation;

public abstract class BrokerDetailsValidator<TCommand> : AbstractValidator<TCommand>
    where TCommand : class, IBrokerDetailsCommand
{
    protected BrokerDetailsValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Continue;

        RuleFor(command => command.Name)
            .NotEmpty().WithMessage("Broker name is required.")
            .Must(value => value.Trim().Length <= Broker.MaxNameLength)
            .WithMessage($"Broker name must not exceed {Broker.MaxNameLength} characters.");
        
        RuleFor(command => command.Email)
            .NotEmpty().WithMessage("Email is required.")
            .Must(value => value.Trim().Length <= Broker.MaxEmailLength)
            .WithMessage($"Email must not exceed {Broker.MaxEmailLength} characters.")
            .Must(value => EmailAddressRules.IsSingleAddress(value.Trim()))
            .WithMessage("A single email address without a display name is required.");
        
        RuleFor(command => command.Phone)
            .NotEmpty().WithMessage("Phone is required.")
            .Must(value => value.Trim().Length <= Broker.MaxPhoneLength)
            .WithMessage($"Phone must not exceed {Broker.MaxPhoneLength} characters.");
    }
}
