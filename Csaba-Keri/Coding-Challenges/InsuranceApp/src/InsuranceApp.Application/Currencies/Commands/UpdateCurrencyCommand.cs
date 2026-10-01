namespace InsuranceApp.Application.Currencies.Commands;

public record UpdateCurrencyCommand(
    Guid CurrencyId,
    string Name,
    decimal ExchangeRateToBase,
    bool IsActive
) : ICurrencyDetailsCommand;
