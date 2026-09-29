using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class GeographyRepository : IGeographyRepository
{
    private readonly InsuranceDbContext _dbContext;

    public GeographyRepository(InsuranceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> CountryExistsAsync(
        Guid countryId,
        CancellationToken cancellationToken = default) =>
        _dbContext.Countries.AnyAsync(
            country => country.Id == countryId,
            cancellationToken);

    public Task<bool> CountyExistsAsync(
        Guid countyId,
        CancellationToken cancellationToken = default) =>
        _dbContext.Counties.AnyAsync(
            county => county.Id == countyId,
            cancellationToken);

    public async Task<IReadOnlyCollection<Country>> GetCountriesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Countries
            .AsNoTracking()
            .OrderBy(country => country.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<County>>
        GetCountiesByCountryIdAsync(Guid countryId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Counties
            .AsNoTracking()
            .Where(county => county.CountryId == countryId)
            .OrderBy(county => county.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<City>>
        GetCitiesByCountyIdAsync(Guid countyId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Cities
            .AsNoTracking()
            .Where(city => city.CountyId == countyId)
            .OrderBy(city => city.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CityExistsAsync(Guid cityId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Cities
            .AsNoTracking()
            .AnyAsync(city => city.Id == cityId, cancellationToken);
    }
}
