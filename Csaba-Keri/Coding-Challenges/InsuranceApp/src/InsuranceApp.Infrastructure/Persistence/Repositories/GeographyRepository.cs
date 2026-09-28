using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Geography;
using InsuranceApp.Application.Geography.Results;
using InsuranceApp.Domain.Geography;
using InsuranceApp.Infrastructure.Persistence.Mappings;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

public class GeographyRepository : IGeographyRepository
{
    private readonly InsuranceDbContext _context;

    public GeographyRepository(InsuranceDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public Task<bool> CountryExistsAsync(Guid countryId, CancellationToken cancellationToken)
    {
        return _context.Countries.AnyAsync(
            country => country.Id == countryId,
            cancellationToken
        );
    }

    public Task<bool> CountyExistsAsync(Guid countyId, CancellationToken cancellationToken)
    {
        return _context.Counties.AnyAsync(
            county => county.Id == countyId,
            cancellationToken
        );
    }

    public Task<PagedResult<Country>> GetCountriesAsync(PageQuery query, CancellationToken cancellationToken)
    {
        return _context.Countries
            .AsNoTracking()
            .OrderBy(country => country.Name)
            .ThenBy(country => country.Id)
            .ToDomainPageAsync(query.PageNumber, query.PageSize, entity => entity.ToDomain(), cancellationToken);
    }

    public Task<PagedResult<County>> GetCountiesByCountryIdAsync(Guid countryId, PageQuery query, CancellationToken cancellationToken)
    {
        return _context.Counties
            .AsNoTracking()
            .Where(county => county.CountryId == countryId)
            .OrderBy(county => county.Name)
            .ThenBy(county => county.Id)
            .ToDomainPageAsync(query.PageNumber, query.PageSize, entity => entity.ToDomain(), cancellationToken);
    }

    public Task<PagedResult<City>> GetCitiesByCountyIdAsync(Guid countyId, PageQuery query, CancellationToken cancellationToken)
    {
        return _context.Cities
            .AsNoTracking()
            .Where(city => city.CountyId == countyId)
            .OrderBy(city => city.Name)
            .ThenBy(city => city.Id)
            .ToDomainPageAsync(query.PageNumber, query.PageSize, entity => entity.ToDomain(), cancellationToken);
    }

    public Task<CityGeographyResult?> GetCityGeographyAsync(Guid cityId, CancellationToken cancellationToken)
    {
        var query = (from city in _context.Cities
                     join county in _context.Counties on city.CountyId equals county.Id
                     join country in _context.Countries on county.CountryId equals country.Id
                     where city.Id == cityId
                     select new CityGeographyResult(
                         City: new(city.Id, city.Name, county.Id),
                         County: new(county.Id, county.Name, country.Id),
                         Country: new(country.Id, country.Name))
                     ).AsNoTracking();

        return query.SingleOrDefaultAsync(cancellationToken);
    }
}
