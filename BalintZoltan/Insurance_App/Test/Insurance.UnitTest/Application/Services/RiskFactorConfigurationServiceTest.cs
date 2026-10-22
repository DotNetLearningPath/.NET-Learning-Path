using Insurance.Application.DTO.Common;
using Insurance.Application.DTO.RiskFactors;
using Insurance.Application.Exceptions;
using Insurance.Application.Services;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Insurance.UnitTest.Application.Fakes;

namespace Insurance.UnitTest.Application.Services;

public sealed class RiskFactorConfigurationServiceTest
{
    private readonly FakeRiskFactorRepository _repository = new();
    private readonly RiskFactorConfigurationService _service;

    public RiskFactorConfigurationServiceTest()
    {
        _service = new RiskFactorConfigurationService(_repository);
    }

    [Theory]
    [InlineData(RiskFactorLevel.Country)]
    [InlineData(RiskFactorLevel.County)]
    [InlineData(RiskFactorLevel.City)]
    public async Task CreateAsync_CreatesGeographyRiskFactor(RiskFactorLevel level)
    {
        // Arrange
        var referenceId = Guid.NewGuid();
        var request = CreateRequest(level, referenceId.ToString());

        // Act
        var dto = await _service.CreateRiskFactorAsync(request, CancellationToken.None);

        // Assert
        var saved = Assert.Single(_repository.Storage);
        Assert.Equal(saved.Id, dto.Id);
        Assert.Equal(level, saved.Level);
        Assert.Equal(referenceId.ToString(), saved.Reference);
        Assert.Equal(request.AdjustmentPercentage, dto.AdjustmentPercentage);
        Assert.Equal(request.IsActive, dto.IsActive);
        Assert.Equal(1, _repository.AddCallCount);
    }

    [Fact]
    public async Task CreateAsync_NormalizesBuildingTypeReference()
    {
        // Arrange
        var request = CreateRequest(RiskFactorLevel.BuildingType, " historic ");

        // Act
        var dto = await _service.CreateRiskFactorAsync(
            request,
            CancellationToken.None);

        // Assert
        Assert.Equal("Historic", dto.Reference);
        Assert.Equal(RiskFactorLevel.BuildingType, dto.Level);
    }

    [Fact]
    public async Task ListAsync_ReturnsPagedMappedConfigurations()
    {
        // Arrange
        var first = CreateConfiguration(RiskFactorLevel.City, Guid.NewGuid().ToString(), 5m);
        var second = CreateConfiguration(RiskFactorLevel.BuildingType, "Residential", -2m);
        _repository.Storage.AddRange(first, second);

        // Act
        var result = await _service.ListRiskFactorAsync(new PaginationRequest
        {
            PageNumber = 2,
            PageSize = 1
        }, CancellationToken.None);

        // Assert
        var dto = Assert.Single(result.Items);
        Assert.Equal(second.Id, dto.Id);
        Assert.Equal(second.Level, dto.Level);
        Assert.Equal(second.Reference, dto.Reference);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(1, result.PageSize);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsMappedConfiguration()
    {
        // Arrange
        var configuration = CreateConfiguration(RiskFactorLevel.BuildingType, "Historic", 12.5m);
        _repository.Storage.Add(configuration);

        // Act
        var dto = await _service.GetRiskFactorByIdAsync(configuration.Id, CancellationToken.None);

        // Assert
        Assert.Equal(configuration.Id, dto.Id);
        Assert.Equal(configuration.Level, dto.Level);
        Assert.Equal(configuration.Reference, dto.Reference);
        Assert.Equal(configuration.AdjustmentPercentage, dto.AdjustmentPercentage);
        Assert.Equal(configuration.IsActive, dto.IsActive);
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsNotFoundWhenConfigurationDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetRiskFactorByIdAsync(id, CancellationToken.None));

        // Assert
        Assert.Equal("Risk factor configuration was not found.", exception.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task UpdateAsync_UpdatesFieldsAndActiveState(bool isActive)
    {
        // Arrange
        var configuration = CreateConfiguration(RiskFactorLevel.City, Guid.NewGuid().ToString(), 2m);
        _repository.Storage.Add(configuration);
        var newReference = Guid.NewGuid().ToString();

        // Act
        var dto = await _service.UpdateRiskFactorAsync(configuration.Id,
            CreateRequest(RiskFactorLevel.County, newReference, -3.5m, isActive),
            CancellationToken.None);

        // Assert
        Assert.Equal(RiskFactorLevel.County, configuration.Level);
        Assert.Equal(newReference, configuration.Reference);
        Assert.Equal(-3.5m, configuration.AdjustmentPercentage);
        Assert.Equal(isActive, configuration.IsActive);
        Assert.Equal(configuration.Id, dto.Id);
        Assert.Equal(isActive, dto.IsActive);
        Assert.Equal(1, _repository.UpdateCallCount);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsNotFoundWhenConfigurationDoesNotExist()
    {
        // Arrange
        var id = Guid.NewGuid();
        var request = CreateRequest(RiskFactorLevel.City, Guid.NewGuid().ToString());

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateRiskFactorAsync(id, request, CancellationToken.None));

        // Assert
        Assert.Equal("Risk factor configuration was not found.", exception.Message);
        Assert.Equal(0, _repository.UpdateCallCount);
    }

    [Theory]
    [InlineData(RiskFactorLevel.City, "not-a-guid")]
    [InlineData(RiskFactorLevel.BuildingType, "UnknownType")]
    public async Task CreateAsync_RejectsInvalidReference(RiskFactorLevel level, string reference)
    {
        // Arrange
        var request = CreateRequest(level, reference);

        // Act
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateRiskFactorAsync(request, CancellationToken.None));

        // Assert
        Assert.Empty(_repository.Storage);
        Assert.Equal(0, _repository.AddCallCount);
    }

    [Fact]
    public async Task CreateAsync_RejectsUndefinedRiskFactorLevel()
    {
        // Arrange
        var request = CreateRequest((RiskFactorLevel)99, Guid.NewGuid().ToString());

        // Act
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateRiskFactorAsync(request, CancellationToken.None));

        // Assert
        Assert.Empty(_repository.Storage);
    }

