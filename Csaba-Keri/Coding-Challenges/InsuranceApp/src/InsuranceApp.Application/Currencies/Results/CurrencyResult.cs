namespace InsuranceApp.Application.Currencies.Results;

public record CurrencyResult(
    Guid Id,
    string Code,
    string Name,
    decimal ExchangeRateToBase,
    bool IsActive
);
