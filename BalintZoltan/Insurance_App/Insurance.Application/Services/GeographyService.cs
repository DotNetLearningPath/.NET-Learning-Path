using Application.Abstractions;
using Application.DTO.Geography;
using Domain.Entities;

namespace Application.Services;

public class GeographyService : IGeographyService
{
    private readonly IGeographyRepository _geographyRepository;

    public GeographyService(IGeographyRepository geographyRepository)
    {
        _geographyRepository = geographyRepository;
    }

    private async Task CheckCountryExistsAsync(
        Guid countryId,
        CancellationToken cancellationToken)
    {
        if (!await _geographyRepository.CountryExistsAsync(
                countryId,
                cancellationToken))
        {
            throw new InvalidOperationException("Country was not found.");
        }
    }

    private async Task CheckCountyExistsAsync(
        Guid countyId,
        CancellationToken cancellationToken)
    {
        if (!await _geographyRepository.CountyExistsAsync(
                countyId,
                cancellationToken))
        {
            throw new InvalidOperationException("County was not found.");
        }
    }

    public async Task<IReadOnlyCollection<CountryDto>> GetCountriesAsync(CancellationToken cancellationToken = default)
    {
        var countries = await _geographyRepository.GetCountriesAsync(cancellationToken);

        return countries
            .Select(MapToCountryDto)
            .ToList();
    }

    public async Task<IReadOnlyCollection<CountyDto>> GetCountiesByCountryIdAsync(
        Guid countryId,
        CancellationToken cancellationToken = default)
    {
        await CheckCountryExistsAsync(countryId, cancellationToken);

        var counties = await _geographyRepository
            .GetCountiesByCountryIdAsync(countryId, cancellationToken);

        return counties
            .Select(MapToCountyDto)
            .ToList();
    }

    public async Task<IReadOnlyCollection<CityDto>> GetCitiesByCountyIdAsync(
        Guid countyId,
        CancellationToken cancellationToken = default)
    {
        await CheckCountyExistsAsync(countyId, cancellationToken);

        var cities = await _geographyRepository
            .GetCitiesByCountyIdAsync(countyId, cancellationToken);

        return cities
            .Select(MapToCityDto)
            .ToList();
    }

    private static CountryDto MapToCountryDto(Country country)
    {
        return new CountryDto
        {
            Id = country.Id,
            Name = country.Name
        };
    }

    private static CountyDto MapToCountyDto(County county)
    {
        return new CountyDto
        {
            Id = county.Id,
            CountryId = county.CountryId,
            Name = county.Name
        };
    }

    private static CityDto MapToCityDto(City city)
    {
        return new CityDto
        {
            Id = city.Id,
            CountyId = city.CountyId,
            Name = city.Name,
            PostalCode = city.PostalCode
        };
    }
}
