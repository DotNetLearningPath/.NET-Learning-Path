using FluentValidation;
using InsuranceApp.Application.Brokers.Commands;
using InsuranceApp.Domain.Brokers;

namespace InsuranceApp.Application.Brokers.Validation;

public class CreateBrokerCommandValidator : BrokerDetailsValidator<CreateBrokerCommand>
{
    public CreateBrokerCommandValidator()
    {
        RuleFor(command => command.Code)
            .NotEmpty().WithMessage("Broker code is required.")
            .Must(value => value.Trim().Length <= Broker.MaxCodeLength)
            .WithMessage($"Broker code must not exceed {Broker.MaxCodeLength} characters.");
        
        RuleFor(command => command.Status)
            .IsInEnum().WithMessage("Broker status is invalid.");
    }
}
