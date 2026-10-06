using Insurance.Application.DTO.Geography;

namespace Insurance.Application.Abstractions;

public interface IGeographyService
{
    Task<List<CountryDto>> GetCountriesAsync(CancellationToken cancellationToken);

    Task<List<CountyDto>> GetCountiesByCountryIdAsync(
        Guid countryId,
        CancellationToken cancellationToken);

    Task<List<CityDto>> GetCitiesByCountyIdAsync(
        Guid countyId,
        CancellationToken cancellationToken);
}
