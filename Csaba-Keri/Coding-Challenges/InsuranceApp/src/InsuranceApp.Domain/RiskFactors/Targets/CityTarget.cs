namespace InsuranceApp.Domain.RiskFactors.Targets;

public record CityTarget : RiskTarget
{
    public Guid CityId { get; }
    public override RiskFactorLevel Level => RiskFactorLevel.City;

    public CityTarget(Guid cityId)
    {
        if (cityId == Guid.Empty)
        {
            throw new ArgumentException(
                "City identifier must not be empty.",
                nameof(cityId)
            );
        }

        CityId = cityId;
    }
}
