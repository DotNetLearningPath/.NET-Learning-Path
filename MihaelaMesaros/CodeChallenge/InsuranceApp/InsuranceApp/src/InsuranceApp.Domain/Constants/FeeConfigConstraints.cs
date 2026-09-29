namespace InsuranceApp.Domain.Constants;

public static class FeeConfigConstraints
{
    public const int NameMinLength = 3;
    public const int NameMaxLength = 200;

    public const decimal MinPercentage = 0m;
    public const decimal MaxPercentage = 100m;

    public const int PercentagePrecision = 7;
    public const int PercentageScale = 4;
}