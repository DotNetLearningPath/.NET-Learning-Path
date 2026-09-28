using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Geography.Results;
using InsuranceApp.Domain.Geography;

namespace InsuranceApp.Application.Geography;

public interface IGeographyRepository
{
    Task<bool> CountryExistsAsync(Guid countryId, CancellationToken cancellationToken);

    Task<bool> CountyExistsAsync(Guid countyId, CancellationToken cancellationToken);

    Task<PagedResult<Country>> GetCountriesAsync(PageQuery query, CancellationToken cancellationToken);

    Task<PagedResult<County>> GetCountiesByCountryIdAsync(Guid countryId, PageQuery query, CancellationToken cancellationToken);

    Task<PagedResult<City>> GetCitiesByCountyIdAsync(Guid countyId, PageQuery query, CancellationToken cancellationToken);

    Task<CityGeographyResult?> GetCityGeographyAsync(Guid cityId, CancellationToken cancellationToken);

}
