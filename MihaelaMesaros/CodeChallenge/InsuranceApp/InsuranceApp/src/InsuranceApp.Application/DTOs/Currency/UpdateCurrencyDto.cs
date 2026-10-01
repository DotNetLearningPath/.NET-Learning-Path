namespace InsuranceApp.Application.DTOs.Currency;

public sealed record UpdateCurrencyDto(
    string Code,
    string Name,
    decimal ExchangeRateToBase,
    bool IsActive
);