using FluentValidation;
using InsuranceApp.Application.Currencies.Commands;
using InsuranceApp.Domain.Currencies;

namespace InsuranceApp.Application.Currencies.Validation;

public abstract class CurrencyDetailsValidator<TCommand> : AbstractValidator<TCommand>
    where TCommand : class, ICurrencyDetailsCommand
{
    protected CurrencyDetailsValidator()
    {
        RuleFor(command => command.Name)
            .Must(CurrencyRules.IsValidName)
            .WithMessage(
                "Currency name is required" +
                $" and must not exceed {CurrencyRules.MaxNameLength} characters."
            );
        
        RuleFor(command => command.ExchangeRateToBase)
            .Must(CurrencyRules.IsValidExchangeRate)
            .WithMessage(
                "Exchange rate must be positive," +
                $" no greater than {CurrencyRules.MaxExchangeRate}" +
                $" and have at most {CurrencyRules.ExchangeRateScale} decimal places."
            );
    }
}
