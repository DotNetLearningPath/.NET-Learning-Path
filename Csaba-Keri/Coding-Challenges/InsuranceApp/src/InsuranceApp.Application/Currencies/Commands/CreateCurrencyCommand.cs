namespace InsuranceApp.Application.Currencies.Commands;

public record CreateCurrencyCommand(
    string Code,
    string Name,
    decimal ExchangeRateToBase,
    bool IsActive
) : ICurrencyDetailsCommand;
