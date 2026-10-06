using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Geography;
using Insurance.Application.Exceptions;
using Insurance.Domain.Entities;

namespace Insurance.Application.Services;

public class GeographyService(
    IGeographyRepository geographyRepository) : IGeographyService
{
    private async Task CheckCountryExistsAsync(
        Guid countryId,
        CancellationToken cancellationToken)
    {
        if (!await geographyRepository.CountryExistsAsync(
                countryId,
                cancellationToken))
        {
            throw new NotFoundException("Country was not found.");
        }
    }

    private async Task CheckCountyExistsAsync(
        Guid countyId,
        CancellationToken cancellationToken)
    {
        if (!await geographyRepository.CountyExistsAsync(
                countyId,
                cancellationToken))
        {
            throw new NotFoundException("County was not found.");
        }
    }

    public async Task<List<CountryDto>> GetCountriesAsync(CancellationToken cancellationToken)
    {
        var countries = await geographyRepository.GetCountriesAsync(cancellationToken);

        return [.. countries.Select(MapToCountryDto)];
    }

    public async Task<List<CountyDto>> GetCountiesByCountryIdAsync(
        Guid countryId,
        CancellationToken cancellationToken)
    {
        await CheckCountryExistsAsync(countryId, cancellationToken);

        var counties = await geographyRepository
            .GetCountiesByCountryIdAsync(countryId, cancellationToken);

        return [.. counties.Select(MapToCountyDto)];
    }

    public async Task<List<CityDto>> GetCitiesByCountyIdAsync(
        Guid countyId,
        CancellationToken cancellationToken)
    {
        await CheckCountyExistsAsync(countyId, cancellationToken);

        var cities = await geographyRepository
            .GetCitiesByCountyIdAsync(countyId, cancellationToken);

        return [.. cities.Select(MapToCityDto)];
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