    [Fact]
    public async Task CreateAsync_RejectsNullRequest()
    {
        // Arrange
        SaveRiskFactorConfigurationRequest? request = null;

        // Act
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.CreateRiskFactorAsync(request!, CancellationToken.None));

        // Assert
        Assert.Empty(_repository.Storage);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotChangeConfigurationWhenRequestIsInvalid()
    {
        // Arrange
        var configuration = CreateConfiguration(RiskFactorLevel.City, Guid.NewGuid().ToString(), 2m);
        _repository.Storage.Add(configuration);
        var originalReference = configuration.Reference;

        // Act
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UpdateRiskFactorAsync(configuration.Id,
                CreateRequest(RiskFactorLevel.County, "invalid", 9m, false),
                CancellationToken.None));

        // Assert
        Assert.Equal(RiskFactorLevel.City, configuration.Level);
        Assert.Equal(originalReference, configuration.Reference);
        Assert.Equal(2m, configuration.AdjustmentPercentage);
        Assert.True(configuration.IsActive);
        Assert.Equal(0, _repository.UpdateCallCount);
    }

    [Fact]
    public async Task ActivateAsync_ActivatesExistingConfiguration()
    {
        // Arrange
        var configuration = CreateConfiguration(RiskFactorLevel.City, Guid.NewGuid().ToString(), 1m, false);
        _repository.Storage.Add(configuration);

        // Act
        var dto = await _service.ActivateRiskAsync(configuration.Id, CancellationToken.None);

        // Assert
        Assert.True(configuration.IsActive);
        Assert.True(dto.IsActive);
        Assert.Equal(configuration.Id, dto.Id);
        Assert.Equal(1, _repository.UpdateCallCount);
    }

    [Fact]
    public async Task DeactivateAsync_DeactivatesExistingConfiguration()
    {
        // Arrange
        var configuration = CreateConfiguration(RiskFactorLevel.City, Guid.NewGuid().ToString(), 1m);
        _repository.Storage.Add(configuration);

        // Act
        var dto = await _service.DeactivateRiskFactorAsync(configuration.Id, CancellationToken.None);

        // Assert
        Assert.False(configuration.IsActive);
        Assert.False(dto.IsActive);
        Assert.Equal(configuration.Id, dto.Id);
        Assert.Equal(1, _repository.UpdateCallCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task StateChanges_ThrowNotFoundForMissingConfiguration(bool activate)
    {
        // Arrange
        var action = activate
            ? _service.ActivateRiskAsync(Guid.NewGuid(), CancellationToken.None)
            : _service.DeactivateRiskFactorAsync(Guid.NewGuid(), CancellationToken.None);

        // Act
        await Assert.ThrowsAsync<NotFoundException>(() => action);

        // Assert
        Assert.Equal(0, _repository.UpdateCallCount);
    }

    private static SaveRiskFactorConfigurationRequest CreateRequest(
        RiskFactorLevel level,
        string reference,
        decimal adjustment = 5m,
        bool isActive = true) => new()
    {
        Level = level,
        Reference = reference,
        AdjustmentPercentage = adjustment,
        IsActive = isActive
    };

    private static RiskFactorConfiguration CreateConfiguration(
        RiskFactorLevel level,
        string reference,
        decimal adjustment,
        bool isActive = true) => new(level, reference, adjustment, isActive);
}
