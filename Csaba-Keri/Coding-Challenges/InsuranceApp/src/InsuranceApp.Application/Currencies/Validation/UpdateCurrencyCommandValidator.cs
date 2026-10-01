using FluentValidation;
using InsuranceApp.Application.Currencies.Commands;

namespace InsuranceApp.Application.Currencies.Validation;

public class UpdateCurrencyCommandValidator : CurrencyDetailsValidator<UpdateCurrencyCommand>
{
    public UpdateCurrencyCommandValidator()
    {
        RuleFor(command => command.CurrencyId)
            .NotEmpty()
            .WithMessage("Currency identifier must not be empty.");
    }
}
