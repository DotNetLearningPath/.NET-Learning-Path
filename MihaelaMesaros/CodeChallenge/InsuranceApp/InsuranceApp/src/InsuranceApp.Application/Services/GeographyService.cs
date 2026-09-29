using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Geography;

namespace InsuranceApp.Application.Services;

public sealed class GeographyService(IGeographyRepository geographyRepository) : IGeographyService
{
    public async Task<IReadOnlyList<CountryDto>> GetCountriesAsync(CancellationToken cancellationToken)
    {
        var countries = await geographyRepository.GetCountriesAsync(cancellationToken);

        return countries.Select(x => new CountryDto(x.CountryId, x.Name)).ToList();
    }

    public async Task<Result<IReadOnlyList<CountyDto>>> GetCountiesByCountryAsync(int countryId, CancellationToken cancellationToken)
    {
        if (countryId <= 0)
        {
            return Result<IReadOnlyList<CountyDto>>.Failure(GeographyErrors.InvalidCountryId);
        }

        var countryExists = await geographyRepository.CountryExistsAsync(countryId, cancellationToken);

        if (!countryExists)
        {
            return Result<IReadOnlyList<CountyDto>>.Failure(GeographyErrors.CountryNotFound(countryId));
        }

        var counties = await geographyRepository.GetCountiesByCountryAsync(countryId, cancellationToken);

        var countyDtos = counties.Select(x => new CountyDto(x.CountyId, x.Name)) .ToList();

        return Result<IReadOnlyList<CountyDto>>.Success(countyDtos);
    }

    public async Task<Result<IReadOnlyList<CityDto>>> GetCitiesByCountyAsync(int countyId, CancellationToken cancellationToken)
    {
        if (countyId <= 0)
        {
            return Result<IReadOnlyList<CityDto>>.Failure(GeographyErrors.InvalidCountyId);
        }

        if (!await geographyRepository.CountyExistsAsync(countyId, cancellationToken))
        {
            return Result<IReadOnlyList<CityDto>>.Failure(GeographyErrors.CountyNotFound(countyId));
        }

        var cities = await geographyRepository.GetCitiesByCountyAsync(countyId, cancellationToken);

        var cityDtos = cities.Select(x => new CityDto(x.CityId, x.Name)).ToList();

        return Result<IReadOnlyList<CityDto>>.Success(cityDtos);
    }
}