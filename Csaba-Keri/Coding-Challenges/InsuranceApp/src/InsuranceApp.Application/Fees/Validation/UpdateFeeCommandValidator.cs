using FluentValidation;
using InsuranceApp.Application.Fees.Commands;

namespace InsuranceApp.Application.Fees.Validation;

public class UpdateFeeCommandValidator : FeeDetailsValidator<UpdateFeeCommand>
{
    public UpdateFeeCommandValidator()
    {
        RuleFor(command => command.FeeId)
            .NotEmpty()
            .WithMessage("Fee identifier must not be empty.");
    }
}
