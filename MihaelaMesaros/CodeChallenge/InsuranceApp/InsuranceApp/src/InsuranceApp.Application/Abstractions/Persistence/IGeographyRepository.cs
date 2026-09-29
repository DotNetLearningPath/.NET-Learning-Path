using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Application.Abstractions.Persistence;

public interface IGeographyRepository
{
    Task<IReadOnlyList<Country>> GetCountriesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<County>> GetCountiesByCountryAsync(int countryId, CancellationToken cancellationToken);

    Task<IReadOnlyList<City>> GetCitiesByCountyAsync(int countyId, CancellationToken cancellationToken);

    Task<bool> CountryExistsAsync(int countryId, CancellationToken cancellationToken);

    Task<bool> CountyExistsAsync(int countyId, CancellationToken cancellationToken);

    Task<bool> CityExistsAsync(int cityId, CancellationToken cancellationToken);
}
