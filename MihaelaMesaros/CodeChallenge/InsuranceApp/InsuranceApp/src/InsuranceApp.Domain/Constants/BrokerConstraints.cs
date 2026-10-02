namespace InsuranceApp.Domain.Constants;

public static class BrokerConstraints
{
    public const int BrokerCodeMinLength = 3;
    public const int BrokerCodeMaxLength = 50;

    public const int NameMinLength = 3;
    public const int NameMaxLength = 100;

    public const int EmailMaxLength = 100;
    public const int PhoneMaxLength = 20;

    public const decimal MinCommissionPercentage = 0m;
    public const decimal MaxCommissionPercentage = 100m;
    public const int CommissionPercentageScale = 2;
}

