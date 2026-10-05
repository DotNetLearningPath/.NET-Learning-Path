namespace InsuranceApp.Domain.RiskFactors.Targets;

public record CountyTarget : RiskTarget
{
    public Guid CountyId { get; }
    public override RiskFactorLevel Level => RiskFactorLevel.County;

    public CountyTarget(Guid countyId)
    {
        if (countyId == Guid.Empty)
        {
            throw new ArgumentException(
                "County identifier must not be empty.",
                nameof(countyId)
            );
        }

        CountyId = countyId;
    }
}
