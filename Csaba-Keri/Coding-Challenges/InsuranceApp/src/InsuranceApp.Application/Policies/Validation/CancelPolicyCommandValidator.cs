using FluentValidation;
using InsuranceApp.Application.Policies.Commands;
using InsuranceApp.Domain.Policies;

namespace InsuranceApp.Application.Policies.Validation;

public class CancelPolicyCommandValidator : AbstractValidator<CancelPolicyCommand>
{
    public CancelPolicyCommandValidator()
    {
        RuleFor(command => command.PolicyId)
            .NotEmpty()
            .WithMessage("Policy identifier must not be empty.");

        RuleFor(command => command.Reason)
            .Must(PolicyRules.IsValidCancellationReason)
            .WithMessage($"Cancellation reason is required and must not exceed {PolicyRules.MaxCancellationReasonLength} characters.");

        RuleFor(command => command.EffectiveDate)
            .NotEmpty()
            .WithMessage("Cancellation effective date is required.");
    }
}
