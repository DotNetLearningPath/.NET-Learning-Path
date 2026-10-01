namespace InsuranceApp.Domain.Currencies;

public static class CurrencyRules
{
    public const string BaseCurrencyCode = "RON";
    public const int CodeLength = 3;
    public const int MaxNameLength = 100;
    public const int ExchangeRatePrecision = 18;
    public const int ExchangeRateScale = 8;
    public const decimal MaxExchangeRate = 9_999_999_999.999_999_99m;

    public static bool IsValidCode(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        var trimmed = code.Trim();

        return trimmed.Length == CodeLength
            && trimmed.All(character => character is >= 'A' and <= 'Z' or >= 'a' and <= 'z');
    }

    public static bool IsValidName(string? name)
    {
        return !string.IsNullOrWhiteSpace(name)
            && name.Trim().Length <= MaxNameLength;
    }

    public static bool IsValidExchangeRate(decimal rate)
    {
        return rate > 0
            && rate <= MaxExchangeRate
            && decimal.Round(rate, ExchangeRateScale) == rate;
    }

    public static bool IsBaseCurrencyRateValid(string? code, decimal rate)
    {
        return !string.Equals(code?.Trim(), BaseCurrencyCode, StringComparison.OrdinalIgnoreCase)
            || rate == 1m;
    }
}
