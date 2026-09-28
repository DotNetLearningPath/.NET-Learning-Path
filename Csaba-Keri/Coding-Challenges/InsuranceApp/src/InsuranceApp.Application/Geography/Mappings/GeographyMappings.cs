using InsuranceApp.Application.Geography.Results;
using InsuranceApp.Domain.Geography;

namespace InsuranceApp.Application.Geography.Mappings;

internal static class GeographyMappings
{
    public static CountryResult ToResult(this Country country)
    {
        return new(
            Id: country.Id,
            Name: country.Name
        );
    }

    public static CountyResult ToResult(this County county)
    {
        return new(
            Id: county.Id,
            Name: county.Name,
            CountryId: county.CountryId
        );
    }

    public static CityResult ToResult(this City city)
    {
        return new(
            Id: city.Id,
            Name: city.Name,
            CountyId: city.CountyId
        );
    }
}
