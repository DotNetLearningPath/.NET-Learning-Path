namespace InsuranceApp.Domain.Policies;

public static class PolicyRules
{
    public const int MaxPolicyNumberLength = 40;
    public const int MoneyPrecision = 14;
    public const int MoneyScale = 2;
    public const decimal MaxPremium = 999_999_999_999.99m;

    public static bool IsValidPolicyNumber(string policyNumber)
    {
        return !string.IsNullOrWhiteSpace(policyNumber)
            && policyNumber.Trim().Length <= MaxPolicyNumberLength;
    }

    public static bool IsValidPremium(decimal amount)
    {
        return amount > 0m
            && amount <= MaxPremium
            && decimal.Round(amount, MoneyScale) == amount;
    }

    public static bool IsValidPeriod(DateOnly startDate, DateOnly endDate)
    {
        return startDate != default
            && endDate >= startDate;
    }
}
