using Application.Abstractions;
using Domain.Entities;

namespace Application.Fakes;

public sealed class FakeGeographyRepository : IGeographyRepository
{
    private readonly List<Country> _countries = new();
    private readonly List<County> _counties = new();
    private readonly List<City> _cities = new();
    private readonly HashSet<Guid> _existingCityIds = new();

    public void SeedCountry(Country country) => _countries.Add(country);

    public void SeedCounty(County county) => _counties.Add(county);

    public void SeedCity(City city)
    {
        _cities.Add(city);
        _existingCityIds.Add(city.Id);
    }

    public void SeedCity(Guid cityId) => _existingCityIds.Add(cityId);

    public Task<bool> CountryExistsAsync(
        Guid countryId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_countries.Any(country => country.Id == countryId));

    public Task<bool> CountyExistsAsync(
        Guid countyId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(_counties.Any(county => county.Id == countyId));

    public Task<IReadOnlyCollection<Country>> GetCountriesAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult((IReadOnlyCollection<Country>)_countries.ToList());

    public Task<IReadOnlyCollection<County>> GetCountiesByCountryIdAsync(Guid countryId, CancellationToken cancellationToken = default) =>
        Task.FromResult((IReadOnlyCollection<County>)_counties
            .Where(county => county.CountryId == countryId)
            .ToList());

    public Task<IReadOnlyCollection<City>> GetCitiesByCountyIdAsync(Guid countyId, CancellationToken cancellationToken = default) =>
        Task.FromResult((IReadOnlyCollection<City>)_cities
            .Where(city => city.CountyId == countyId)
            .ToList());

    public Task<bool> CityExistsAsync(Guid cityId, CancellationToken cancellationToken = default) =>
        Task.FromResult(_existingCityIds.Contains(cityId));
}
