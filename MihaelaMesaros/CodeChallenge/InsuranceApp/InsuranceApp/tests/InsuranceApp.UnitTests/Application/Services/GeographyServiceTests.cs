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

    [Fact]
    public async Task GetCountriesAsync_ReturnsMappedCountries()
    {
        // Arrange
        var romaniaId = Guid.NewGuid();
        var hungaryId = Guid.NewGuid();

        var countries = new List<Country>
        {
            new()
            {
                CountryId = romaniaId,
                Name = "Romania"
            },
            new()
            {
                CountryId = hungaryId,
                Name = "Hungary"
            }
        };

        _geographyRepositoryMock
            .Setup(x => x.GetCountriesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(countries);

        // Act
        var result = await _service.GetCountriesAsync(
            CancellationToken.None);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal(romaniaId, result[0].CountryId);
        Assert.Equal("Romania", result[0].Name);

        Assert.Equal(hungaryId, result[1].CountryId);
        Assert.Equal("Hungary", result[1].Name);
    }

    [Fact]
    public async Task GetCountiesByCountryAsync_WhenCountryExists_ReturnsSuccess()
    {
        // Arrange
        var countryId = Guid.NewGuid();

        var counties = new List<County>
        {
            new()
            {
                CountyId = Guid.NewGuid(),
                CountryId = countryId,
                Name = "Cluj"
            },
            new()
            {
                CountyId = Guid.NewGuid(),
                CountryId = countryId,
                Name = "Brasov"
            }
        };

        _geographyRepositoryMock
            .Setup(x => x.CountryExistsAsync(
                countryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _geographyRepositoryMock
            .Setup(x => x.GetCountiesByCountryAsync(
                countryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(counties);

        // Act
        var result = await _service.GetCountiesByCountryAsync(
            countryId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(2, result.Value.Count);
        Assert.Equal("Cluj", result.Value[0].Name);
        Assert.Equal("Brasov", result.Value[1].Name);
    }

    [Fact]
    public async Task GetCountiesByCountryAsync_WhenCountryDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var countryId = TestConstants.NonExistingId;

        _geographyRepositoryMock
            .Setup(x => x.CountryExistsAsync(
                countryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.GetCountiesByCountryAsync(
            countryId,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);

        _geographyRepositoryMock.Verify(
            x => x.GetCountiesByCountryAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetCitiesByCountyAsync_WhenCountyExists_ReturnsSuccess()
    {
        // Arrange
        var countyId = Guid.NewGuid();

        var cities = new List<City>
        {
            new()
            {
                CityId = Guid.NewGuid(),
                CountyId = countyId,
                Name = "Cluj-Napoca"
            },
            new()
            {
                CityId = Guid.NewGuid(),
                CountyId = countyId,
                Name = "Turda"
            }
        };

        _geographyRepositoryMock
            .Setup(x => x.CountyExistsAsync(
                countyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _geographyRepositoryMock
            .Setup(x => x.GetCitiesByCountyAsync(
                countyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(cities);

        // Act
        var result = await _service.GetCitiesByCountyAsync(
            countyId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(2, result.Value.Count);
        Assert.Equal("Cluj-Napoca", result.Value[0].Name);
        Assert.Equal("Turda", result.Value[1].Name);
    }

    [Fact]
    public async Task GetCitiesByCountyAsync_WhenCountyDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var countyId = TestConstants.NonExistingId;

        _geographyRepositoryMock
            .Setup(x => x.CountyExistsAsync(
                countyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.GetCitiesByCountyAsync(
            countyId,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);

        _geographyRepositoryMock.Verify(
            x => x.GetCitiesByCountyAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetCountiesByCountryAsync_EmptyCountryId_ReturnsValidationError()
    {
        // Act
        var result = await _service.GetCountiesByCountryAsync(
            Guid.Empty,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(
            GeographyErrors.InvalidCountryId.Code,
            result.Error.Code);

        _geographyRepositoryMock.Verify(
            x => x.CountryExistsAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _geographyRepositoryMock.Verify(
            x => x.GetCountiesByCountryAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetCitiesByCountyAsync_EmptyCountyId_ReturnsValidationError()
    {
        // Act
        var result = await _service.GetCitiesByCountyAsync(
            Guid.Empty,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(
            GeographyErrors.InvalidCountyId.Code,
            result.Error.Code);

        _geographyRepositoryMock.Verify(
            x => x.CountyExistsAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _geographyRepositoryMock.Verify(
            x => x.GetCitiesByCountyAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}