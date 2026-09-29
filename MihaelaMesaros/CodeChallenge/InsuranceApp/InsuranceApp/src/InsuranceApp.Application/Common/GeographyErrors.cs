namespace InsuranceApp.Application.Common;

public static class GeographyErrors
{
    public static readonly Error InvalidCountryId = new(
        "Geography.InvalidCountryId",
        "Country ID must be greater than zero.",
        ErrorType.Validation);

    public static Error CountryNotFound(int countryId) => new(
        "Geography.CountryNotFound",
        $"Country with ID {countryId} was not found.",
        ErrorType.NotFound);

    public static readonly Error InvalidCountyId = new(
        "Geography.InvalidCountyId",
        "County ID must be greater than zero.",
        ErrorType.Validation);

    public static Error CountyNotFound(int countyId) => new(
        "Geography.CountyNotFound",
        $"County with ID {countyId} was not found.",
        ErrorType.NotFound);
}