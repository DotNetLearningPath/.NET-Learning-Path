using FluentValidation;
using InsuranceApp.Application.RiskFactors.Commands;

namespace InsuranceApp.Application.RiskFactors.Validation;

public class UpdateRiskFactorCommandValidator : RiskFactorDetailsValidator<UpdateRiskFactorCommand>
{
    public UpdateRiskFactorCommandValidator()
    {
        RuleFor(command => command.RiskFactorId)
            .NotEmpty()
            .WithMessage("Risk factor identifier must not be empty.");
    }
}
