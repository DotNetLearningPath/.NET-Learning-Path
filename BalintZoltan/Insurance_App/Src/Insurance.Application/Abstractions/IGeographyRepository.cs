using Domain.Entities;

namespace Application.Abstractions;

public interface IGeographyRepository
{
    Task<bool> CountryExistsAsync(Guid countryId, CancellationToken cancellationToken = default);
    Task<bool> CountyExistsAsync(Guid countyId, CancellationToken cancellationToken = default);
    Task<bool> CityExistsAsync(Guid cityId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Country>> GetCountriesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<County>> GetCountiesByCountryIdAsync(
        Guid countryId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<City>> GetCitiesByCountyIdAsync(
        Guid countyId,
        CancellationToken cancellationToken = default);

}
