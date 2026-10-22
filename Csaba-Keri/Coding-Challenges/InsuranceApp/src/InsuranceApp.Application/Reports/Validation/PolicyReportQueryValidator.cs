using FluentValidation;
using InsuranceApp.Application.Reports.Queries;

namespace InsuranceApp.Application.Reports.Validation;

public class PolicyReportQueryValidator : AbstractValidator<PolicyReportQuery>
{
    public PolicyReportQueryValidator()
    {
        RuleFor(query => query.From)
            .NotEmpty()
            .WithMessage("The policy start date (from) is required.");

        RuleFor(query => query.To)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("The policy start date (to) is required.")
            .GreaterThanOrEqualTo(query => query.From)
            .WithMessage("The policy start date (to) must be on or after the policy start date (from).");

        RuleFor(query => query.Status)
            .IsInEnum()
            .WithMessage("Report status is invalid.");

        RuleFor(query => query.CurrencyId)
            .Must(id => id is null || id != Guid.Empty)
            .WithMessage("Currency identifier must not be empty when specified.");

        RuleFor(query => query.BuildingType)
            .IsInEnum()
            .WithMessage("Building type is invalid.");
    }
}
