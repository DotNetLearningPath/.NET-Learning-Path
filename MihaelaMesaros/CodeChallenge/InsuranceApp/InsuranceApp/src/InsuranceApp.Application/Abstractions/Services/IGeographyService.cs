using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Geography;

namespace InsuranceApp.Application.Abstractions.Services;

public interface IGeographyService
{
    Task<IReadOnlyList<CountryDto>> GetCountriesAsync(CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<CountyDto>>> GetCountiesByCountryAsync(int countryId, CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<CityDto>>> GetCitiesByCountyAsync(int countyId, CancellationToken cancellationToken);
}