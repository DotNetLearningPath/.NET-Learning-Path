using Insurance.Application.DTO.Geography;

namespace Insurance.Application.Abstractions;

public interface IGeographyService
{
    Task<IReadOnlyCollection<CountryDto>> GetCountriesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CountyDto>> GetCountiesByCountryIdAsync(
        Guid countryId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<CityDto>> GetCitiesByCountyIdAsync(
        Guid countyId,
        CancellationToken cancellationToken);
}
