using FluentValidation;
using FluentValidation.Results;
using InsuranceApp.Application.Buildings;
using InsuranceApp.Application.Buildings.Commands;
using InsuranceApp.Application.Clients;
using InsuranceApp.Application.Common.Exceptions;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Geography;
using InsuranceApp.Application.Geography.Results;
using InsuranceApp.Domain.Buildings;
using InsuranceApp.Domain.Clients;
using InsuranceApp.UnitTests.Common;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace InsuranceApp.UnitTests.Application.Buildings;

public sealed class BuildingServiceTests
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IGeographyRepository _geographyRepository;
    private readonly IValidator<CreateBuildingCommand> _createValidator;
    private readonly IValidator<UpdateBuildingCommand> _updateValidator;
    private readonly IValidator<PageQuery> _pageValidator;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<BuildingService> _logger;

    private readonly BuildingService _buildingService;

    private const int CurrentYear = 2030;
    private readonly Guid _clientId = Guid.NewGuid();
    private readonly Guid _cityId = Guid.NewGuid();
    private readonly CityGeographyResult _geography;

    public BuildingServiceTests()
    {
        _buildingRepository = CreateBuildingRepositoryMock();
        _clientRepository = CreateClientRepositoryMock();
        _geographyRepository = CreateGeographyRepositoryMock();

        _createValidator = ValidatorMocks.CreatePassing<CreateBuildingCommand>();
        _updateValidator = ValidatorMocks.CreatePassing<UpdateBuildingCommand>();
        _pageValidator = ValidatorMocks.CreatePassing<PageQuery>();

        _timeProvider = CreateTimeProviderMock();
        _logger = Substitute.For<ILogger<BuildingService>>();

        _buildingService = new(_buildingRepository, _clientRepository, _geographyRepository,
            _createValidator, _updateValidator, _pageValidator, _timeProvider, _logger
        );

        var country = new CountryResult(Guid.NewGuid(), "Romania");
        var county = new CountyResult(Guid.NewGuid(), "Cluj", country.Id);
        var city = new CityResult(_cityId, "Cluj-Napoca", county.Id);
        _geography = new CityGeographyResult(city, county, country);
    }

    [Fact]
    public async Task CreateBuildingAsync_WhenBuiltInCurrentYear_SavesAndReturnsDetails()
    {
        // Arrange
        var command = CreateCommand();
        ArrangeExistingReferences();

        // Act
        var result = await _buildingService.CreateBuildingAsync(command, CancellationToken.None);

        // Assert
        var building = result.Building;
        Assert.NotEqual(Guid.Empty, building.Id);
        Assert.Equal(_clientId, building.ClientId);
        Assert.Equal(CurrentYear, building.ConstructionYear);
        Assert.Equal(command.Type, building.Type);
        Assert.Equal(command.NumberOfFloors, building.NumberOfFloors);
        Assert.Equal(command.SurfaceArea, building.SurfaceArea);
        Assert.Equal(command.InsuredValue, building.InsuredValue);
        Assert.Equal(command.Address!.Street, building.Address.Street);
        Assert.Equal(command.Address.Number, building.Address.Number);
        Assert.Equal(_cityId, building.Address.CityId);
        Assert.Same(_geography, result.Geography);

        await _createValidator.Received(1).ValidateAsync(
            Arg.Is<IValidationContext>(context =>
                ReferenceEquals(context.InstanceToValidate, command) && context.ThrowOnFailures
            ),
            CancellationToken.None
        );
        
        _timeProvider.Received(1).GetUtcNow();
        await _clientRepository.Received(1).ClientExistsByIdAsync(_clientId, CancellationToken.None);
        await _geographyRepository.Received(1).GetCityGeographyAsync(_cityId, CancellationToken.None);
        
        await _buildingRepository.Received(1).AddBuildingAsync(
            Arg.Is<Building>(received =>
                received.Id == building.Id
                && received.ClientId == _clientId
                && received.Address.Street == command.Address.Street
                && received.Address.Number == command.Address.Number
                && received.Address.CityId == _cityId
                && received.ConstructionYear == CurrentYear
                && received.Type == command.Type
                && received.NumberOfFloors == command.NumberOfFloors
                && received.SurfaceArea == command.SurfaceArea
                && received.InsuredValue == command.InsuredValue
            ),
            CancellationToken.None
        );
    }

    [Fact]
    public async Task CreateBuildingAsync_WhenClientIsMissing_ThrowsWithoutSaving()
    {
        // Arrange
        var command = CreateCommand();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _buildingService.CreateBuildingAsync(command, CancellationToken.None)
        );

        Assert.Equal(nameof(Client), exception.EntityName);
        Assert.Equal(_clientId, exception.EntityId);
        Assert.Empty(_buildingRepository.ReceivedCalls());
        Assert.Empty(_geographyRepository.ReceivedCalls());
    }

    [Fact]
    public async Task CreateBuildingAsync_WhenCityIsMissing_ThrowsValidationErrorWithoutSaving()
    {
        // Arrange
        var command = CreateCommand();

        _clientRepository.ClientExistsByIdAsync(_clientId, CancellationToken.None)
            .Returns(Task.FromResult(true));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _buildingService.CreateBuildingAsync(command, CancellationToken.None)
        );

        Assert.Equal("Address.CityId", Assert.Single(exception.Errors).PropertyName);
        Assert.Empty(_buildingRepository.ReceivedCalls());
    }

    [Fact]
    public async Task CreateBuildingAsync_WhenValidationFails_ThrowsWithoutRepositoryCalls()
    {
        // Arrange
        var command = CreateCommand();
        var failure = new ValidationException("Rejected by the validator mock.");
        
        _createValidator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ValidationResult>(failure));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _buildingService.CreateBuildingAsync(command, CancellationToken.None)
        );

        Assert.Same(failure, exception);
        Assert.Empty(_buildingRepository.ReceivedCalls());
        Assert.Empty(_clientRepository.ReceivedCalls());
        Assert.Empty(_geographyRepository.ReceivedCalls());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SaveOperations_WhenConstructionYearIsInFuture_ThrowBeforeRepositoryAccess(bool create)
    {
        // Arrange
        Func<Task> action = create
            ? () => _buildingService.CreateBuildingAsync(CreateCommand(CurrentYear + 1), CancellationToken.None)
            : () => _buildingService.UpdateBuildingAsync(UpdateCommand(Guid.NewGuid(), CurrentYear + 1), CancellationToken.None);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(action);

        Assert.Equal("ConstructionYear", Assert.Single(exception.Errors).PropertyName);
        _timeProvider.Received(1).GetUtcNow();
        Assert.Empty(_buildingRepository.ReceivedCalls());
        Assert.Empty(_clientRepository.ReceivedCalls());
        Assert.Empty(_geographyRepository.ReceivedCalls());
    }

    [Fact]
    public async Task UpdateBuildingAsync_WhenValid_ReplacesDetailsAndPreservesOwner()
    {
        // Arrange
        var building = CreateDomainBuilding();
        var command = UpdateCommand(building.Id);

        ArrangeExistingReferences();

        _buildingRepository.GetBuildingByIdAsync(building.Id, CancellationToken.None)
            .Returns(Task.FromResult<Building?>(building));

        // Act
        var result = await _buildingService.UpdateBuildingAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(command.BuildingId, result.Building.Id);
        Assert.Equal(_clientId, result.Building.ClientId);
        Assert.Equal(CurrentYear, result.Building.ConstructionYear);
        Assert.Equal(command.Type, result.Building.Type);
        Assert.Equal(command.NumberOfFloors, result.Building.NumberOfFloors);
        Assert.Equal(command.SurfaceArea, result.Building.SurfaceArea);
        Assert.Equal(command.InsuredValue, result.Building.InsuredValue);
        Assert.Equal(command.Address!.Street, result.Building.Address.Street);
        Assert.Equal(command.Address.Number, result.Building.Address.Number);
        Assert.Equal(_cityId, result.Building.Address.CityId);
        Assert.Same(_geography, result.Geography);

        await _updateValidator.Received(1).ValidateAsync(
            Arg.Is<IValidationContext>(context =>
                ReferenceEquals(context.InstanceToValidate, command) && context.ThrowOnFailures
            ),
            CancellationToken.None
        );

        _timeProvider.Received(1).GetUtcNow();

        await _buildingRepository.Received(1).UpdateBuildingAsync(
            Arg.Is<Building>(received =>
                received.Id == command.BuildingId
                && received.ClientId == _clientId
                && received.Address.CityId == _cityId
                && received.Address.Street == command.Address.Street
                && received.Address.Number == command.Address.Number
                && received.ConstructionYear == CurrentYear
                && received.Type == command.Type
                && received.NumberOfFloors == command.NumberOfFloors
                && received.SurfaceArea == command.SurfaceArea
                && received.InsuredValue == command.InsuredValue
            ),
            CancellationToken.None
        );

        await _buildingRepository.DidNotReceive()
            .AddBuildingAsync(Arg.Any<Building>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateBuildingAsync_WhenBuildingIsMissing_ThrowsWithoutSaving()
    {
        // Arrange
        var command = UpdateCommand(Guid.NewGuid());

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _buildingService.UpdateBuildingAsync(command, CancellationToken.None)
        );

        Assert.Equal(nameof(Building), exception.EntityName);
        Assert.Equal(command.BuildingId, exception.EntityId);
        
        await _buildingRepository.DidNotReceive()
            .UpdateBuildingAsync(Arg.Any<Building>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetBuildingDetailsByIdAsync_WhenBuildingExists_ReturnsDetailsWithGeography()
    {
        // Arrange
        var building = CreateDomainBuilding();
        ArrangeExistingReferences();
        
        _buildingRepository.GetBuildingByIdAsync(building.Id, CancellationToken.None)
            .Returns(Task.FromResult<Building?>(building));

        // Act
        var result = await _buildingService.GetBuildingDetailsByIdAsync(building.Id, CancellationToken.None);

        // Assert
        Assert.Equal(building.Id, result.Building.Id);
        Assert.Equal(_clientId, result.Building.ClientId);
        Assert.Equal(building.Address.Street, result.Building.Address.Street);
        Assert.Equal(building.Address.Number, result.Building.Address.Number);
        Assert.Equal(_cityId, result.Building.Address.CityId);
        Assert.Equal(building.ConstructionYear, result.Building.ConstructionYear);
        Assert.Equal(building.Type, result.Building.Type);
        Assert.Equal(building.NumberOfFloors, result.Building.NumberOfFloors);
        Assert.Equal(building.SurfaceArea, result.Building.SurfaceArea);
        Assert.Equal(building.InsuredValue, result.Building.InsuredValue);
        Assert.Same(_geography, result.Geography);

        await _geographyRepository.Received(1)
            .GetCityGeographyAsync(_cityId, CancellationToken.None);
    }

    [Fact]
    public async Task GetBuildingDetailsByIdAsync_WhenBuildingIsMissing_ThrowsNotFound()
    {
        // Arrange
        var buildingId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _buildingService.GetBuildingDetailsByIdAsync(buildingId, CancellationToken.None)
        );

        Assert.Equal(nameof(Building), exception.EntityName);
        Assert.Equal(buildingId, exception.EntityId);
        Assert.Empty(_geographyRepository.ReceivedCalls());
    }

    [Fact]
    public async Task GetBuildingsByClientIdAsync_WhenClientExists_ReturnsMappedPage()
    {
        // Arrange
        var building = CreateDomainBuilding();

        var pageNumber = 2;
        var pageSize = 10;
        var totalCount = 11L;
        var query = new PageQuery(pageNumber, pageSize);
        var page = new PagedResult<Building>([building], pageNumber, pageSize, totalCount);

        _clientRepository.ClientExistsByIdAsync(_clientId, CancellationToken.None)
            .Returns(Task.FromResult(true));
        
        _buildingRepository.GetBuildingsByClientIdAsync(_clientId, query, CancellationToken.None)
            .Returns(Task.FromResult(page));

        // Act
        var result = await _buildingService.GetBuildingsByClientIdAsync(_clientId, query, CancellationToken.None);

        // Assert
        var item = Assert.Single(result.Items);
        Assert.Equal(building.Id, item.Id);
        Assert.Equal(_clientId, item.ClientId);
        Assert.Equal(_cityId, item.Address.CityId);
        Assert.Equal(pageNumber, result.PageNumber);
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal(totalCount, result.TotalCount);

        await _pageValidator.Received(1).ValidateAsync(
            Arg.Is<IValidationContext>(context =>
                ReferenceEquals(context.InstanceToValidate, query) && context.ThrowOnFailures
            ),
            CancellationToken.None
        );

        await _clientRepository.Received(1)
            .ClientExistsByIdAsync(_clientId, CancellationToken.None);

        await _buildingRepository.Received(1)
            .GetBuildingsByClientIdAsync(_clientId, query, CancellationToken.None);

        Assert.Empty(_geographyRepository.ReceivedCalls());
    }

    [Fact]
    public async Task GetBuildingsByClientIdAsync_WhenClientIsMissing_ThrowsWithoutListingBuildings()
    {
        // Arrange
        var query = new PageQuery();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _buildingService.GetBuildingsByClientIdAsync(_clientId, query, CancellationToken.None)
        );

        Assert.Equal(nameof(Client), exception.EntityName);
        Assert.Equal(_clientId, exception.EntityId);
        Assert.Empty(_buildingRepository.ReceivedCalls());
    }

    private static IBuildingRepository CreateBuildingRepositoryMock()
    {
        var repository = Substitute.For<IBuildingRepository>();

        repository.GetBuildingByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Building?>(null));

        repository.AddBuildingAsync(Arg.Any<Building>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        repository.UpdateBuildingAsync(Arg.Any<Building>(), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        repository.ClearReceivedCalls();

        return repository;
    }

    private static IClientRepository CreateClientRepositoryMock()
    {
        var repository = Substitute.For<IClientRepository>();

        repository.ClientExistsByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(false));

        repository.ClearReceivedCalls();

        return repository;
    }

    private static IGeographyRepository CreateGeographyRepositoryMock()
    {
        var repository = Substitute.For<IGeographyRepository>();

        repository.GetCityGeographyAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<CityGeographyResult?>(null));

        repository.ClearReceivedCalls();

        return repository;
    }

    private static TimeProvider CreateTimeProviderMock()
    {
        var provider = Substitute.For<TimeProvider>();

        provider.GetUtcNow()
            .Returns(new DateTimeOffset(CurrentYear, 1, 1, 0, 0, 0, TimeSpan.Zero));

        provider.ClearReceivedCalls();

        return provider;
    }

    private void ArrangeExistingReferences()
    {
        _clientRepository.ClientExistsByIdAsync(_clientId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(true));

        _geographyRepository.GetCityGeographyAsync(_cityId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<CityGeographyResult?>(_geography));
    }

    private CreateBuildingCommand CreateCommand(int constructionYear = CurrentYear)
    {
        return new(
            ClientId: _clientId,
            Type: BuildingType.Residential,
            Address: new(_cityId, "Example Street", "12A"),
            ConstructionYear: constructionYear,
            NumberOfFloors: 2,
            SurfaceArea: 125.50m,
            InsuredValue: 250000m
        );
    }

    private UpdateBuildingCommand UpdateCommand(Guid buildingId, int constructionYear = CurrentYear)
    {
        return new(
            BuildingId: buildingId,
            Type: BuildingType.Office,
            Address: new(_cityId, "Updated Street", "8B"),
            ConstructionYear: constructionYear,
            NumberOfFloors: 3,
            SurfaceArea: 200m,
            InsuredValue: 500000m
        );
    }

    private Building CreateDomainBuilding()
    {
        return new(
            id: Guid.NewGuid(),
            clientId: _clientId,
            type: BuildingType.Residential,
            address: new(_cityId, "Original Street", "10"),
            constructionYear: 2005,
            numberOfFloors: 2,
            surfaceArea: 125.50m,
            insuredValue: 250000m
        );
    }
}
