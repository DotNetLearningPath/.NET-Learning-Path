using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Building;
using InsuranceApp.Application.Services;
using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using InsuranceApp.UnitTests.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace InsuranceApp.UnitTests.Application.Services;

public sealed class BuildingServiceTests
{
    private readonly Mock<IBuildingRepository> _buildingRepositoryMock;
    private readonly Mock<IClientRepository> _clientRepositoryMock;
    private readonly Mock<IGeographyRepository> _geographyRepositoryMock;
    private readonly Mock<ILogger<BuildingService>> _loggerMock;
    private readonly BuildingService _service;

    public BuildingServiceTests()
    {
        _buildingRepositoryMock = new Mock<IBuildingRepository>();
        _clientRepositoryMock = new Mock<IClientRepository>();
        _geographyRepositoryMock = new Mock<IGeographyRepository>();
        _loggerMock = new Mock<ILogger<BuildingService>>();

        _service = new BuildingService(_buildingRepositoryMock.Object, _clientRepositoryMock.Object, _geographyRepositoryMock.Object, _loggerMock.Object);
    }

    #region Get Building By Id Tests

    [Fact]
    public async Task GetBuildingByIdAsync_ExistingBuilding_ReturnsSuccess()
    {
        // Arrange
        var building = TestData.CreateBuilding1();

        _buildingRepositoryMock.Setup(x => x.GetBuildingByIdAsync(building.BuildingId, It.IsAny<CancellationToken>())).ReturnsAsync(building);

        // Act
        var result = await _service.GetBuildingByIdAsync(building.BuildingId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(building.BuildingId, result.Value.BuildingId);
        Assert.Equal(building.ClientId, result.Value.ClientId);
        Assert.Equal(building.CityId, result.Value.CityId);
        Assert.Equal(building.BuildingTypeId, result.Value.BuildingTypeId);
        Assert.Equal(building.AddressStreet, result.Value.AddressStreet);
        Assert.Equal(building.AddressStreetNumber, result.Value.AddressStreetNumber);
        Assert.Equal(building.ConstructionYear, result.Value.ConstructionYear);
        Assert.Equal(building.NumberOfFloors, result.Value.NumberOfFloors);
        Assert.Equal(building.SurfaceArea, result.Value.SurfaceArea);
        Assert.Equal(building.InsuredValue, result.Value.InsuredValue);
        Assert.Equal(building.RiskIndicators, result.Value.RiskIndicators);
    }

    [Fact]
    public async Task GetBuildingByIdAsync_EmptyBuildingId_ReturnsValidationError()
    {
        // Arrange
        var buildingId = Guid.Empty;

        // Act
        var result = await _service.GetBuildingByIdAsync(buildingId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BuildingErrors.InvalidBuildingId.Code, result.Error.Code);

        _buildingRepositoryMock.Verify(x => x.GetBuildingByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetBuildingByIdAsync_NonExistingBuilding_ReturnsNotFound()
    {
        // Arrange
        var buildingId = TestData.NonExistingId;

        _buildingRepositoryMock.Setup(x => x.GetBuildingByIdAsync(buildingId, It.IsAny<CancellationToken>())).ReturnsAsync((Building?)null);

        // Act
        var result = await _service.GetBuildingByIdAsync(buildingId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(BuildingErrors.NotFound(buildingId).Code, result.Error.Code);
    }

    #endregion

    #region Get Buildings By Client Tests

    [Fact]
    public async Task GetBuildingsByClientAsync_ExistingClient_ReturnsBuildings()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var building1 = TestData.CreateBuilding1();
        var building2 = TestData.CreateBuilding2();
        var buildings = new List<Building> { building1, building2 };

        SetupExistingClient(client);

        _buildingRepositoryMock.Setup(x => x.GetBuildingsByClientAsync(client.ClientId, It.IsAny<CancellationToken>())).ReturnsAsync(buildings);

        // Act
        var result = await _service.GetBuildingsByClientAsync(client.ClientId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(buildings.Count, result.Value.Count);
        Assert.All(result.Value, building => Assert.Equal(client.ClientId, building.ClientId));

        _buildingRepositoryMock.Verify(x => x.GetBuildingsByClientAsync(client.ClientId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBuildingsByClientAsync_ClientWithoutBuildings_ReturnsEmptyList()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();

        SetupExistingClient(client);

        _buildingRepositoryMock.Setup(x => x.GetBuildingsByClientAsync(client.ClientId, It.IsAny<CancellationToken>())).ReturnsAsync([]);

        // Act
        var result = await _service.GetBuildingsByClientAsync(client.ClientId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetBuildingsByClientAsync_EmptyClientId_ReturnsValidationError()
    {
        // Arrange
        var clientId = Guid.Empty;

        // Act
        var result = await _service.GetBuildingsByClientAsync(clientId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BuildingErrors.InvalidClientId.Code, result.Error.Code);

        _clientRepositoryMock.Verify(x => x.GetClientByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _buildingRepositoryMock.Verify(x => x.GetBuildingsByClientAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetBuildingsByClientAsync_NonExistingClient_ReturnsNotFound()
    {
        // Arrange
        var clientId = TestData.NonExistingId;

        _clientRepositoryMock.Setup(x => x.GetClientByIdAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync((Client?)null);

        // Act
        var result = await _service.GetBuildingsByClientAsync(clientId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(ClientErrors.NotFound(clientId).Code, result.Error.Code);

        _buildingRepositoryMock.Verify(x => x.GetBuildingsByClientAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Create Building Tests

    [Fact]
    public async Task CreateBuildingForClientAsync_ValidBuilding_ReturnsSuccess()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var cityId = TestData.CityId;
        var buildingTypeId = TestData.BuildingTypeId;
        var buildingDto = TestData.CreateBuildingDto();

        SetupExistingClient(client);
        SetupExistingCity(cityId);
        SetupExistingBuildingType(buildingTypeId);

        // Act
        var result = await _service.CreateBuildingForClientAsync(client.ClientId, buildingDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(client.ClientId, result.Value.ClientId);
        Assert.Equal(buildingDto.CityId, result.Value.CityId);
        Assert.Equal(buildingDto.BuildingTypeId, result.Value.BuildingTypeId);
        Assert.Equal(buildingDto.AddressStreet, result.Value.AddressStreet);
        Assert.Equal(buildingDto.AddressStreetNumber, result.Value.AddressStreetNumber);
        Assert.Equal(buildingDto.ConstructionYear, result.Value.ConstructionYear);
        Assert.Equal(buildingDto.NumberOfFloors, result.Value.NumberOfFloors);
        Assert.Equal(buildingDto.SurfaceArea, result.Value.SurfaceArea);
        Assert.Equal(buildingDto.InsuredValue, result.Value.InsuredValue);
        Assert.Equal(buildingDto.RiskIndicators, result.Value.RiskIndicators);

        _buildingRepositoryMock.Verify(x => x.AddBuildingAsync(It.Is<Building>(building =>
            building.ClientId == client.ClientId &&
            building.CityId == buildingDto.CityId &&
            building.BuildingTypeId == buildingDto.BuildingTypeId &&
            building.AddressStreet == buildingDto.AddressStreet &&
            building.AddressStreetNumber == buildingDto.AddressStreetNumber &&
            building.SurfaceArea == buildingDto.SurfaceArea &&
            building.InsuredValue == buildingDto.InsuredValue), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_EmptyClientId_ReturnsValidationError()
    {
        // Arrange
        var clientId = Guid.Empty;
        var buildingDto = TestData.CreateBuildingDto();

        // Act
        var result = await _service.CreateBuildingForClientAsync(clientId, buildingDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BuildingErrors.InvalidClientId.Code, result.Error.Code);

        _clientRepositoryMock.Verify(x => x.GetClientByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _buildingRepositoryMock.Verify(x => x.AddBuildingAsync(It.IsAny<Building>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_NonExistingClient_ReturnsNotFound()
    {
        // Arrange
        var clientId = TestData.NonExistingId;
        var buildingDto = TestData.CreateBuildingDto();

        _clientRepositoryMock.Setup(x => x.GetClientByIdAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync((Client?)null);

        // Act
        var result = await _service.CreateBuildingForClientAsync(clientId, buildingDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(ClientErrors.NotFound(clientId).Code, result.Error.Code);

        _geographyRepositoryMock.Verify(x => x.CityExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _buildingRepositoryMock.Verify(x => x.BuildingTypeExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _buildingRepositoryMock.Verify(x => x.AddBuildingAsync(It.IsAny<Building>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_EmptyCityId_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var createBuildingDto = TestData.CreateBuildingDto();
        var buildingDto = createBuildingDto with { CityId = Guid.Empty };

        SetupExistingClient(client);

        // Act
        var result = await _service.CreateBuildingForClientAsync(client.ClientId, buildingDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BuildingErrors.InvalidCityId.Code, result.Error.Code);

        _geographyRepositoryMock.Verify(x => x.CityExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _buildingRepositoryMock.Verify(x => x.AddBuildingAsync(It.IsAny<Building>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_NonExistingCity_ReturnsNotFound()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var cityId = TestData.CityId;
        var buildingDto = TestData.CreateBuildingDto();

        SetupExistingClient(client);

        _geographyRepositoryMock.Setup(x => x.CityExistsAsync(cityId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.CreateBuildingForClientAsync(client.ClientId, buildingDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(BuildingErrors.CityNotFound(cityId).Code, result.Error.Code);

        _buildingRepositoryMock.Verify(x => x.BuildingTypeExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _buildingRepositoryMock.Verify(x => x.AddBuildingAsync(It.IsAny<Building>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_EmptyBuildingTypeId_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var cityId = TestData.CityId;
        var createBuildingDto = TestData.CreateBuildingDto();
        var buildingDto = createBuildingDto with { BuildingTypeId = Guid.Empty };

        SetupExistingClient(client);
        SetupExistingCity(cityId);

        // Act
        var result = await _service.CreateBuildingForClientAsync(client.ClientId, buildingDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BuildingErrors.InvalidBuildingType.Code, result.Error.Code);

        _buildingRepositoryMock.Verify(x => x.BuildingTypeExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _buildingRepositoryMock.Verify(x => x.AddBuildingAsync(It.IsAny<Building>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_NonExistingBuildingType_ReturnsNotFound()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var cityId = TestData.CityId;
        var buildingTypeId = TestData.BuildingTypeId;
        var buildingDto = TestData.CreateBuildingDto();

        SetupExistingClient(client);
        SetupExistingCity(cityId);

        _buildingRepositoryMock.Setup(x => x.BuildingTypeExistsAsync(buildingTypeId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.CreateBuildingForClientAsync(client.ClientId, buildingDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(BuildingErrors.BuildingTypeNotFound(buildingTypeId).Code, result.Error.Code);

        _buildingRepositoryMock.Verify(x => x.AddBuildingAsync(It.IsAny<Building>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_MissingStreet_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var buildingDto = TestData.CreateBuildingDto() with { AddressStreet = "" };

        // Act & Assert
        await AssertInvalidCreateBuildingAsync(client, buildingDto, BuildingErrors.AddressStreetRequired);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_StreetTooLong_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var buildingDto = TestData.CreateBuildingDto() with 
        { 
            AddressStreet = new string('A', BuildingConstraints.AddressStreetMaxLength + 1) 
        };

        // Act & Assert
        await AssertInvalidCreateBuildingAsync(client, buildingDto, BuildingErrors.InvalidAddressStreetLength);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_MissingStreetNumber_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var buildingDto = TestData.CreateBuildingDto() with { AddressStreetNumber = "" };

        // Act & Assert
        await AssertInvalidCreateBuildingAsync(client, buildingDto, BuildingErrors.AddressStreetNumberRequired);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_StreetNumberTooLong_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var buildingDto = TestData.CreateBuildingDto() with 
        { 
            AddressStreetNumber = new string('1', BuildingConstraints.AddressStreetNumberMaxLength + 1) 
        };

        // Act & Assert
        await AssertInvalidCreateBuildingAsync(client, buildingDto, BuildingErrors.InvalidAddressStreetNumberLength);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_ConstructionYearBelowMinimum_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var buildingDto = TestData.CreateBuildingDto() with { ConstructionYear = BuildingConstraints.MinConstructionYear - 1 };

        // Act & Assert
        await AssertInvalidCreateBuildingAsync(client, buildingDto, BuildingErrors.InvalidConstructionYear);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_ConstructionYearInFuture_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var buildingDto = TestData.CreateBuildingDto() with { ConstructionYear = DateTime.UtcNow.Year + 1 };

        // Act & Assert
        await AssertInvalidCreateBuildingAsync(client, buildingDto, BuildingErrors.InvalidConstructionYear);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_NumberOfFloorsBelowMinimum_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var buildingDto = TestData.CreateBuildingDto() with { NumberOfFloors = BuildingConstraints.MinNumberOfFloors - 1 };

        // Act & Assert
        await AssertInvalidCreateBuildingAsync(client, buildingDto, BuildingErrors.InvalidNumberOfFloors);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_NumberOfFloorsAboveMaximum_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var buildingDto = TestData.CreateBuildingDto() with { NumberOfFloors = BuildingConstraints.MaxNumberOfFloors + 1 };

        // Act & Assert
        await AssertInvalidCreateBuildingAsync(client, buildingDto, BuildingErrors.InvalidNumberOfFloors);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_SurfaceAreaBelowMinimum_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var buildingDto = TestData.CreateBuildingDto() with { SurfaceArea = BuildingConstraints.MinSurfaceArea - 0.01m };

        // Act & Assert
        await AssertInvalidCreateBuildingAsync(client, buildingDto, BuildingErrors.InvalidSurfaceArea);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_SurfaceAreaAboveMaximum_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var buildingDto = TestData.CreateBuildingDto() with { SurfaceArea = BuildingConstraints.MaxSurfaceArea + 0.01m };

        // Act & Assert
        await AssertInvalidCreateBuildingAsync(client, buildingDto, BuildingErrors.InvalidSurfaceArea);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_SurfaceAreaWithTooManyDecimals_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var buildingDto = TestData.CreateBuildingDto() with { SurfaceArea = TestData.InvalidSurfaceAreaScale };

        // Act & Assert
        await AssertInvalidCreateBuildingAsync(client, buildingDto, BuildingErrors.InvalidSurfaceAreaScale);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_InsuredValueBelowMinimum_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var buildingDto = TestData.CreateBuildingDto() with { InsuredValue = BuildingConstraints.MinInsuredValue - 0.01m };

        // Act & Assert
        await AssertInvalidCreateBuildingAsync(client, buildingDto, BuildingErrors.InvalidInsuredValue);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_InsuredValueAboveMaximum_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var buildingDto = TestData.CreateBuildingDto() with { InsuredValue = BuildingConstraints.MaxInsuredValue + 0.01m };

        // Act & Assert
        await AssertInvalidCreateBuildingAsync(client, buildingDto, BuildingErrors.InvalidInsuredValue);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_InsuredValueWithTooManyDecimals_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var buildingDto = TestData.CreateBuildingDto() with { InsuredValue = TestData.InvalidInsuredValueScale };

        // Act & Assert
        await AssertInvalidCreateBuildingAsync(client, buildingDto, BuildingErrors.InvalidInsuredValueScale);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_RiskIndicatorsTooLong_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var buildingDto = TestData.CreateBuildingDto() with 
        { 
            RiskIndicators = new string('A', BuildingConstraints.RiskIndicatorsMaxLength + 1) 
        };

        // Act & Assert
        await AssertInvalidCreateBuildingAsync(client, buildingDto, BuildingErrors.InvalidRiskIndicatorsLength);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_ValidBuilding_TrimsTextValues()
    {
        // Arrange
        var client = TestData.CreateClientForBuilding();
        var cityId = TestData.CityId;
        var buildingTypeId = TestData.BuildingTypeId;
        var createBuildingDto = TestData.CreateBuildingDto();

        var buildingDto = createBuildingDto with
        {
            AddressStreet = $"  {createBuildingDto.AddressStreet}  ",
            AddressStreetNumber = $"  {createBuildingDto.AddressStreetNumber}  ",
            RiskIndicators = $"  {createBuildingDto.RiskIndicators}  "
        };

        SetupExistingClient(client);
        SetupExistingCity(cityId);
        SetupExistingBuildingType(buildingTypeId);

        // Act
        var result = await _service.CreateBuildingForClientAsync(client.ClientId, buildingDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(createBuildingDto.AddressStreet, result.Value.AddressStreet);
        Assert.Equal(createBuildingDto.AddressStreetNumber, result.Value.AddressStreetNumber);
        Assert.Equal(createBuildingDto.RiskIndicators, result.Value.RiskIndicators);
    }

    #endregion

    #region Update Building Tests

    [Fact]
    public async Task UpdateBuildingAsync_ValidBuilding_ReturnsUpdatedBuilding()
    {
        // Arrange
        var cityId = TestData.CityId;
        var buildingTypeId = TestData.BuildingTypeId;
        var building = TestData.CreateBuilding1();
        var buildingDto = TestData.UpdateBuildingDto();
        var originalClientId = building.ClientId;

        SetupExistingCity(cityId);
        SetupExistingBuildingType(buildingTypeId);
        SetupExistingBuildingForUpdate(building);

        // Act
        var result = await _service.UpdateBuildingAsync(building.BuildingId, buildingDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(originalClientId, result.Value.ClientId);
        Assert.Equal(buildingDto.CityId, result.Value.CityId);
        Assert.Equal(buildingDto.BuildingTypeId, result.Value.BuildingTypeId);
        Assert.Equal(buildingDto.AddressStreet, result.Value.AddressStreet);
        Assert.Equal(buildingDto.AddressStreetNumber, result.Value.AddressStreetNumber);
        Assert.Equal(buildingDto.ConstructionYear, result.Value.ConstructionYear);
        Assert.Equal(buildingDto.NumberOfFloors, result.Value.NumberOfFloors);
        Assert.Equal(buildingDto.SurfaceArea, result.Value.SurfaceArea);
        Assert.Equal(buildingDto.InsuredValue, result.Value.InsuredValue);
        Assert.Equal(buildingDto.RiskIndicators, result.Value.RiskIndicators);
        Assert.NotNull(building.ModifiedAt);

        _buildingRepositoryMock.Verify(x => x.SaveBuildingChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateBuildingAsync_EmptyBuildingId_ReturnsValidationError()
    {
        // Arrange
        var buildingId = Guid.Empty;
        var buildingDto = TestData.UpdateBuildingDto();

        // Act
        var result = await _service.UpdateBuildingAsync(buildingId, buildingDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BuildingErrors.InvalidBuildingId.Code, result.Error.Code);

        _buildingRepositoryMock.Verify(x => x.GetBuildingForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateBuildingAsync_NonExistingBuilding_ReturnsNotFound()
    {
        // Arrange
        var buildingId = TestData.NonExistingId;
        var buildingDto = TestData.UpdateBuildingDto();

        SetupExistingCity(buildingDto.CityId);
        SetupExistingBuildingType(buildingDto.BuildingTypeId);

        _buildingRepositoryMock.Setup(x => x.GetBuildingForUpdateAsync(buildingId, It.IsAny<CancellationToken>())).ReturnsAsync((Building?)null);

        // Act
        var result = await _service.UpdateBuildingAsync(buildingId, buildingDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(BuildingErrors.NotFound(buildingId).Code, result.Error.Code);

        _buildingRepositoryMock.Verify(x => x.GetBuildingForUpdateAsync(buildingId, It.IsAny<CancellationToken>()), Times.Once);
        _buildingRepositoryMock.Verify(x => x.SaveBuildingChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateBuildingAsync_EmptyCityId_ReturnsValidationError()
    {
        // Arrange
        var building = TestData.CreateBuilding1();
        var updateBuildingDto = TestData.UpdateBuildingDto();
        var buildingDto = updateBuildingDto with { CityId = Guid.Empty };

        // Act
        var result = await _service.UpdateBuildingAsync(building.BuildingId, buildingDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BuildingErrors.InvalidCityId.Code, result.Error.Code);

        _geographyRepositoryMock.Verify(x => x.CityExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _buildingRepositoryMock.Verify(x => x.SaveBuildingChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateBuildingAsync_NonExistingCity_ReturnsNotFound()
    {
        // Arrange
        var cityId = TestData.CityId;
        var building = TestData.CreateBuilding1();
        var buildingDto = TestData.UpdateBuildingDto();

        _geographyRepositoryMock.Setup(x => x.CityExistsAsync(cityId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.UpdateBuildingAsync(building.BuildingId, buildingDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(BuildingErrors.CityNotFound(cityId).Code, result.Error.Code);

        _buildingRepositoryMock.Verify(x => x.BuildingTypeExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _buildingRepositoryMock.Verify(x => x.GetBuildingForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _buildingRepositoryMock.Verify(x => x.SaveBuildingChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateBuildingAsync_EmptyBuildingTypeId_ReturnsValidationError()
    {
        // Arrange
        var cityId = TestData.CityId;
        var building = TestData.CreateBuilding1();
        var updateBuildingDto = TestData.UpdateBuildingDto();
        var buildingDto = updateBuildingDto with { BuildingTypeId = Guid.Empty };

        SetupExistingCity(cityId);

        // Act
        var result = await _service.UpdateBuildingAsync(building.BuildingId, buildingDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BuildingErrors.InvalidBuildingType.Code, result.Error.Code);

        _buildingRepositoryMock.Verify(x => x.BuildingTypeExistsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _buildingRepositoryMock.Verify(x => x.GetBuildingForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _buildingRepositoryMock.Verify(x => x.SaveBuildingChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateBuildingAsync_NonExistingBuildingType_ReturnsNotFound()
    {
        // Arrange
        var cityId = TestData.CityId;
        var buildingTypeId = TestData.BuildingTypeId;
        var building = TestData.CreateBuilding1();
        var buildingDto = TestData.UpdateBuildingDto();

        SetupExistingCity(cityId);

        _buildingRepositoryMock.Setup(x => x.BuildingTypeExistsAsync(buildingTypeId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.UpdateBuildingAsync(building.BuildingId, buildingDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(BuildingErrors.BuildingTypeNotFound(buildingTypeId).Code, result.Error.Code);

        _buildingRepositoryMock.Verify(x => x.GetBuildingForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _buildingRepositoryMock.Verify(x => x.SaveBuildingChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateBuildingAsync_MissingStreet_ReturnsValidationError()
    {
        // Arrange
        var building = TestData.CreateBuilding1();
        var buildingDto = TestData.UpdateBuildingDto() with { AddressStreet = "" };

        // Act & Assert
        await AssertInvalidUpdateBuildingAsync(building, buildingDto, BuildingErrors.AddressStreetRequired);
    }

    [Fact]
    public async Task UpdateBuildingAsync_SurfaceAreaWithTooManyDecimals_ReturnsValidationError()
    {
        // Arrange
        var building = TestData.CreateBuilding1();
        var buildingDto = TestData.UpdateBuildingDto() with { SurfaceArea = TestData.InvalidSurfaceAreaScale };

        // Act & Assert
        await AssertInvalidUpdateBuildingAsync(building, buildingDto, BuildingErrors.InvalidSurfaceAreaScale);
    }

    [Fact]
    public async Task UpdateBuildingAsync_InsuredValueWithTooManyDecimals_ReturnsValidationError()
    {
        // Arrange
        var building = TestData.CreateBuilding1();
        var buildingDto = TestData.UpdateBuildingDto() with { InsuredValue = TestData.InvalidInsuredValueScale };

        // Act & Assert
        await AssertInvalidUpdateBuildingAsync(building, buildingDto, BuildingErrors.InvalidInsuredValueScale);
    }

    [Fact]
    public async Task UpdateBuildingAsync_RiskIndicatorsTooLong_ReturnsValidationError()
    {
        // Arrange
        var building = TestData.CreateBuilding1();
        var buildingDto = TestData.UpdateBuildingDto() with 
        { 
            RiskIndicators = new string('A', BuildingConstraints.RiskIndicatorsMaxLength + 1) 
        };

        // Act & Assert
        await AssertInvalidUpdateBuildingAsync(building, buildingDto, BuildingErrors.InvalidRiskIndicatorsLength);
    }

    [Fact]
    public async Task UpdateBuildingAsync_ValidBuilding_TrimsTextValues()
    {
        // Arrange
        var cityId = TestData.CityId;
        var buildingTypeId = TestData.BuildingTypeId;
        var building = TestData.CreateBuilding1();
        var updateBuildingDto = TestData.UpdateBuildingDto();

        var buildingDto = updateBuildingDto with
        {
            AddressStreet = $"  {updateBuildingDto.AddressStreet}  ",
            AddressStreetNumber = $"  {updateBuildingDto.AddressStreetNumber}  ",
            RiskIndicators = $"  {updateBuildingDto.RiskIndicators}  "
        };

        SetupExistingCity(cityId);
        SetupExistingBuildingType(buildingTypeId);
        SetupExistingBuildingForUpdate(building);

        // Act
        var result = await _service.UpdateBuildingAsync(building.BuildingId, buildingDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(updateBuildingDto.AddressStreet, result.Value.AddressStreet);
        Assert.Equal(updateBuildingDto.AddressStreetNumber, result.Value.AddressStreetNumber);
        Assert.Equal(updateBuildingDto.RiskIndicators, result.Value.RiskIndicators);
    }

    #endregion


    #region Helpers

    private void SetupExistingClient(Client client)
    {
        _clientRepositoryMock.Setup(x => x.GetClientByIdAsync(client.ClientId, It.IsAny<CancellationToken>())).ReturnsAsync(client);
    }

    private void SetupExistingCity(Guid cityId)
    {
        _geographyRepositoryMock.Setup(x => x.CityExistsAsync(cityId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    private void SetupExistingBuildingType(Guid buildingTypeId)
    {
        _buildingRepositoryMock.Setup(x => x.BuildingTypeExistsAsync(buildingTypeId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    private void SetupExistingBuildingForUpdate(Building building)
    {
        _buildingRepositoryMock.Setup(x => x.GetBuildingForUpdateAsync(building.BuildingId, It.IsAny<CancellationToken>())).ReturnsAsync(building);
    }


    private async Task AssertInvalidCreateBuildingAsync(Client client, CreateBuildingDto buildingDto, Error expectedError)
    {
        SetupExistingClient(client);

        var result = await _service.CreateBuildingForClientAsync(client.ClientId, buildingDto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(expectedError.Code, result.Error.Code);

        _buildingRepositoryMock.Verify(x => x.AddBuildingAsync(It.IsAny<Building>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private async Task AssertInvalidUpdateBuildingAsync(Building building, UpdateBuildingDto buildingDto, Error expectedError)
    {
        var result = await _service.UpdateBuildingAsync(building.BuildingId, buildingDto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(expectedError.Code, result.Error.Code);

        _buildingRepositoryMock.Verify(x => x.GetBuildingForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _buildingRepositoryMock.Verify(x => x.SaveBuildingChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
