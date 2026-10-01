using FluentValidation;
using InsuranceApp.Application.Currencies.Commands;
using InsuranceApp.Domain.Currencies;

namespace InsuranceApp.Application.Currencies.Validation;

public class CreateCurrencyCommandValidator : CurrencyDetailsValidator<CreateCurrencyCommand>
{
    public CreateCurrencyCommandValidator()
    {
        RuleFor(command => command.Code)
            .Must(CurrencyRules.IsValidCode)
            .WithMessage($"Currency code must contain exactly {CurrencyRules.CodeLength} ASCII letters.");
        
        RuleFor(command => command.ExchangeRateToBase)
            .Must((command, rate) => CurrencyRules.IsBaseCurrencyRateValid(command.Code, rate))
            .WithMessage($"The {CurrencyRules.BaseCurrencyCode} exchange rate must be 1.");
    }
}
