namespace InsuranceApp.Domain.RiskFactors.Targets;

public abstract record RiskTarget
{
    private protected RiskTarget()
    {
    }

    public abstract RiskFactorLevel Level { get; }
}
