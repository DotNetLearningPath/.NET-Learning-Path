using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;

namespace Insurance.Application.Services
{
    public class GeographyService : IGeographyService
    {
        private readonly IGeographyRepository _geographyRepository;

        public GeographyService(IGeographyRepository geographyRepository)
        {
            _geographyRepository = geographyRepository;
        }

        public async Task<IReadOnlyList<Country>> GetCountriesAsync(CancellationToken cancellationToken)
        {
            return await _geographyRepository.GetCountriesAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<County>> GetCountiesByCountryIdAsync(Guid countryId, CancellationToken cancellationToken)
        {
            return await _geographyRepository.GetCountiesByCountryIdAsync(countryId, cancellationToken);
        }

        public async Task<IReadOnlyList<City>> GetCitiesByCountyIdAsync(Guid countyId, CancellationToken cancellationToken)
        {
            return await _geographyRepository.GetCitiesByCountyIdAsync(countyId, cancellationToken);
        }
    }
}
