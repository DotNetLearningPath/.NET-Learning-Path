namespace InsuranceApp.Domain.Fees;

public static class FeeRules
{
    public const int MaxNameLength = 200;
    public const int PercentagePrecision = 7;
    public const int PercentageScale = 4;
    public const decimal MinPercentage = 0m;
    public const decimal MaxPercentage = 100m;

    public static bool IsValidName(string? name)
    {
        return !string.IsNullOrWhiteSpace(name)
            && name.Trim().Length <= MaxNameLength;
    }

    public static bool IsValidPercentage(decimal percentage)
    {
        return percentage >= MinPercentage
            && percentage <= MaxPercentage
            && decimal.Round(percentage, PercentageScale) == percentage;
    }

    public static bool IsValidPeriod(DateOnly effectiveFrom, DateOnly? effectiveTo)
    {
        return effectiveFrom != default
            && (!effectiveTo.HasValue || effectiveTo.Value >= effectiveFrom);
    }
}
