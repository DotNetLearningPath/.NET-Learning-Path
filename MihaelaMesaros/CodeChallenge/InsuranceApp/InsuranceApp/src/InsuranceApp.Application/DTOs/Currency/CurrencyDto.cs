namespace InsuranceApp.Application.DTOs.Currency;

public sealed record CurrencyDto(
    Guid CurrencyId,
    string Code,
    string Name,
    decimal ExchangeRateToBase,
    bool IsActive
);