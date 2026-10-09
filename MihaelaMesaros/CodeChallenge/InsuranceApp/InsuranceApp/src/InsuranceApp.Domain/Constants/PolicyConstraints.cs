namespace InsuranceApp.Domain.Constants;

public static class PolicyConstraints
{
    public const int PolicyNumberMaxLength = 50;
    public const decimal MinBasePremium = 0.01m;
    public const int PremiumScale = 2;
    public const int CancellationReasonMaxLength = 500;
}
