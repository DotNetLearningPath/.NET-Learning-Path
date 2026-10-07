namespace InsuranceApp.Domain.Constants;

public static class PolicyConstraints
{
    public const int MaxPolicyNumberGenerationAttempts = 3;
    public const decimal MinBasePremium = 0.01m;
    public const int PremiumScale = 2;
}
