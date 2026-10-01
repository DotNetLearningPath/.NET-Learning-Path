using InsuranceApp.Application.Currencies.Results;
using InsuranceApp.Domain.Currencies;

namespace InsuranceApp.Application.Currencies.Mappings;

internal static class CurrencyMappings
{
    public static CurrencyResult ToResult(this Currency currency)
    {
        return new(
            Id: currency.Id,
            Code: currency.Code,
            Name: currency.Name,
            ExchangeRateToBase: currency.ExchangeRateToBase,
            IsActive: currency.IsActive
        );
    }
}
