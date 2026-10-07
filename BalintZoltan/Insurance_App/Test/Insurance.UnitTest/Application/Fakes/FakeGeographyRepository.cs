using Insurance.Application.Abstractions;
using Insurance.Domain.Entities;

namespace Insurance.UnitTest.Application.Fakes;

public sealed class FakeGeographyRepository : IGeographyRepository
{
    private readonly List<Country> _countries = [];
    private readonly List<County> _counties = [];
    private readonly List<City> _cities = [];
    private readonly HashSet<Guid> _existingCityIds = [];

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
        CancellationToken cancellationToken) =>
        Task.FromResult(_countries.Any(country => country.Id == countryId));

    public Task<bool> CountyExistsAsync(
        Guid countyId,
        CancellationToken cancellationToken) =>
        Task.FromResult(_counties.Any(county => county.Id == countyId));

    public Task<IReadOnlyCollection<Country>> GetCountriesAsync(CancellationToken cancellationToken) =>
        Task.FromResult((IReadOnlyCollection<Country>)[.. _countries]);

    public Task<IReadOnlyCollection<County>> GetCountiesByCountryIdAsync(Guid countryId, CancellationToken cancellationToken) =>
        Task.FromResult((IReadOnlyCollection<County>)[.. _counties.Where(county => county.CountryId == countryId)]);

    public Task<IReadOnlyCollection<City>> GetCitiesByCountyIdAsync(Guid countyId, CancellationToken cancellationToken) =>
        Task.FromResult((IReadOnlyCollection<City>)[.. _cities.Where(city => city.CountyId == countyId)]);

    public Task<bool> CityExistsAsync(Guid cityId, CancellationToken cancellationToken) =>
        Task.FromResult(_existingCityIds.Contains(cityId));
}
