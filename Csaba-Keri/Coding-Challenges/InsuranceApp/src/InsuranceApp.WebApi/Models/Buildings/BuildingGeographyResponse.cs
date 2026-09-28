namespace InsuranceApp.WebApi.Models.Buildings;

public record BuildingGeographyResponse(
    NamedLocationResponse Country,
    NamedLocationResponse County,
    NamedLocationResponse City
)
{
    public NamedLocationResponse Country { get; } = Country;

    public NamedLocationResponse County { get; } = County;

    public NamedLocationResponse City { get; } = City;
}
