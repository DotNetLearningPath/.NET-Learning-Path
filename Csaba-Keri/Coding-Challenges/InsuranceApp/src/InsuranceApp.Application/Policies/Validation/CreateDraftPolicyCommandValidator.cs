using FluentValidation;
using InsuranceApp.Application.Policies.Commands;
using InsuranceApp.Domain.Policies;

namespace InsuranceApp.Application.Policies.Validation;

public class CreateDraftPolicyCommandValidator : AbstractValidator<CreateDraftPolicyCommand>
{
    public CreateDraftPolicyCommandValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Continue;

        RuleFor(command => command.ClientId)
            .NotEmpty()
            .WithMessage("Client identifier must not be empty.");

        RuleFor(command => command.BuildingId)
            .NotEmpty()
            .WithMessage("Building identifier must not be empty.");

        RuleFor(command => command.BrokerId)
            .NotEmpty()
            .WithMessage("Broker identifier must not be empty.");

        RuleFor(command => command.CurrencyId)
            .NotEmpty()
            .WithMessage("Currency identifier must not be empty.");

        RuleFor(command => command.StartDate)
            .NotEmpty()
            .WithMessage("Start date is required.");

        RuleFor(command => command.EndDate)
            .NotEmpty()
            .WithMessage("End date is required.")
            .GreaterThanOrEqualTo(command => command.StartDate)
            .WithMessage("End date must be on or after the start date.");

        RuleFor(command => command.BasePremium)
            .Must(PolicyRules.IsValidBasePremium)
            .WithMessage(
                $"Base premium must be positive, at most {PolicyRules.MaxPremium}" +
                $", and have at most {PolicyRules.MoneyScale} decimal places."
            );
    }
}
