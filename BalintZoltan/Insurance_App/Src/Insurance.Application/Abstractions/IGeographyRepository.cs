using Insurance.Domain.Entities;

namespace Insurance.Application.Abstractions;

public interface IGeographyRepository
{
    Task<bool> CountryExistsAsync(Guid countryId, CancellationToken cancellationToken);
    Task<bool> CountyExistsAsync(Guid countyId, CancellationToken cancellationToken);
    Task<bool> CityExistsAsync(Guid cityId, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Country>> GetCountriesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<County>> GetCountiesByCountryIdAsync(
        Guid countryId,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<City>> GetCitiesByCountyIdAsync(
        Guid countyId,
        CancellationToken cancellationToken);

}
