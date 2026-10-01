namespace InsuranceApp.Application.DTOs.Currency;

public sealed record CreateCurrencyDto(
    string Code,
    string Name,
    decimal ExchangeRateToBase,
    bool IsActive
);