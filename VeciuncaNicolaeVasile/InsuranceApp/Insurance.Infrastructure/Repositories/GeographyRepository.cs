using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories
{
    public class GeographyRepository : IGeographyRepository 
    {
        private readonly InsuranceDbContext _context;

        public GeographyRepository(InsuranceDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Country>> GetCountriesAsync(CancellationToken cancellationToken)
        {
            return await _context.Countries
                .AsNoTracking()
                .OrderBy(country => country.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<County>> GetCountiesByCountryIdAsync(
            Guid countryId,
            CancellationToken cancellationToken)
        {
            return await _context.Counties
                .AsNoTracking()
                .Where(county => county.CountryId == countryId)
                .OrderBy(county => county.Name)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<City>> GetCitiesByCountyIdAsync(Guid countyId, CancellationToken cancellationToken)
        {
            return await _context.Cities
                .AsNoTracking()
                .Where(city => city.CountyId == countyId)
                .OrderBy(city => city.Name)
                .ToListAsync(cancellationToken);
        }
    }
}
