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

    private readonly Guid _clientId;
    private readonly Guid _buildingId;
    private readonly Guid _cityId;
    private readonly Guid _buildingTypeId;

    private readonly CreateBuildingDto _validCreateDto;
    private readonly UpdateBuildingDto _validUpdateDto;
    private readonly Building _existingBuilding;
    private readonly Client _existingClient;

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

        _clientId = Guid.NewGuid();
        _buildingId = Guid.NewGuid();
        _cityId = Guid.NewGuid();
        _buildingTypeId = Guid.NewGuid();

        _validCreateDto = new CreateBuildingDto(
            _buildingTypeId,
            "Memorandumului",
            "25A",
            _cityId,
            2015,
            4,
            185.50m,
            750_000.00m,
            "Flood zone");

        _validUpdateDto = new UpdateBuildingDto(
            _buildingTypeId,
            "Republicii",
            "10",
            _cityId,
            2020,
            6,
            300.50m,
            1_000_000.00m,
            "Earthquake risk zone");

        _existingClient = new Client
        {
            ClientId = _clientId,
            ClientType = ClientType.Individual,
            Name = "John Doe",
            IdentificationNumber = "1980101223344",
            Email = "john@test.com",
            Phone = "0712345678",
            Address = "Cluj-Napoca",
            CreatedAt = DateTime.UtcNow
        };

        _existingBuilding = new Building
        {
            BuildingId = _buildingId,
            ClientId = _clientId,
            CityId = _cityId,
            BuildingTypeId = _buildingTypeId,
            AddressStreet = "Memorandumului",
            AddressStreetNumber = "25A",
            ConstructionYear = 2015,
            NumberOfFloors = 4,
            SurfaceArea = 185.50m,
            InsuredValue = 750_000.00m,
            RiskIndicators = "Flood zone",
            CreatedAt = DateTime.UtcNow
        };
    }

    #region Get Building By Id Tests

    [Fact]
    public async Task GetBuildingByIdAsync_ExistingBuilding_ReturnsSuccess()
    {
        _buildingRepositoryMock
            .Setup(x => x.GetBuildingByIdAsync(
                _buildingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingBuilding);

        var result = await _service.GetBuildingByIdAsync(
            _buildingId,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(_buildingId, result.Value.BuildingId);
        Assert.Equal(_clientId, result.Value.ClientId);
        Assert.Equal(_cityId, result.Value.CityId);
        Assert.Equal(_buildingTypeId, result.Value.BuildingTypeId);
        Assert.Equal(_existingBuilding.AddressStreet, result.Value.AddressStreet);
        Assert.Equal(_existingBuilding.AddressStreetNumber, result.Value.AddressStreetNumber);
        Assert.Equal(_existingBuilding.ConstructionYear, result.Value.ConstructionYear);
        Assert.Equal(_existingBuilding.NumberOfFloors, result.Value.NumberOfFloors);
        Assert.Equal(_existingBuilding.SurfaceArea, result.Value.SurfaceArea);
        Assert.Equal(_existingBuilding.InsuredValue, result.Value.InsuredValue);
        Assert.Equal(_existingBuilding.RiskIndicators, result.Value.RiskIndicators);
    }

    [Fact]
    public async Task GetBuildingByIdAsync_EmptyBuildingId_ReturnsValidationError()
    {
        var result = await _service.GetBuildingByIdAsync(
            Guid.Empty,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BuildingErrors.InvalidBuildingId.Code, result.Error.Code);

        _buildingRepositoryMock.Verify(
            x => x.GetBuildingByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetBuildingByIdAsync_NonExistingBuilding_ReturnsNotFound()
    {
        var buildingId = TestConstants.NonExistingId;

        _buildingRepositoryMock
            .Setup(x => x.GetBuildingByIdAsync(
                buildingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Building?)null);

        var result = await _service.GetBuildingByIdAsync(
            buildingId,
            CancellationToken.None);

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
        var secondBuilding = new Building
        {
            BuildingId = Guid.NewGuid(),
            ClientId = _clientId,
            CityId = _cityId,
            BuildingTypeId = _buildingTypeId,
            AddressStreet = "Republicii",
            AddressStreetNumber = "10",
            ConstructionYear = 2020,
            NumberOfFloors = 6,
            SurfaceArea = 300.50m,
            InsuredValue = 1_000_000.00m,
            RiskIndicators = "Earthquake risk zone",
            CreatedAt = DateTime.UtcNow
        };

        var buildings = new List<Building>
        {
            _existingBuilding,
            secondBuilding
        };

        SetupExistingClient();

        _buildingRepositoryMock
            .Setup(x => x.GetBuildingsByClientAsync(
                _clientId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(buildings);

        var result = await _service.GetBuildingsByClientAsync(
            _clientId,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);

        Assert.All(
            result.Value,
            building => Assert.Equal(_clientId, building.ClientId));

        _buildingRepositoryMock.Verify(
            x => x.GetBuildingsByClientAsync(
                _clientId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetBuildingsByClientAsync_ClientWithoutBuildings_ReturnsEmptyList()
    {
        SetupExistingClient();

        _buildingRepositoryMock
            .Setup(x => x.GetBuildingsByClientAsync(
                _clientId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var result = await _service.GetBuildingsByClientAsync(
            _clientId,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task GetBuildingsByClientAsync_EmptyClientId_ReturnsValidationError()
    {
        var result = await _service.GetBuildingsByClientAsync(
            Guid.Empty,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BuildingErrors.InvalidClientId.Code, result.Error.Code);

        _clientRepositoryMock.Verify(
            x => x.GetClientByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _buildingRepositoryMock.Verify(
            x => x.GetBuildingsByClientAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetBuildingsByClientAsync_NonExistingClient_ReturnsNotFound()
    {
        var clientId = TestConstants.NonExistingId;

        _clientRepositoryMock
            .Setup(x => x.GetClientByIdAsync(
                clientId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var result = await _service.GetBuildingsByClientAsync(
            clientId,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(ClientErrors.NotFound(clientId).Code, result.Error.Code);

        _buildingRepositoryMock.Verify(
            x => x.GetBuildingsByClientAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Create Building Tests

    [Fact]
    public async Task CreateBuildingForClientAsync_ValidBuilding_ReturnsSuccess()
    {
        SetupExistingClient();
        SetupExistingCity();
        SetupExistingBuildingType();

        var result = await _service.CreateBuildingForClientAsync(
            _clientId,
            _validCreateDto,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(_clientId, result.Value.ClientId);
        Assert.Equal(_validCreateDto.CityId, result.Value.CityId);
        Assert.Equal(_validCreateDto.BuildingTypeId, result.Value.BuildingTypeId);
        Assert.Equal(_validCreateDto.AddressStreet, result.Value.AddressStreet);
        Assert.Equal(_validCreateDto.AddressStreetNumber, result.Value.AddressStreetNumber);
        Assert.Equal(_validCreateDto.ConstructionYear, result.Value.ConstructionYear);
        Assert.Equal(_validCreateDto.NumberOfFloors, result.Value.NumberOfFloors);
        Assert.Equal(_validCreateDto.SurfaceArea, result.Value.SurfaceArea);
        Assert.Equal(_validCreateDto.InsuredValue, result.Value.InsuredValue);
        Assert.Equal(_validCreateDto.RiskIndicators, result.Value.RiskIndicators);

        _buildingRepositoryMock.Verify(
            x => x.AddBuildingAsync(
                It.Is<Building>(building =>
                    building.ClientId == _clientId &&
                    building.CityId == _cityId &&
                    building.BuildingTypeId == _buildingTypeId &&
                    building.AddressStreet == _validCreateDto.AddressStreet &&
                    building.AddressStreetNumber == _validCreateDto.AddressStreetNumber &&
                    building.SurfaceArea == _validCreateDto.SurfaceArea &&
                    building.InsuredValue == _validCreateDto.InsuredValue),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_EmptyClientId_ReturnsValidationError()
    {
        var result = await _service.CreateBuildingForClientAsync(
            Guid.Empty,
            _validCreateDto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(BuildingErrors.InvalidClientId.Code, result.Error.Code);

        _clientRepositoryMock.Verify(
            x => x.GetClientByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _buildingRepositoryMock.Verify(
            x => x.AddBuildingAsync(
                It.IsAny<Building>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_NonExistingClient_ReturnsNotFound()
    {
        var clientId = TestConstants.NonExistingId;

        _clientRepositoryMock
            .Setup(x => x.GetClientByIdAsync(
                clientId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        var result = await _service.CreateBuildingForClientAsync(
            clientId,
            _validCreateDto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal(ClientErrors.NotFound(clientId).Code, result.Error.Code);

        _geographyRepositoryMock.Verify(
            x => x.CityExistsAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _buildingRepositoryMock.Verify(
            x => x.BuildingTypeExistsAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _buildingRepositoryMock.Verify(
            x => x.AddBuildingAsync(
                It.IsAny<Building>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_EmptyCityId_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            CityId = Guid.Empty
        };

        SetupExistingClient();

        var result = await _service.CreateBuildingForClientAsync(
            _clientId,
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal(BuildingErrors.InvalidCityId.Code, result.Error.Code);

        _geographyRepositoryMock.Verify(
            x => x.CityExistsAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _buildingRepositoryMock.Verify(
            x => x.AddBuildingAsync(
                It.IsAny<Building>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_NonExistingCity_ReturnsNotFound()
    {
        SetupExistingClient();

        _geographyRepositoryMock
            .Setup(x => x.CityExistsAsync(
                _cityId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.CreateBuildingForClientAsync(
            _clientId,
            _validCreateDto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal(
            BuildingErrors.CityNotFound(_cityId).Code,
            result.Error.Code);

        _buildingRepositoryMock.Verify(
            x => x.AddBuildingAsync(
                It.IsAny<Building>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_EmptyBuildingTypeId_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            BuildingTypeId = Guid.Empty
        };

        SetupExistingClient();
        SetupExistingCity();

        var result = await _service.CreateBuildingForClientAsync(
            _clientId,
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal(BuildingErrors.InvalidBuildingType.Code, result.Error.Code);

        _buildingRepositoryMock.Verify(
            x => x.BuildingTypeExistsAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _buildingRepositoryMock.Verify(
            x => x.AddBuildingAsync(
                It.IsAny<Building>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_NonExistingBuildingType_ReturnsNotFound()
    {
        SetupExistingClient();
        SetupExistingCity();

        _buildingRepositoryMock
            .Setup(x => x.BuildingTypeExistsAsync(
                _buildingTypeId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.CreateBuildingForClientAsync(
            _clientId,
            _validCreateDto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal(
            BuildingErrors.BuildingTypeNotFound(_buildingTypeId).Code,
            result.Error.Code);

        _buildingRepositoryMock.Verify(
            x => x.AddBuildingAsync(
                It.IsAny<Building>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_MissingStreet_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            AddressStreet = ""
        };

        await AssertInvalidCreateAsync(
            dto,
            BuildingErrors.AddressStreetRequired);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_StreetTooLong_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            AddressStreet =
                new string('A', BuildingConstraints.AddressStreetMaxLength + 1)
        };

        await AssertInvalidCreateAsync(
            dto,
            BuildingErrors.InvalidAddressStreetLength);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_MissingStreetNumber_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            AddressStreetNumber = ""
        };

        await AssertInvalidCreateAsync(
            dto,
            BuildingErrors.AddressStreetNumberRequired);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_StreetNumberTooLong_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            AddressStreetNumber =
                new string('1', BuildingConstraints.AddressStreetNumberMaxLength + 1)
        };

        await AssertInvalidCreateAsync(
            dto,
            BuildingErrors.InvalidAddressStreetNumberLength);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_ConstructionYearBelowMinimum_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            ConstructionYear = BuildingConstraints.MinConstructionYear - 1
        };

        await AssertInvalidCreateAsync(
            dto,
            BuildingErrors.InvalidConstructionYear);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_ConstructionYearInFuture_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            ConstructionYear = DateTime.UtcNow.Year + 1
        };

        await AssertInvalidCreateAsync(
            dto,
            BuildingErrors.InvalidConstructionYear);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_NumberOfFloorsBelowMinimum_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            NumberOfFloors = BuildingConstraints.MinNumberOfFloors - 1
        };

        await AssertInvalidCreateAsync(
            dto,
            BuildingErrors.InvalidNumberOfFloors);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_NumberOfFloorsAboveMaximum_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            NumberOfFloors = BuildingConstraints.MaxNumberOfFloors + 1
        };

        await AssertInvalidCreateAsync(
            dto,
            BuildingErrors.InvalidNumberOfFloors);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_SurfaceAreaBelowMinimum_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            SurfaceArea = BuildingConstraints.MinSurfaceArea - 0.01m
        };

        await AssertInvalidCreateAsync(
            dto,
            BuildingErrors.InvalidSurfaceArea);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_SurfaceAreaAboveMaximum_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            SurfaceArea = BuildingConstraints.MaxSurfaceArea + 0.01m
        };

        await AssertInvalidCreateAsync(
            dto,
            BuildingErrors.InvalidSurfaceArea);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_SurfaceAreaWithTooManyDecimals_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            SurfaceArea = 123.456m
        };

        await AssertInvalidCreateAsync(
            dto,
            BuildingErrors.InvalidSurfaceAreaScale);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_InsuredValueBelowMinimum_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            InsuredValue = BuildingConstraints.MinInsuredValue - 0.01m
        };

        await AssertInvalidCreateAsync(
            dto,
            BuildingErrors.InvalidInsuredValue);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_InsuredValueAboveMaximum_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            InsuredValue = BuildingConstraints.MaxInsuredValue + 0.01m
        };

        await AssertInvalidCreateAsync(
            dto,
            BuildingErrors.InvalidInsuredValue);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_InsuredValueWithTooManyDecimals_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            InsuredValue = 1000.999m
        };

        await AssertInvalidCreateAsync(
            dto,
            BuildingErrors.InvalidInsuredValueScale);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_RiskIndicatorsTooLong_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            RiskIndicators =
                new string('A', BuildingConstraints.RiskIndicatorsMaxLength + 1)
        };

        await AssertInvalidCreateAsync(
            dto,
            BuildingErrors.InvalidRiskIndicatorsLength);
    }

    [Fact]
    public async Task CreateBuildingForClientAsync_ValidBuilding_TrimsTextValues()
    {
        var dto = _validCreateDto with
        {
            AddressStreet = "  Memorandumului  ",
            AddressStreetNumber = "  25A  ",
            RiskIndicators = "  Flood zone  "
        };

        SetupExistingClient();
        SetupExistingCity();
        SetupExistingBuildingType();

        var result = await _service.CreateBuildingForClientAsync(
            _clientId,
            dto,
            CancellationToken.None);

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
        var originalClientId = _existingBuilding.ClientId;

        SetupExistingCity();
        SetupExistingBuildingType();
        SetupExistingBuildingForUpdate();

        var result = await _service.UpdateBuildingAsync(
            _buildingId,
            _validUpdateDto,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(originalClientId, result.Value.ClientId);
        Assert.Equal(_validUpdateDto.CityId, result.Value.CityId);
        Assert.Equal(_validUpdateDto.BuildingTypeId, result.Value.BuildingTypeId);
        Assert.Equal(_validUpdateDto.AddressStreet, result.Value.AddressStreet);
        Assert.Equal(_validUpdateDto.AddressStreetNumber, result.Value.AddressStreetNumber);
        Assert.Equal(_validUpdateDto.ConstructionYear, result.Value.ConstructionYear);
        Assert.Equal(_validUpdateDto.NumberOfFloors, result.Value.NumberOfFloors);
        Assert.Equal(_validUpdateDto.SurfaceArea, result.Value.SurfaceArea);
        Assert.Equal(_validUpdateDto.InsuredValue, result.Value.InsuredValue);
        Assert.Equal(_validUpdateDto.RiskIndicators, result.Value.RiskIndicators);

        Assert.NotNull(_existingBuilding.ModifiedAt);

        _buildingRepositoryMock.Verify(
            x => x.SaveBuildingChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateBuildingAsync_EmptyBuildingId_ReturnsValidationError()
    {
        var result = await _service.UpdateBuildingAsync(
            Guid.Empty,
            _validUpdateDto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal(BuildingErrors.InvalidBuildingId.Code, result.Error.Code);

        _buildingRepositoryMock.Verify(
            x => x.GetBuildingForUpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateBuildingAsync_NonExistingBuilding_ReturnsNotFound()
    {
        var buildingId = TestConstants.NonExistingId;

        SetupExistingCity();
        SetupExistingBuildingType();

        _buildingRepositoryMock
            .Setup(x => x.GetBuildingForUpdateAsync(
                buildingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Building?)null);

        var result = await _service.UpdateBuildingAsync(
            buildingId,
            _validUpdateDto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal(BuildingErrors.NotFound(buildingId).Code, result.Error.Code);

        _buildingRepositoryMock.Verify(
            x => x.SaveBuildingChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateBuildingAsync_EmptyCityId_ReturnsValidationError()
    {
        var dto = _validUpdateDto with
        {
            CityId = Guid.Empty
        };

        var result = await _service.UpdateBuildingAsync(
            _buildingId,
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
        Assert.Equal(BuildingErrors.InvalidCityId.Code, result.Error.Code);

        _geographyRepositoryMock.Verify(
            x => x.CityExistsAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _buildingRepositoryMock.Verify(
            x => x.SaveBuildingChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateBuildingAsync_NonExistingCity_ReturnsNotFound()
    {
        _geographyRepositoryMock
            .Setup(x => x.CityExistsAsync(
                _cityId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.UpdateBuildingAsync(
            _buildingId,
            _validUpdateDto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal(
            BuildingErrors.CityNotFound(_cityId).Code,
            result.Error.Code);

        _buildingRepositoryMock.Verify(
            x => x.GetBuildingForUpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _buildingRepositoryMock.Verify(
            x => x.SaveBuildingChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateBuildingAsync_EmptyBuildingTypeId_ReturnsValidationError()
    {
        var dto = _validUpdateDto with
        {
            BuildingTypeId = Guid.Empty
        };

        SetupExistingCity();

        var result = await _service.UpdateBuildingAsync(
            _buildingId,
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(
            BuildingErrors.InvalidBuildingType.Code,
            result.Error.Code);

        _buildingRepositoryMock.Verify(
            x => x.BuildingTypeExistsAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _buildingRepositoryMock.Verify(
            x => x.GetBuildingForUpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _buildingRepositoryMock.Verify(
            x => x.SaveBuildingChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateBuildingAsync_NonExistingBuildingType_ReturnsNotFound()
    {
        SetupExistingCity();

        _buildingRepositoryMock
            .Setup(x => x.BuildingTypeExistsAsync(
                _buildingTypeId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.UpdateBuildingAsync(
            _buildingId,
            _validUpdateDto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal(
            BuildingErrors.BuildingTypeNotFound(_buildingTypeId).Code,
            result.Error.Code);

        _buildingRepositoryMock.Verify(
            x => x.GetBuildingForUpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _buildingRepositoryMock.Verify(
            x => x.SaveBuildingChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateBuildingAsync_MissingStreet_ReturnsValidationError()
    {
        var dto = _validUpdateDto with
        {
            AddressStreet = ""
        };

        await AssertInvalidUpdateAsync(
            dto,
            BuildingErrors.AddressStreetRequired);
    }

    [Fact]
    public async Task UpdateBuildingAsync_SurfaceAreaWithTooManyDecimals_ReturnsValidationError()
    {
        var dto = _validUpdateDto with
        {
            SurfaceArea = 123.456m
        };

        await AssertInvalidUpdateAsync(
            dto,
            BuildingErrors.InvalidSurfaceAreaScale);
    }

    [Fact]
    public async Task UpdateBuildingAsync_InsuredValueWithTooManyDecimals_ReturnsValidationError()
    {
        var dto = _validUpdateDto with
        {
            InsuredValue = 1000.999m
        };

        await AssertInvalidUpdateAsync(
            dto,
            BuildingErrors.InvalidInsuredValueScale);
    }

    [Fact]
    public async Task UpdateBuildingAsync_RiskIndicatorsTooLong_ReturnsValidationError()
    {
        var dto = _validUpdateDto with
        {
            RiskIndicators =
                new string('A', BuildingConstraints.RiskIndicatorsMaxLength + 1)
        };

        await AssertInvalidUpdateAsync(
            dto,
            BuildingErrors.InvalidRiskIndicatorsLength);
    }

    [Fact]
    public async Task UpdateBuildingAsync_ValidBuilding_TrimsTextValues()
    {
        var dto = _validUpdateDto with
        {
            AddressStreet = "  Republicii  ",
            AddressStreetNumber = "  10A  ",
            RiskIndicators = "  Earthquake risk  "
        };

        SetupExistingCity();
        SetupExistingBuildingType();
        SetupExistingBuildingForUpdate();

        var result = await _service.UpdateBuildingAsync(
            _buildingId,
            dto,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("Republicii", result.Value.AddressStreet);
        Assert.Equal("10A", result.Value.AddressStreetNumber);
        Assert.Equal("Earthquake risk", result.Value.RiskIndicators);
    }

    #endregion

    #region Helpers

    private void SetupExistingClient()
    {
        _clientRepositoryMock
            .Setup(x => x.GetClientByIdAsync(
                _clientId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingClient);
    }

    private void SetupExistingCity()
    {
        _geographyRepositoryMock
            .Setup(x => x.CityExistsAsync(
                _cityId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private void SetupExistingBuildingType()
    {
        _buildingRepositoryMock
            .Setup(x => x.BuildingTypeExistsAsync(
                _buildingTypeId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private void SetupExistingBuildingForUpdate()
    {
        _buildingRepositoryMock
            .Setup(x => x.GetBuildingForUpdateAsync(
                _buildingId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingBuilding);
    }

    private async Task AssertInvalidCreateAsync(
        CreateBuildingDto dto,
        Error expectedError)
    {
        SetupExistingClient();

        var result = await _service.CreateBuildingForClientAsync(
            _clientId,
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(expectedError.Code, result.Error.Code);

        _buildingRepositoryMock.Verify(
            x => x.AddBuildingAsync(
                It.IsAny<Building>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private async Task AssertInvalidUpdateAsync(
        UpdateBuildingDto dto,
        Error expectedError)
    {
        var result = await _service.UpdateBuildingAsync(
            _buildingId,
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(expectedError.Code, result.Error.Code);

        _buildingRepositoryMock.Verify(
            x => x.SaveBuildingChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion
}
