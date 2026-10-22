using Insurance.Application.Exceptions;
using Insurance.Application.Services;
using Insurance.Domain.Entities;
using Insurance.UnitTest.Application.Helper;

namespace Insurance.UnitTest.Application.Services
{
    public class GeographyServiceTest
    {
        private readonly FakeRepositories _fakeRepositories;
        private readonly GeographyService _service;
        public GeographyServiceTest()
        {
            _fakeRepositories = new FakeRepositories();
            _service = new GeographyService(_fakeRepositories.Geography);
        }


        [Fact]
        public async Task GetCountriesAsync_Should_Return_List()
        {
            // Arrange
            var c1 = new Country("CountryA");
            var c2 = new Country("CountryB");
            _fakeRepositories.Geography.SeedCountry(c1);
            _fakeRepositories.Geography.SeedCountry(c2);

            // Act
            var list = await _service.GetCountriesAsync(CancellationToken.None);

            // Assert
            Assert.Equal(2, list.Count);
            Assert.Contains(list, x => x.Id == c1.Id && x.Name == "CountryA");
            Assert.Contains(list, x => x.Id == c2.Id && x.Name == "CountryB");
        }

        [Fact]
        public async Task GetCountriesAsync_Should_Return_Empty_When_None()
        {
            // Act
            var list = await _service.GetCountriesAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(list);
            Assert.Empty(list);
        }

        [Fact]
        public async Task GetCountiesByCountryIdAsync_Should_Return_Counties_For_Country()
        {
            // Arrange
            var country = new Country("Cty");
            var county1 = new County(country.Id, "County1");
            var county2 = new County(country.Id, "County2");
            var other = new County(Guid.NewGuid(), "Other");
            _fakeRepositories.Geography.SeedCountry(country);
            _fakeRepositories.Geography.SeedCounty(county1);
            _fakeRepositories.Geography.SeedCounty(county2);
            _fakeRepositories.Geography.SeedCounty(other);

            // Act
            var list = await _service.GetCountiesByCountryIdAsync(country.Id, CancellationToken.None);

            // Assert
            Assert.Equal(2, list.Count);
            Assert.All(list, c => Assert.Equal(country.Id, c.CountryId));
        }

        [Fact]
        public async Task GetCountiesByCountryIdAsync_Should_Throw_When_Country_Does_Not_Exist()
        {
            // Act
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.GetCountiesByCountryIdAsync(Guid.NewGuid(), CancellationToken.None));

            // Assert
            Assert.Equal("Country was not found.", exception.Message);
        }

        [Fact]
        public async Task GetCitiesByCountyIdAsync_Should_Return_Cities_For_County()
        {
            // Arrange
            var county = new County(Guid.NewGuid(), "Cnty");
            var city1 = new City(county.Id, "City1", "1111");
            var city2 = new City(county.Id, "City2", "2222");
            var other = new City(Guid.NewGuid(), "Other", "3333");
            _fakeRepositories.Geography.SeedCounty(county);
            _fakeRepositories.Geography.SeedCity(city1);
            _fakeRepositories.Geography.SeedCity(city2);
            _fakeRepositories.Geography.SeedCity(other);

            // Act
            var list = await _service.GetCitiesByCountyIdAsync(county.Id, CancellationToken.None);

            // Assert
            Assert.Equal(2, list.Count);
            Assert.All(list, c => Assert.Equal(county.Id, c.CountyId));
        }

        [Fact]
        public async Task GetCitiesByCountyIdAsync_Should_Throw_When_County_Does_Not_Exist()
        {
            // Act
            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => _service.GetCitiesByCountyIdAsync(Guid.NewGuid(), CancellationToken.None));

            // Assert
            Assert.Equal("County was not found.", exception.Message);
        }
    }
}
