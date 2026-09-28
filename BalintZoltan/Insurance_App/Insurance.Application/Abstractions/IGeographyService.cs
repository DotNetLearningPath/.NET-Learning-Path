using Application.DTO.Geography;

namespace Application.Abstractions;

public interface IGeographyService
{
    Task<IReadOnlyCollection<CountryDto>> GetCountriesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CountyDto>> GetCountiesByCountryIdAsync(
        Guid countryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<CityDto>> GetCitiesByCountyIdAsync(
        Guid countyId,
        CancellationToken cancellationToken = default);
}
