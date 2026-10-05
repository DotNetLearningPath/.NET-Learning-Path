namespace InsuranceApp.Domain.RiskFactors;

public static class RiskFactorRules
{
    public const int AdjustmentPrecision = 7;
    public const int AdjustmentScale = 4;
    public const decimal MinAdjustment = -100m;
    public const decimal MaxAdjustment = 100m;

    public static bool IsValidAdjustment(decimal percentage)
    {
        return percentage >= MinAdjustment
            && percentage <= MaxAdjustment
            && decimal.Round(percentage, AdjustmentScale) == percentage;
    }
}
