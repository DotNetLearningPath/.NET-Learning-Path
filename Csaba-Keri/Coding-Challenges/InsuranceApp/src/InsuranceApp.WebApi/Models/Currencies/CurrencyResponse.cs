namespace InsuranceApp.WebApi.Models.Currencies;

public record CurrencyResponse(
    Guid Id,
    string Code,
    string Name,
    decimal ExchangeRateToBase,
    bool IsActive
);
