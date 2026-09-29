using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Building;
using InsuranceApp.Application.Services;
using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
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

        _service = new BuildingService(
            _buildingRepositoryMock.Object,
            _clientRepositoryMock.Object,
            _geographyRepositoryMock.Object,
            _loggerMock.Object);
    }

    #region Get Building By Id Tests

    [Fact]
    public async Task GetBuildingByIdAsync_ExistingBuilding_ReturnsSuccess()
    {
        // Arrange
        var building = CreateBuildingEntity();

        _buildingRepositoryMock
            .Setup(x => x.GetBuildingByIdAsync(building.BuildingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(building);

        // Act
        var result = await _service.GetBuildingByIdAsync(building.BuildingId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(building.BuildingId, result.Value.BuildingId);
        Assert.Equal(building.ClientId, result.Value.ClientId);
        Assert.Equal(building.CityId, result.Value.CityId);
        Assert.Equal(building.AddressStreet, result.Value.AddressStreet);
        Assert.Equal(building.AddressStreetNumber, result.Value.AddressStreetNumber);
        Assert.Equal(building.ConstructionYear, result.Value.ConstructionYear);
        Assert.Equal(building.BuildingType, result.Value.BuildingType);
        Assert.Equal(building.NumberOfFloors, result.Value.NumberOfFloors);
        Assert.Equal(building.SurfaceArea, result.Value.SurfaceArea);
        Assert.Equal(building.InsuredValue, result.Value.InsuredValue);
        Assert.Equal(building.RiskIndicators, result.Value.RiskIndicators);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetBuildingByIdAsync_InvalidBuildingId_ReturnsValidationError(int buildingId)
    {
        // Act
        var result = await _service.GetBuildingByIdAsync(buildingId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BuildingErrors.InvalidBuildingId.Code, result.Error.Code);

        _buildingRepositoryMock.Verify(
            x => x.GetBuildingByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetBuildingByIdAsync_NonExistingBuilding_ReturnsNotFound()
    {
        // Arrange
        const int buildingId = TestConstants.NonExistingId;

        _buildingRepositoryMock
            .Setup(x => x.GetBuildingByIdAsync(buildingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Building?)null);

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
        const int clientId = 1;

        var buildings = new List<Building>
        {
            CreateBuildingEntity(1, clientId),
            CreateBuildingEntity(2, clientId)
        };

        SetupExistingClient(clientId);

        _buildingRepositoryMock
            .Setup(x => x.GetBuildingsByClientAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(buildings);

        // Act
        var result = await _service.GetBuildingsByClientAsync(clientId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);
        Assert.All(result.Value, building => Assert.Equal(clientId, building.ClientId));

        _buildingRepositoryMock.Verify(
            x => x.GetBuildingsByClientAsync(clientId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetBuildingsByClientAsync_ClientWithoutBuildings_ReturnsEmptyList()
    {
        // Arrange
        const int clientId = 1;

        SetupExistingClient(clientId);

        _buildingRepositoryMock
            .Setup(x => x.GetBuildingsByClientAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _service.GetBuildingsByClientAsync(clientId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetBuildingsByClientAsync_InvalidClientId_ReturnsValidationError(int clientId)
    {
        // Act
        var result = await _service.GetBuildingsByClientAsync(clientId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BuildingErrors.InvalidClientId.Code, result.Error.Code);

        _clientRepositoryMock.Verify(
            x => x.GetClientByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _buildingRepositoryMock.Verify(
            x => x.GetBuildingsByClientAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetBuildingsByClientAsync_NonExistingClient_ReturnsNotFound()
    {
        // Arrange
        const int clientId = TestConstants.NonExistingId;

        _clientRepositoryMock
            .Setup(x => x.GetClientByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        // Act
        var result = await _service.GetBuildingsByClientAsync(clientId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(ClientErrors.NotFound(clientId).Code, result.Error.Code);

        _buildingRepositoryMock.Verify(
            x => x.GetBuildingsByClientAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion


    #region Create Building Tests

    [Fact]
    public async Task CreateBuildingForClientAsync_ValidBuilding_ReturnsSuccess()
    {
        // Arrange
        const int clientId = 1;
        var dto = CreateValidBuildingDto();

        SetupExistingClient(clientId);
        SetupExistingCity(dto.CityId);

        // Act
        var result = await _service.CreateBuildingForClientAsync(clientId, dto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(clientId, result.Value.ClientId);
        Assert.Equal(dto.CityId, result.Value.CityId);
        Assert.Equal(dto.AddressStreet, result.Value.AddressStreet);
        Assert.Equal(dto.AddressStreetNumber, result.Value.AddressStreetNumber);
        Assert.Equal(dto.ConstructionYear, result.Value.ConstructionYear);
        Assert.Equal(dto.BuildingType, result.Value.BuildingType);
        Assert.Equal(dto.NumberOfFloors, result.Value.NumberOfFloors);
        Assert.Equal(dto.SurfaceArea, result.Value.SurfaceArea);
        Assert.Equal(dto.InsuredValue, result.Value.InsuredValue);
        Assert.Equal(dto.RiskIndicators, result.Value.RiskIndicators);

        _buildingRepositoryMock.Verify(
            x => x.AddBuildingAsync(
                It.Is<Building>(building =>
                    building.ClientId == clientId &&
                    building.CityId == dto.CityId &&
                    building.AddressStreet == dto.AddressStreet &&
                    building.AddressStreetNumber == dto.AddressStreetNumber &&
                    building.BuildingType == dto.BuildingType &&
                    building.SurfaceArea == dto.SurfaceArea &&
                    building.InsuredValue == dto.InsuredValue),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateBuildingForClientAsync_InvalidClientId_ReturnsValidationError(int clientId)
    {
        // Arrange
        var dto = CreateValidBuildingDto();

        // Act
        var result = await _service.CreateBuildingForClientAsync(clientId, dto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BuildingErrors.InvalidClientId.Code, result.Error.Code);

        _clientRepositoryMock.Verify(
            x => x.GetClientByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _buildingRepositoryMock.Verify(
            x => x.AddBuildingAsync(It.IsAny<Building>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_NonExistingClient_ReturnsNotFound()
    {
        // Arrange
        const int clientId = TestConstants.NonExistingId;
        var dto = CreateValidBuildingDto();

        _clientRepositoryMock
            .Setup(x => x.GetClientByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        // Act
        var result = await _service.CreateBuildingForClientAsync(clientId, dto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(ClientErrors.NotFound(clientId).Code, result.Error.Code);

        _geographyRepositoryMock.Verify(
            x => x.CityExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _buildingRepositoryMock.Verify(
            x => x.AddBuildingAsync(It.IsAny<Building>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreateBuildingForClientAsync_InvalidCityId_ReturnsValidationError(int cityId)
    {
        // Arrange
        const int clientId = 1;

        var dto = CreateValidBuildingDto() with
        {
            CityId = cityId
        };

        SetupExistingClient(clientId);

        // Act
        var result = await _service.CreateBuildingForClientAsync(clientId, dto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BuildingErrors.InvalidCityId.Code, result.Error.Code);

        _geographyRepositoryMock.Verify(
            x => x.CityExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _buildingRepositoryMock.Verify(
            x => x.AddBuildingAsync(It.IsAny<Building>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_NonExistingCity_ReturnsNotFound()
    {
        // Arrange
        const int clientId = 1;
        var dto = CreateValidBuildingDto();

        SetupExistingClient(clientId);

        _geographyRepositoryMock
            .Setup(x => x.CityExistsAsync(dto.CityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.CreateBuildingForClientAsync(clientId, dto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(BuildingErrors.CityNotFound(dto.CityId).Code, result.Error.Code);

        _buildingRepositoryMock.Verify(
            x => x.AddBuildingAsync(It.IsAny<Building>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_MissingStreet_ReturnsValidationError()
    {
        var dto = CreateValidBuildingDto() with { AddressStreet = "" };

        await AssertInvalidCreateAsync(dto, BuildingErrors.AddressStreetRequired);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_StreetTooLong_ReturnsValidationError()
    {
        var dto = CreateValidBuildingDto() with
        {
            AddressStreet = new string('A', BuildingConstraints.AddressStreetMaxLength + 1)
        };

        await AssertInvalidCreateAsync(dto, BuildingErrors.InvalidAddressStreetLength);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_MissingStreetNumber_ReturnsValidationError()
    {
        var dto = CreateValidBuildingDto() with { AddressStreetNumber = "" };

        await AssertInvalidCreateAsync(dto, BuildingErrors.AddressStreetNumberRequired);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_StreetNumberTooLong_ReturnsValidationError()
    {
        var dto = CreateValidBuildingDto() with
        {
            AddressStreetNumber = new string('1', BuildingConstraints.AddressStreetNumberMaxLength + 1)
        };

        await AssertInvalidCreateAsync(dto, BuildingErrors.InvalidAddressStreetNumberLength);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_ConstructionYearBelowMinimum_ReturnsValidationError()
    {
        var dto = CreateValidBuildingDto() with
        {
            ConstructionYear = BuildingConstraints.MinConstructionYear - 1
        };

        await AssertInvalidCreateAsync(dto, BuildingErrors.InvalidConstructionYear);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_ConstructionYearInFuture_ReturnsValidationError()
    {
        var dto = CreateValidBuildingDto() with
        {
            ConstructionYear = DateTime.UtcNow.Year + 1
        };

        await AssertInvalidCreateAsync(dto, BuildingErrors.InvalidConstructionYear);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_InvalidBuildingType_ReturnsValidationError()
    {
        var dto = CreateValidBuildingDto() with { BuildingType = (BuildingType)TestConstants.NonExistingId };

        await AssertInvalidCreateAsync(dto, BuildingErrors.InvalidBuildingType);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_NumberOfFloorsBelowMinimum_ReturnsValidationError()
    {
        var dto = CreateValidBuildingDto() with
        {
            NumberOfFloors = BuildingConstraints.MinNumberOfFloors - 1
        };

        await AssertInvalidCreateAsync(dto, BuildingErrors.InvalidNumberOfFloors);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_NumberOfFloorsAboveMaximum_ReturnsValidationError()
    {
        var dto = CreateValidBuildingDto() with
        {
            NumberOfFloors = BuildingConstraints.MaxNumberOfFloors + 1
        };

        await AssertInvalidCreateAsync(dto, BuildingErrors.InvalidNumberOfFloors);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_SurfaceAreaBelowMinimum_ReturnsValidationError()
    {
        var dto = CreateValidBuildingDto() with
        {
            SurfaceArea = BuildingConstraints.MinSurfaceArea - 0.01m
        };

        await AssertInvalidCreateAsync(dto, BuildingErrors.InvalidSurfaceArea);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_SurfaceAreaAboveMaximum_ReturnsValidationError()
    {
        var dto = CreateValidBuildingDto() with
        {
            SurfaceArea = BuildingConstraints.MaxSurfaceArea + 0.01m
        };

        await AssertInvalidCreateAsync(dto, BuildingErrors.InvalidSurfaceArea);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_SurfaceAreaWithTooManyDecimals_ReturnsValidationError()
    {
        var dto = CreateValidBuildingDto() with { SurfaceArea = 123.456m };

        await AssertInvalidCreateAsync(dto, BuildingErrors.InvalidSurfaceAreaScale);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_InsuredValueBelowMinimum_ReturnsValidationError()
    {
        var dto = CreateValidBuildingDto() with
        {
            InsuredValue = BuildingConstraints.MinInsuredValue - 0.01m
        };

        await AssertInvalidCreateAsync(dto, BuildingErrors.InvalidInsuredValue);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_InsuredValueAboveMaximum_ReturnsValidationError()
    {
        var dto = CreateValidBuildingDto() with
        {
            InsuredValue = BuildingConstraints.MaxInsuredValue + 0.01m
        };

        await AssertInvalidCreateAsync(dto, BuildingErrors.InvalidInsuredValue);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_InsuredValueWithTooManyDecimals_ReturnsValidationError()
    {
        var dto = CreateValidBuildingDto() with { InsuredValue = 1000.999m };

        await AssertInvalidCreateAsync(dto, BuildingErrors.InvalidInsuredValueScale);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_RiskIndicatorsTooLong_ReturnsValidationError()
    {
        var dto = CreateValidBuildingDto() with
        {
            RiskIndicators = new string('A', BuildingConstraints.RiskIndicatorsMaxLength + 1)
        };

        await AssertInvalidCreateAsync(dto, BuildingErrors.InvalidRiskIndicatorsLength);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_ValidBuilding_TrimsTextValues()
    {
        // Arrange
        const int clientId = 1;

        var dto = CreateValidBuildingDto() with
        {
            AddressStreet = "  Memorandumului  ",
            AddressStreetNumber = "  25A  ",
            RiskIndicators = "  Flood zone  "
        };

        SetupExistingClient(clientId);
        SetupExistingCity(dto.CityId);

        // Act
        var result = await _service.CreateBuildingForClientAsync(clientId, dto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Memorandumului", result.Value.AddressStreet);
        Assert.Equal("25A", result.Value.AddressStreetNumber);
        Assert.Equal("Flood zone", result.Value.RiskIndicators);
    }

    #endregion


    #region Update Building Tests

    [Fact]
    public async Task UpdateBuildingAsync_ValidBuilding_ReturnsUpdatedBuilding()
    {
        // Arrange
        var building = CreateBuildingEntity();
        var originalClientId = building.ClientId;
        var dto = CreateValidUpdateBuildingDto();

        SetupExistingCity(dto.CityId);

        _buildingRepositoryMock
            .Setup(x => x.GetBuildingForUpdateAsync(building.BuildingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(building);

        // Act
        var result = await _service.UpdateBuildingAsync(building.BuildingId, dto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(originalClientId, result.Value.ClientId);
        Assert.Equal(dto.CityId, result.Value.CityId);
        Assert.Equal(dto.AddressStreet, result.Value.AddressStreet);
        Assert.Equal(dto.AddressStreetNumber, result.Value.AddressStreetNumber);
        Assert.Equal(dto.ConstructionYear, result.Value.ConstructionYear);
        Assert.Equal(dto.BuildingType, result.Value.BuildingType);
        Assert.Equal(dto.NumberOfFloors, result.Value.NumberOfFloors);
        Assert.Equal(dto.SurfaceArea, result.Value.SurfaceArea);
        Assert.Equal(dto.InsuredValue, result.Value.InsuredValue);
        Assert.Equal(dto.RiskIndicators, result.Value.RiskIndicators);

        Assert.NotNull(building.ModifiedAt);

        _buildingRepositoryMock.Verify(
            x => x.SaveBuildingChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UpdateBuildingAsync_InvalidBuildingId_ReturnsValidationError(int buildingId)
    {
        // Arrange
        var dto = CreateValidUpdateBuildingDto();

        // Act
        var result = await _service.UpdateBuildingAsync(buildingId, dto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BuildingErrors.InvalidBuildingId.Code, result.Error.Code);

        _buildingRepositoryMock.Verify(
            x => x.GetBuildingForUpdateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateBuildingAsync_NonExistingBuilding_ReturnsNotFound()
    {
        // Arrange
        const int buildingId = TestConstants.NonExistingId;
        var dto = CreateValidUpdateBuildingDto();

        SetupExistingCity(dto.CityId);

        _buildingRepositoryMock
            .Setup(x => x.GetBuildingForUpdateAsync(buildingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Building?)null);

        // Act
        var result = await _service.UpdateBuildingAsync(buildingId, dto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(BuildingErrors.NotFound(buildingId).Code, result.Error.Code);

        _buildingRepositoryMock.Verify(
            x => x.SaveBuildingChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UpdateBuildingAsync_InvalidCityId_ReturnsValidationError(int cityId)
    {
        // Arrange
        var dto = CreateValidUpdateBuildingDto() with { CityId = cityId };

        // Act
        var result = await _service.UpdateBuildingAsync(1, dto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BuildingErrors.InvalidCityId.Code, result.Error.Code);

        _geographyRepositoryMock.Verify(
            x => x.CityExistsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _buildingRepositoryMock.Verify(
            x => x.SaveBuildingChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateBuildingAsync_NonExistingCity_ReturnsNotFound()
    {
        // Arrange
        var dto = CreateValidUpdateBuildingDto();

        _geographyRepositoryMock
            .Setup(x => x.CityExistsAsync(dto.CityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.UpdateBuildingAsync(1, dto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(BuildingErrors.CityNotFound(dto.CityId).Code, result.Error.Code);

        _buildingRepositoryMock.Verify(
            x => x.GetBuildingForUpdateAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _buildingRepositoryMock.Verify(
            x => x.SaveBuildingChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateBuildingAsync_MissingStreet_ReturnsValidationError()
    {
        var dto = CreateValidUpdateBuildingDto() with { AddressStreet = "" };

        await AssertInvalidUpdateAsync(dto, BuildingErrors.AddressStreetRequired);
    }

    [Fact]
    public async Task UpdateBuildingAsync_InvalidBuildingType_ReturnsValidationError()
    {
        var dto = CreateValidUpdateBuildingDto() with { BuildingType = (BuildingType)TestConstants.NonExistingId };

        await AssertInvalidUpdateAsync(dto, BuildingErrors.InvalidBuildingType);
    }

    [Fact]
    public async Task UpdateBuildingAsync_SurfaceAreaWithTooManyDecimals_ReturnsValidationError()
    {
        var dto = CreateValidUpdateBuildingDto() with { SurfaceArea = 123.456m };

        await AssertInvalidUpdateAsync(dto, BuildingErrors.InvalidSurfaceAreaScale);
    }

    [Fact]
    public async Task UpdateBuildingAsync_InsuredValueWithTooManyDecimals_ReturnsValidationError()
    {
        var dto = CreateValidUpdateBuildingDto() with { InsuredValue = 1000.999m };

        await AssertInvalidUpdateAsync(dto, BuildingErrors.InvalidInsuredValueScale);
    }

    [Fact]
    public async Task UpdateBuildingAsync_RiskIndicatorsTooLong_ReturnsValidationError()
    {
        var dto = CreateValidUpdateBuildingDto() with
        {
            RiskIndicators = new string('A', BuildingConstraints.RiskIndicatorsMaxLength + 1)
        };

        await AssertInvalidUpdateAsync(dto, BuildingErrors.InvalidRiskIndicatorsLength);
    }

    [Fact]
    public async Task UpdateBuildingAsync_ValidBuilding_TrimsTextValues()
    {
        // Arrange
        var building = CreateBuildingEntity();

        var dto = CreateValidUpdateBuildingDto() with
        {
            AddressStreet = "  Republicii  ",
            AddressStreetNumber = "  10A  ",
            RiskIndicators = "  Earthquake risk  "
        };

        SetupExistingCity(dto.CityId);

        _buildingRepositoryMock
            .Setup(x => x.GetBuildingForUpdateAsync(building.BuildingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(building);

        // Act
        var result = await _service.UpdateBuildingAsync(building.BuildingId, dto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Republicii", result.Value.AddressStreet);
        Assert.Equal("10A", result.Value.AddressStreetNumber);
        Assert.Equal("Earthquake risk", result.Value.RiskIndicators);
    }

    #endregion


    #region Helpers

    private void SetupExistingClient(int clientId)
    {
        _clientRepositoryMock
            .Setup(x => x.GetClientByIdAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateClientEntity(clientId));
    }

    private void SetupExistingCity(int cityId)
    {
        _geographyRepositoryMock
            .Setup(x => x.CityExistsAsync(cityId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private async Task AssertInvalidCreateAsync(CreateBuildingDto dto, Error expectedError)
    {
        const int clientId = 1;

        SetupExistingClient(clientId);

        var result = await _service.CreateBuildingForClientAsync(clientId, dto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(expectedError.Code, result.Error.Code);

        _buildingRepositoryMock.Verify(
            x => x.AddBuildingAsync(It.IsAny<Building>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private async Task AssertInvalidUpdateAsync(UpdateBuildingDto dto, Error expectedError)
    {
        var result = await _service.UpdateBuildingAsync(1, dto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(expectedError.Code, result.Error.Code);

        _buildingRepositoryMock.Verify(
            x => x.SaveBuildingChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static CreateBuildingDto CreateValidBuildingDto()
    {
        return new CreateBuildingDto(
            BuildingType.Residential,
            "Memorandumului",
            "25A",
            1,
            2015,
            4,
            185.50m,
            750_000.00m,
            "Flood zone");
    }

    private static UpdateBuildingDto CreateValidUpdateBuildingDto()
    {
        return new UpdateBuildingDto(
            BuildingType.Office,
            "Republicii",
            "10",
            2,
            2020,
            6,
            300.50m,
            1_000_000.00m,
            "Earthquake risk zone");
    }

    private static Building CreateBuildingEntity(int buildingId = 1, int clientId = 1)
    {
        return new Building
        {
            BuildingId = buildingId,
            ClientId = clientId,
            CityId = 1,
            AddressStreet = "Memorandumului",
            AddressStreetNumber = "25A",
            ConstructionYear = 2015,
            BuildingType = BuildingType.Residential,
            NumberOfFloors = 4,
            SurfaceArea = 185.50m,
            InsuredValue = 750_000.00m,
            RiskIndicators = "Flood zone",
            CreatedAt = DateTime.UtcNow
        };
    }

    private static Client CreateClientEntity(int clientId = 1)
    {
        return new Client
        {
            ClientId = clientId,
            ClientType = ClientType.Individual,
            Name = "John Doe",
            IdentificationNumber = "1980101223344",
            Email = "john@test.com",
            Phone = "0712345678",
            Address = "Cluj-Napoca",
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion
}