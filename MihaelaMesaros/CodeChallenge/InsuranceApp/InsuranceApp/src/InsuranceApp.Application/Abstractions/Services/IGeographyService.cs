using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Geography;

namespace InsuranceApp.Application.Abstractions.Services;

public interface IGeographyService
{
    Task<IReadOnlyList<CountryDto>> GetCountriesAsync(CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<CountyDto>>> GetCountiesByCountryAsync(Guid countryId, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<CityDto>>> GetCitiesByCountyAsync(Guid countyId, CancellationToken cancellationToken);
}