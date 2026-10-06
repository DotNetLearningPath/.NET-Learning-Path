using Insurance.Application.Abstractions;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories;

public sealed class GeographyRepository(
    InsuranceDbContext dbContext) : IGeographyRepository
{
    public Task<bool> CountryExistsAsync(
        Guid countryId,
        CancellationToken cancellationToken) =>
        dbContext.Countries.AnyAsync(
            country => country.Id == countryId,
            cancellationToken);

    public Task<bool> CountyExistsAsync(
        Guid countyId,
        CancellationToken cancellationToken) =>
        dbContext.Counties.AnyAsync(
            county => county.Id == countyId,
            cancellationToken);

    public async Task<List<Country>> GetCountriesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Countries
            .AsNoTracking()
            .OrderBy(country => country.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<County>> GetCountiesByCountryIdAsync(Guid countryId, CancellationToken cancellationToken)
    {
        return await dbContext.Counties
            .AsNoTracking()
            .Where(county => county.CountryId == countryId)
            .OrderBy(county => county.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<City>> GetCitiesByCountyIdAsync(Guid countyId, CancellationToken cancellationToken)
    {
        return await dbContext.Cities
            .AsNoTracking()
            .Where(city => city.CountyId == countyId)
            .OrderBy(city => city.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CityExistsAsync(Guid cityId, CancellationToken cancellationToken)
    {
        return await dbContext.Cities
            .AsNoTracking()
            .AnyAsync(city => city.Id == cityId, cancellationToken);
    }
}
