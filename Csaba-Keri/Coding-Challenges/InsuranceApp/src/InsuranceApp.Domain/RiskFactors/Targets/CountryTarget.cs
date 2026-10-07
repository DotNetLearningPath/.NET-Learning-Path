namespace InsuranceApp.Domain.RiskFactors.Targets;

public record CountryTarget : RiskTarget
{
    public Guid CountryId { get; }
    public override RiskFactorLevel Level => RiskFactorLevel.Country;

    public CountryTarget(Guid countryId)
    {
        if (countryId == Guid.Empty)
        {
            throw new ArgumentException(
                "Country identifier must not be empty.",
                nameof(countryId)
            );
        }

        CountryId = countryId;
    }
}
