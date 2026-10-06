using FluentValidation;
using FluentValidation.Results;
using InsuranceApp.Application.Common.Exceptions;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.RiskFactors;
using InsuranceApp.Application.RiskFactors.Commands;
using InsuranceApp.Domain.Buildings;
using InsuranceApp.Domain.RiskFactors;
using InsuranceApp.UnitTests.Common;
using InsuranceApp.UnitTests.TestData.RiskFactors;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace InsuranceApp.UnitTests.Application.RiskFactors;

public sealed class RiskFactorServiceTests
{
    private readonly IRiskFactorRepository _riskFactorRepository;
    private readonly IValidator<CreateRiskFactorCommand> _createValidator;
    private readonly IValidator<UpdateRiskFactorCommand> _updateValidator;
    private readonly IValidator<PageQuery> _pageValidator;
    private readonly ILogger<RiskFactorService> _logger;

    private readonly RiskFactorService _riskFactorService;

    public RiskFactorServiceTests()
    {
        _riskFactorRepository = Substitute.For<IRiskFactorRepository>();

        _createValidator = ValidatorMocks.CreatePassing<CreateRiskFactorCommand>();
        _updateValidator = ValidatorMocks.CreatePassing<UpdateRiskFactorCommand>();
        _pageValidator = ValidatorMocks.CreatePassing<PageQuery>();

        _logger = Substitute.For<ILogger<RiskFactorService>>();

        _riskFactorService = new(_riskFactorRepository, _createValidator, _updateValidator, _pageValidator, _logger);
    }

    [Theory]
    [InlineData(RiskFactorLevel.Country)]
    [InlineData(RiskFactorLevel.County)]
    [InlineData(RiskFactorLevel.City)]
    [InlineData(RiskFactorLevel.BuildingType)]
    public async Task CreateRiskFactorAsync_ValidTarget_SavesRiskFactorAndReturnsResult(RiskFactorLevel level)
    {
        // Arrange
        var command = CreateCommand(level);
        var expectedTarget = RiskTargetTestData.Create(level);

        // Act
        var result = await _riskFactorService.CreateRiskFactorAsync(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(command.Level, result.Level);
        Assert.Equal(command.CountryId, result.CountryId);
        Assert.Equal(command.CountyId, result.CountyId);
        Assert.Equal(command.CityId, result.CityId);
        Assert.Equal(command.BuildingType, result.BuildingType);
        Assert.Equal(command.AdjustmentPercentage, result.AdjustmentPercentage);
        Assert.Equal(command.IsActive, result.IsActive);

        await _createValidator.Received(1).ValidateAsync(
            Arg.Is<IValidationContext>(context =>
                ReferenceEquals(context.InstanceToValidate, command) && context.ThrowOnFailures
            ),
            CancellationToken.None
        );

        await _riskFactorRepository.Received(1).AddRiskFactorAsync(
            Arg.Is<RiskFactorConfiguration>(received =>
                received.Id == result.Id
                && received.Target == expectedTarget
                && received.AdjustmentPercentage == command.AdjustmentPercentage
                && received.IsActive == command.IsActive
            ),
            CancellationToken.None
        );
    }

    [Fact]
    public async Task CreateRiskFactorAsync_ValidationFails_DoesNotAccessRepository()
    {
        // Arrange
        var command = CreateCommand();
        var exception = new ValidationException("Rejected by the validator mock.");

        _createValidator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ValidationResult>(exception));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _riskFactorService.CreateRiskFactorAsync(command, CancellationToken.None)
        );
        
