namespace InsuranceApp.Domain.Constants;

public static class RiskFactorConfigConstraints
{
    public const decimal MinAdjustmentPercentage = -100m;
    public const decimal MaxAdjustmentPercentage = 100m;
    public const int AdjustmentPercentagePrecision = 5;
    public const int AdjustmentPercentageScale = 2;
}
