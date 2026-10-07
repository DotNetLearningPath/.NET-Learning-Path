using FluentValidation;
using InsuranceApp.Application.Fees.Commands;
using InsuranceApp.Domain.Fees;

namespace InsuranceApp.Application.Fees.Validation;

public abstract class FeeDetailsValidator<TCommand> : AbstractValidator<TCommand>
    where TCommand : class, IFeeDetailsCommand
{
    protected FeeDetailsValidator()
    {
        RuleFor(command => command.Name)
            .Must(FeeRules.IsValidName)
            .WithMessage($"Fee name is required and must not exceed {FeeRules.MaxNameLength} characters.");
        
        RuleFor(command => command.Type)
            .IsInEnum()
            .WithMessage("Fee type is invalid.");
        
        RuleFor(command => command.Percentage)
            .Must(FeeRules.IsValidPercentage)
            .WithMessage(
                $"Percentage must be between {FeeRules.MinPercentage} and {FeeRules.MaxPercentage}" +
                $" with at most {FeeRules.PercentageScale} decimal places."
            );
        
        RuleFor(command => command.EffectiveFrom)
            .NotEmpty()
            .WithMessage("Start date is required.");
        
        RuleFor(command => command.EffectiveTo)
            .Must((command, effectiveTo) => FeeRules.IsValidPeriod(command.EffectiveFrom, effectiveTo))
            .When(command => command.EffectiveFrom != default)
            .WithMessage("End date must not precede the start date.");
    }
}