        Assert.Empty(_riskFactorRepository.ReceivedCalls());
    }

    [Fact]
    public async Task UpdateRiskFactorAsync_ValidCommand_PreservesIdAndUpdatesAllFields()
    {
        // Arrange
        var riskFactor = RiskFactorConfigurationTestData.Create();
        
        _riskFactorRepository.GetRiskFactorByIdAsync(riskFactor.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<RiskFactorConfiguration?>(riskFactor));

        var level = RiskFactorLevel.County;
        var countyId = Guid.NewGuid();
        var adjustmentPercentage = -3m;
        var isActive = false;

        var command = UpdateCommand(
            riskFactorId: riskFactor.Id,
            level: level,
            countyId: countyId,
            adjustmentPercentage: adjustmentPercentage,
            isActive: isActive
        );

        var expectedTarget = RiskTargetTestData.CreateCountyTarget(countyId);

        // Act
        var result = await _riskFactorService.UpdateRiskFactorAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(riskFactor.Id, result.Id);
        Assert.Equal(level, result.Level);
        Assert.Equal(countyId, result.CountyId);
        Assert.Equal(expectedTarget, riskFactor.Target);
        Assert.Null(result.CountryId);
        Assert.Null(result.CityId);
        Assert.Null(result.BuildingType);
        Assert.Equal(adjustmentPercentage, result.AdjustmentPercentage);
        Assert.Equal(isActive, result.IsActive);

        await _updateValidator.Received(1).ValidateAsync(
            Arg.Is<IValidationContext>(context =>
                ReferenceEquals(context.InstanceToValidate, command) && context.ThrowOnFailures
            ),
            CancellationToken.None
        );
        
        await _riskFactorRepository.Received(1).UpdateRiskFactorAsync(riskFactor, CancellationToken.None);
    }

    [Fact]
    public async Task UpdateRiskFactorAsync_ValidationFails_DoesNotAccessRepository()
    {
        // Arrange
        var command = UpdateCommand(riskFactorId: Guid.NewGuid());
        var exception = new ValidationException("Rejected by the validator mock.");

        _updateValidator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ValidationResult>(exception));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _riskFactorService.UpdateRiskFactorAsync(command, CancellationToken.None)
        );
        
        Assert.Empty(_riskFactorRepository.ReceivedCalls());
    }

    [Fact]
    public async Task GetRiskFactorByIdAsync_ExistingId_ReturnsDetails()
    {
        // Arrange
        var riskFactor = RiskFactorConfigurationTestData.Create();

        _riskFactorRepository.GetRiskFactorByIdAsync(riskFactor.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<RiskFactorConfiguration?>(riskFactor));

        // Act
        var result = await _riskFactorService.GetRiskFactorByIdAsync(riskFactor.Id, CancellationToken.None);

        // Assert
        Assert.Equal(riskFactor.Id, result.Id);
        Assert.Equal(riskFactor.Target.Level, result.Level);
        Assert.Equal(riskFactor.AdjustmentPercentage, result.AdjustmentPercentage);
        Assert.Equal(riskFactor.IsActive, result.IsActive);
    }

    [Fact]
    public async Task GetRiskFactorByIdAsync_UnknownId_ThrowsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _riskFactorService.GetRiskFactorByIdAsync(id, CancellationToken.None)
        );
    }

    [Fact]
    public async Task GetRiskFactorByIdAsync_EmptyId_DoesNotAccessRepository()
    {
        // Arrange
        var id = Guid.Empty;

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _riskFactorService.GetRiskFactorByIdAsync(id, CancellationToken.None)
        );
        
        Assert.Empty(_riskFactorRepository.ReceivedCalls());
    }

    [Fact]
    public async Task GetRiskFactorsAsync_ValidQuery_PreservesPagination()
    {
        // Arrange
        var riskFactor = RiskFactorConfigurationTestData.Create();

        var pageNumber = 2;
        var pageSize = 1;
        var totalCount = 3L;

        var query = new PageQuery(pageNumber, pageSize);
        var page = new PagedResult<RiskFactorConfiguration>([riskFactor], pageNumber, pageSize, totalCount);

        _riskFactorRepository.GetRiskFactorsAsync(query, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(page));

        // Act
        var result = await _riskFactorService.GetRiskFactorsAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(pageNumber, result.PageNumber);
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(riskFactor.Id, Assert.Single(result.Items).Id);

        await _pageValidator.Received(1).ValidateAsync(
            Arg.Is<IValidationContext>(context =>
                ReferenceEquals(context.InstanceToValidate, query) && context.ThrowOnFailures
            ),
            CancellationToken.None
        );
    }

    private static CreateRiskFactorCommand CreateCommand(
        RiskFactorLevel level = RiskTargetTestData.DefaultRiskFactorLevel,
        Guid? countryId = null,
        Guid? countyId = null,
        Guid? cityId = null,
        BuildingType buildingType = RiskTargetTestData.DefaultBuildingType,
        decimal adjustmentPercentage = RiskFactorConfigurationTestData.DefaultAdjustmentPercentage,
        bool isActive = RiskFactorConfigurationTestData.DefaultIsActive
    )
    {
        return new(
            Level: level,
            CountryId: level == RiskFactorLevel.Country ? countryId ?? RiskTargetTestData.DefaultCountryId : null,
            CountyId: level == RiskFactorLevel.County ? countyId ?? RiskTargetTestData.DefaultCountyId : null,
            CityId: level == RiskFactorLevel.City ? cityId ?? RiskTargetTestData.DefaultCityId : null,
            BuildingType: level == RiskFactorLevel.BuildingType ? buildingType : null,
            AdjustmentPercentage: adjustmentPercentage,
            IsActive: isActive
        );
    }

    private static UpdateRiskFactorCommand UpdateCommand(
        Guid riskFactorId,
        RiskFactorLevel level = RiskFactorLevel.City,
        Guid? countryId = null,
        Guid? countyId = null,
        Guid? cityId = null,
        BuildingType buildingType = BuildingType.Office,
        decimal adjustmentPercentage = -3m,
        bool isActive = false
    )
    {
        return new(
            RiskFactorId: riskFactorId,
            Level: level,
            CountryId: level == RiskFactorLevel.Country ? countryId ?? Guid.NewGuid() : null,
            CountyId: level == RiskFactorLevel.County ? countyId ?? Guid.NewGuid() : null,
            CityId: level == RiskFactorLevel.City ? cityId ?? Guid.NewGuid() : null,
            BuildingType: level == RiskFactorLevel.BuildingType ? buildingType : null,
            AdjustmentPercentage: adjustmentPercentage,
            IsActive: isActive
        );
    }
}
