namespace InsuranceApp.Domain.Constants;

public static class CurrencyConstraints
{
    public const int CodeMinLength = 3;
    public const int CodeMaxLength = 3;

    public const int NameMinLength = 2;
    public const int NameMaxLength = 100;

    public const decimal MinExchangeRate = 0.0001m;
    public const decimal MaxExchangeRate = 999_999.9999m;

    public const int ExchangeRatePrecision = 18;
    public const int ExchangeRateScale = 4;
}