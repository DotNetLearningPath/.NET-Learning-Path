namespace InsuranceApp.Domain.Currencies;

public class Currency
{
    public Guid Id { get; }
    public string Code { get; }

    public string Name { get; private set; }
    public decimal ExchangeRateToBase { get; private set; }
    public bool IsActive { get; private set; }

    public Currency(Guid id, string code, string name, decimal exchangeRateToBase, bool isActive)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Currency identifier must not be empty.",
                nameof(id)
            );
        }

        if (!CurrencyRules.IsValidCode(code))
        {
            throw new ArgumentException(
                $"Currency code must contain exactly {CurrencyRules.CodeLength} ASCII letters.",
                nameof(code)
            );
        }

        Id = id;
        Code = code.Trim().ToUpperInvariant();
        Name = string.Empty;

        UpdateDetails(name, exchangeRateToBase, isActive);
    }

    public void UpdateDetails(string name, decimal exchangeRateToBase, bool isActive)
    {
        if (!CurrencyRules.IsValidName(name))
        {
            throw new ArgumentException(
                $"Currency name is required and must not exceed {CurrencyRules.MaxNameLength} characters.",
                nameof(name)
            );
        }

        if (!CurrencyRules.IsValidExchangeRate(exchangeRateToBase))
        {
            throw new ArgumentOutOfRangeException(
                nameof(exchangeRateToBase),
                exchangeRateToBase,
                $"Exchange rate must be positive, within the supported range" +
                $" and have at most {CurrencyRules.ExchangeRateScale} decimal places."
            );
        }

        if (!CurrencyRules.IsBaseCurrencyRateValid(Code, exchangeRateToBase))
        {
            throw new ArgumentException(
                $"The {CurrencyRules.BaseCurrencyCode} exchange rate must be 1.",
                nameof(exchangeRateToBase)
            );
        }

        Name = name.Trim();
        ExchangeRateToBase = exchangeRateToBase;
        IsActive = isActive;
    }
}
