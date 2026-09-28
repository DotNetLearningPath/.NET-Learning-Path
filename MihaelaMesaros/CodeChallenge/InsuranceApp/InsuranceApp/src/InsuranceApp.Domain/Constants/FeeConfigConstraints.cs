namespace InsuranceApp.Domain.Constants;

public static class FeeConfigConstraints
{
    public const int NameMinLength = 3;
    public const int NameMaxLength = 200;

    public const decimal MinPercentage = 0m;
    public const decimal MaxPercentage = 100m;

    public const int PercentagePrecision = 5;
    public const int PercentageScale = 2;
}