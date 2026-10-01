using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Application.Abstractions.Persistence;

public interface IGeographyRepository
{
    Task<IReadOnlyList<Country>> GetCountriesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<County>> GetCountiesByCountryAsync(Guid countryId, CancellationToken cancellationToken);

    Task<IReadOnlyList<City>> GetCitiesByCountyAsync(Guid countyId, CancellationToken cancellationToken);

    Task<bool> CountryExistsAsync(Guid countryId, CancellationToken cancellationToken);

    Task<bool> CountyExistsAsync(Guid countyId, CancellationToken cancellationToken);

    Task<bool> CityExistsAsync(Guid cityId, CancellationToken cancellationToken);
}
