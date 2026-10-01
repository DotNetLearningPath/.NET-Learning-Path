using FluentValidation;
using InsuranceApp.Application.Brokers.Commands;

namespace InsuranceApp.Application.Brokers.Validation;

public class UpdateBrokerCommandValidator : BrokerDetailsValidator<UpdateBrokerCommand>
{
    public UpdateBrokerCommandValidator()
    {
        RuleFor(command => command.BrokerId)
            .NotEmpty().WithMessage("Broker identifier must not be empty.");
    }
}
