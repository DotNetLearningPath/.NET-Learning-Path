using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

internal sealed class GeographyRepository(InsuranceDbContext dbContext) : IGeographyRepository
{
    public async Task<IReadOnlyList<Country>> GetCountriesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Countries
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<County>> GetCountiesByCountryAsync(Guid countryId, CancellationToken cancellationToken)
    {
        return await dbContext.Counties
            .AsNoTracking()
            .Where(x => x.CountryId == countryId)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.CountryId)
            .ThenBy(x => x.CountyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<City>> GetCitiesByCountyAsync(Guid countyId, CancellationToken cancellationToken)
    {
        return await dbContext.Cities
            .AsNoTracking()
            .Where(x => x.CountyId == countyId)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.CountyId)
            .ThenBy(x => x.CityId)
            .ToListAsync(cancellationToken);
    }

    public Task<bool> CountryExistsAsync(Guid countryId, CancellationToken cancellationToken)
    {
        return dbContext.Countries.AnyAsync(x => x.CountryId == countryId, cancellationToken);
    }

    public Task<bool> CountyExistsAsync(Guid countyId, CancellationToken cancellationToken)
    {
        return dbContext.Counties.AnyAsync(x => x.CountyId == countyId, cancellationToken);
    }

    public Task<bool> CityExistsAsync(Guid cityId, CancellationToken cancellationToken)
    {
        return dbContext.Cities.AnyAsync(x => x.CityId == cityId, cancellationToken);
    }
}
