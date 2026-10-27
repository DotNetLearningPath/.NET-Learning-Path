using Insurance.Domain.Entities;

namespace Insurance.Application.Interfaces
{
    public interface IGeographyService
    {
        Task<IReadOnlyList<Country>> GetCountriesAsync(CancellationToken cancellationToken);
        Task<IReadOnlyList<County>> GetCountiesByCountryIdAsync(Guid countryId, CancellationToken cancellationToken);
        Task<IReadOnlyList<City>> GetCitiesByCountyIdAsync(Guid countyId, CancellationToken cancellationToken);
    }
}
