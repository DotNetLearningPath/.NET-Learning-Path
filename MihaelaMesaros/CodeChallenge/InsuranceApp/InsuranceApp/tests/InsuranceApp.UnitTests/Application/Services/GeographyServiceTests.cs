using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.Services;
using InsuranceApp.Domain.Entities;
using InsuranceApp.UnitTests.Common;
using Moq;

namespace InsuranceApp.UnitTests.Application.Services;

public sealed class GeographyServiceTests
{
    private readonly Mock<IGeographyRepository> _geographyRepositoryMock;
    private readonly GeographyService _service;

    public GeographyServiceTests()
    {
        _geographyRepositoryMock = new Mock<IGeographyRepository>();

        _service = new GeographyService(_geographyRepositoryMock.Object);
    }

    #region Countries Tests

    [Fact]
    public async Task GetCountriesAsync_ReturnsMappedCountries()
    {
        // Arrange
        var country1 = TestData.CreateCountry1();
        var country2 = TestData.CreateCountry2();
        var countries = new List<Country> { country1, country2 };

        _geographyRepositoryMock.Setup(x => x.GetCountriesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(countries);

        // Act
        var result = await _service.GetCountriesAsync(CancellationToken.None);

        // Assert
        Assert.Equal(countries.Count, result.Count);
        Assert.Equal(country1.CountryId, result[0].CountryId);
        Assert.Equal(country1.Name, result[0].Name);
        Assert.Equal(country2.CountryId, result[1].CountryId);
        Assert.Equal(country2.Name, result[1].Name);
    }

    #endregion

    #region Counties Tests

    [Fact]
    public async Task GetCountiesByCountryAsync_WhenCountryExists_ReturnsSuccess()
    {
        // Arrange
        var country = TestData.CreateCountry1();
        var county1 = TestData.CreateCounty1();
        var county2 = TestData.CreateCounty2();
        var counties = new List<County> { county1, county2 };

        _geographyRepositoryMock.Setup(x => x.CountryExistsAsync(country.CountryId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _geographyRepositoryMock.Setup(x => x.GetCountiesByCountryAsync(country.CountryId, It.IsAny<CancellationToken>())).ReturnsAsync(counties);

        // Act
        var result = await _service.GetCountiesByCountryAsync(country.CountryId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(counties.Count, result.Value.Count);
        Assert.Equal(county1.Name, result.Value[0].Name);
        Assert.Equal(county2.Name, result.Value[1].Name);
    }

    [Fact]
    public async Task GetCountiesByCountryAsync_WhenCountryDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var countryId = TestData.NonExistingId;

        _geographyRepositoryMock.Setup(x => x.CountryExistsAsync(countryId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.GetCountiesByCountryAsync(countryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(GeographyErrors.CountryNotFound(countryId).Code, result.Error.Code);

        _geographyRepositoryMock.Verify(x => x.GetCountiesByCountryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetCountiesByCountryAsync_EmptyCountryId_ReturnsValidationError()
    {
        // Arrange
        var countryId = Guid.Empty;

        // Act
        var result = await _service.GetCountiesByCountryAsync(countryId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(GeographyErrors.InvalidCountryId.Code, result.Error.Code);

        _geographyRepositoryMock.Verify(x => x.CountryExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _geographyRepositoryMock.Verify(x => x.GetCountiesByCountryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Cities Tests

    [Fact]
    public async Task GetCitiesByCountyAsync_WhenCountyExists_ReturnsSuccess()
    {
        // Arrange
        var county = TestData.CreateCounty1();
        var city1 = TestData.CreateCity1();
        var city2 = TestData.CreateCity2();
        var cities = new List<City> { city1, city2 };

        _geographyRepositoryMock.Setup(x => x.CountyExistsAsync(county.CountyId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        _geographyRepositoryMock.Setup(x => x.GetCitiesByCountyAsync(county.CountyId, It.IsAny<CancellationToken>())).ReturnsAsync(cities);

        // Act
        var result = await _service.GetCitiesByCountyAsync(county.CountyId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(cities.Count, result.Value.Count);
        Assert.Equal(city1.Name, result.Value[0].Name);
        Assert.Equal(city2.Name, result.Value[1].Name);
    }

    [Fact]
    public async Task GetCitiesByCountyAsync_WhenCountyDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var countyId = TestData.NonExistingId;

        _geographyRepositoryMock.Setup(x => x.CountyExistsAsync(countyId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.GetCitiesByCountyAsync(countyId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(GeographyErrors.CountyNotFound(countyId).Code, result.Error.Code);

        _geographyRepositoryMock.Verify(x => x.GetCitiesByCountyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetCitiesByCountyAsync_EmptyCountyId_ReturnsValidationError()
    {
        // Arrange
        var countyId = Guid.Empty;

        // Act
        var result = await _service.GetCitiesByCountyAsync(countyId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(GeographyErrors.InvalidCountyId.Code, result.Error.Code);

        _geographyRepositoryMock.Verify(x => x.CountyExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _geographyRepositoryMock.Verify(x => x.GetCitiesByCountyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
