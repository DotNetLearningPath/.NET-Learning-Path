namespace InsuranceApp.Application.Geography.Results;

public record CityGeographyResult(CityResult City, CountyResult County, CountryResult Country)
{
    public CityResult City { get; } = City;
    public CountyResult County { get; } = County;
    public CountryResult Country { get; } = Country;
}
