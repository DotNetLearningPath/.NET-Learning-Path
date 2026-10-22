using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.RiskFactorConfig;
using InsuranceApp.Application.Exceptions;
using InsuranceApp.Application.Services;
using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.UnitTests.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace InsuranceApp.UnitTests.Application.Services;

public sealed class RiskFactorConfigServiceTests
{
    private readonly Mock<IRiskFactorConfigRepository> _repositoryMock;
    private readonly Mock<ILogger<RiskFactorConfigService>> _loggerMock;
    private readonly RiskFactorConfigService _service;

    public RiskFactorConfigServiceTests()
    {
        _repositoryMock = new Mock<IRiskFactorConfigRepository>();
        _loggerMock = new Mock<ILogger<RiskFactorConfigService>>();

        _service = new RiskFactorConfigService(_repositoryMock.Object, _loggerMock.Object);
    }

    #region Get All Tests

    [Fact]
    public async Task GetRiskFactorConfigsAsync_ReturnsMappedConfigurations()
    {
        // Arrange
        var config1 = TestData.CreateRiskFactorConfig1();
        var config2 = TestData.CreateRiskFactorConfig2();
        var configs = new List<RiskFactorConfig> { config1, config2 };

        _repositoryMock.Setup(x => x.GetRiskFactorConfigsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(configs);

        // Act
        var result = await _service.GetRiskFactorConfigsAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(configs.Count, result.Value.Count);
        Assert.Equal(config1.RiskFactorConfigId, result.Value[0].RiskFactorConfigId);
        Assert.Equal(config2.RiskFactorConfigId, result.Value[1].RiskFactorConfigId);
    }

    #endregion

    #region Get By Id Tests

    [Fact]
    public async Task GetRiskFactorConfigByIdAsync_ExistingConfig_ReturnsSuccess()
    {
        // Arrange
        var config = TestData.CreateRiskFactorConfig1();

        _repositoryMock.Setup(x => x.GetRiskFactorConfigByIdAsync(config.RiskFactorConfigId, It.IsAny<CancellationToken>())).ReturnsAsync(config);

        // Act
        var result = await _service.GetRiskFactorConfigByIdAsync(config.RiskFactorConfigId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(config.RiskFactorConfigId, result.Value.RiskFactorConfigId);
        Assert.Equal(config.Level, result.Value.Level);
        Assert.Equal(config.ReferenceId, result.Value.ReferenceId);
        Assert.Equal(config.AdjustmentPercentage, result.Value.AdjustmentPercentage);
        Assert.Equal(config.IsActive, result.Value.IsActive);
    }

    [Fact]
    public async Task GetRiskFactorConfigByIdAsync_EmptyId_ReturnsValidationError()
    {
        // Arrange
        var configId = Guid.Empty;

        // Act
        var result = await _service.GetRiskFactorConfigByIdAsync(configId, CancellationToken.None);

        // Assert
        AssertValidationError(result, RiskFactorConfigErrors.InvalidRiskFactorConfigId);

        _repositoryMock.Verify(x => x.GetRiskFactorConfigByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetRiskFactorConfigByIdAsync_NonExistingConfig_ReturnsNotFound()
    {
        // Arrange
        var configId = TestData.NonExistingId;

        _repositoryMock.Setup(x => x.GetRiskFactorConfigByIdAsync(configId, It.IsAny<CancellationToken>())).ReturnsAsync((RiskFactorConfig?)null);

        // Act
        var result = await _service.GetRiskFactorConfigByIdAsync(configId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(RiskFactorConfigErrors.NotFound(configId).Code, result.Error.Code);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task CreateRiskFactorConfigAsync_ValidConfig_ReturnsSuccess()
    {
        // Arrange
        var configDto = TestData.CreateRiskFactorConfigDto();

        SetupExistingReference(configDto.Level, configDto.ReferenceId);
        SetupNoDuplicateForCreate(configDto.Level, configDto.ReferenceId);

        // Act
        var result = await _service.CreateRiskFactorConfigAsync(configDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.NotEqual(Guid.Empty, result.Value.RiskFactorConfigId);
        Assert.Equal(configDto.Level, result.Value.Level);
        Assert.Equal(configDto.ReferenceId, result.Value.ReferenceId);
        Assert.Equal(configDto.AdjustmentPercentage, result.Value.AdjustmentPercentage);
        Assert.Equal(configDto.IsActive, result.Value.IsActive);

        _repositoryMock.Verify(x => x.AddRiskFactorConfigAsync(
            It.Is<RiskFactorConfig>(config =>
                config.Level == configDto.Level &&
                config.ReferenceId == configDto.ReferenceId &&
                config.AdjustmentPercentage == configDto.AdjustmentPercentage &&
                config.IsActive == configDto.IsActive),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateRiskFactorConfigAsync_NegativePercentage_ReturnsSuccess()
    {
        // Arrange
        var configDto = TestData.CreateRiskFactorConfigDto() with
        {
            AdjustmentPercentage = -TestData.CreateRiskFactorConfigDto().AdjustmentPercentage
        };

        SetupExistingReference(configDto.Level, configDto.ReferenceId);
        SetupNoDuplicateForCreate(configDto.Level, configDto.ReferenceId);

        // Act
        var result = await _service.CreateRiskFactorConfigAsync(configDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(configDto.AdjustmentPercentage, result.Value.AdjustmentPercentage);
    }

    [Fact]
    public async Task CreateRiskFactorConfigAsync_InvalidLevel_ReturnsValidationError()
    {
        // Arrange
        var configDto = TestData.CreateRiskFactorConfigDto() with { Level = (RiskFactorLevel)999 };

        // Act
        var result = await _service.CreateRiskFactorConfigAsync(configDto, CancellationToken.None);

        // Assert
        AssertValidationError(result, RiskFactorConfigErrors.InvalidLevel);
        VerifyNoCreateDatabaseValidation();
    }

    [Fact]
    public async Task CreateRiskFactorConfigAsync_EmptyReferenceId_ReturnsValidationError()
    {
        // Arrange
        var configDto = TestData.CreateRiskFactorConfigDto() with { ReferenceId = Guid.Empty };

        // Act
        var result = await _service.CreateRiskFactorConfigAsync(configDto, CancellationToken.None);

        // Assert
        AssertValidationError(result, RiskFactorConfigErrors.InvalidReferenceId);
        VerifyNoCreateDatabaseValidation();
    }

    [Fact]
    public async Task CreateRiskFactorConfigAsync_PercentageBelowMinimum_ReturnsValidationError()
    {
        // Arrange
        var configDto = TestData.CreateRiskFactorConfigDto() with { AdjustmentPercentage = RiskFactorConfigConstraints.MinAdjustmentPercentage - 0.01m };

        // Act
        var result = await _service.CreateRiskFactorConfigAsync(configDto, CancellationToken.None);

        // Assert
        AssertValidationError(result, RiskFactorConfigErrors.InvalidAdjustmentPercentage);
        VerifyNoCreateDatabaseValidation();
    }

    [Fact]
    public async Task CreateRiskFactorConfigAsync_PercentageAboveMaximum_ReturnsValidationError()
    {
        // Arrange
        var configDto = TestData.CreateRiskFactorConfigDto() with { AdjustmentPercentage = RiskFactorConfigConstraints.MaxAdjustmentPercentage + 0.01m };

        // Act
        var result = await _service.CreateRiskFactorConfigAsync(configDto, CancellationToken.None);

        // Assert
        AssertValidationError(result, RiskFactorConfigErrors.InvalidAdjustmentPercentage);
        VerifyNoCreateDatabaseValidation();
    }

    [Fact]
    public async Task CreateRiskFactorConfigAsync_PercentageWithTooManyDecimals_ReturnsValidationError()
    {
        // Arrange
        var configDto = TestData.CreateRiskFactorConfigDto() with { AdjustmentPercentage = TestData.InvalidRiskFactorPercentageScale };

        // Act
        var result = await _service.CreateRiskFactorConfigAsync(configDto, CancellationToken.None);

        // Assert
        AssertValidationError(result, RiskFactorConfigErrors.InvalidAdjustmentPercentageScale);
        VerifyNoCreateDatabaseValidation();
    }

    [Fact]
    public async Task CreateRiskFactorConfigAsync_NonExistingReference_ReturnsNotFound()
    {
        // Arrange
        var configDto = TestData.CreateRiskFactorConfigDto();

        _repositoryMock.Setup(x => x.ReferenceExistsAsync(configDto.Level, configDto.ReferenceId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.CreateRiskFactorConfigAsync(configDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(RiskFactorConfigErrors.ReferenceNotFound.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.ReferenceExistsAsync(configDto.Level, configDto.ReferenceId, It.IsAny<CancellationToken>()), Times.Once);
        _repositoryMock.Verify(x => x.RiskFactorConfigExistsAsync(It.IsAny<RiskFactorLevel>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.AddRiskFactorConfigAsync(It.IsAny<RiskFactorConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateRiskFactorConfigAsync_DuplicateConfig_ReturnsConflict()
    {
        // Arrange
        var configDto = TestData.CreateRiskFactorConfigDto();

        SetupExistingReference(configDto.Level, configDto.ReferenceId);

        _repositoryMock.Setup(x => x.RiskFactorConfigExistsAsync(configDto.Level, configDto.ReferenceId, null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _service.CreateRiskFactorConfigAsync(configDto, CancellationToken.None);

        // Assert
        AssertConflict(result);

        _repositoryMock.Verify(x => x.AddRiskFactorConfigAsync(It.IsAny<RiskFactorConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateRiskFactorConfigAsync_DuplicateOnInsert_ReturnsConflict()
    {
        // Arrange
        var configDto = TestData.CreateRiskFactorConfigDto();

        SetupExistingReference(configDto.Level, configDto.ReferenceId);
        SetupNoDuplicateForCreate(configDto.Level, configDto.ReferenceId);

        _repositoryMock.Setup(x => x.AddRiskFactorConfigAsync(It.IsAny<RiskFactorConfig>(), It.IsAny<CancellationToken>())).ThrowsAsync(new DuplicateEntityException(nameof(RiskFactorConfig)));

        // Act
        var result = await _service.CreateRiskFactorConfigAsync(configDto, CancellationToken.None);

        // Assert
        AssertConflict(result);

        _repositoryMock.Verify(x => x.AddRiskFactorConfigAsync(It.IsAny<RiskFactorConfig>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_ValidConfig_ReturnsSuccess()
    {
        // Arrange
        var config = TestData.CreateRiskFactorConfig1();
        var configDto = TestData.UpdateRiskFactorConfigDto() with
        {
            Level = RiskFactorLevel.City,
            ReferenceId = TestData.RiskFactorUpdateReferenceId,
            AdjustmentPercentage = -3.25m,
            IsActive = false
        };

        SetupExistingConfigForUpdate(config);
        SetupExistingReference(configDto.Level, configDto.ReferenceId);
        SetupNoDuplicateForUpdate(configDto.Level, configDto.ReferenceId, config.RiskFactorConfigId);

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(config.RiskFactorConfigId, configDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(configDto.Level, result.Value.Level);
        Assert.Equal(configDto.ReferenceId, result.Value.ReferenceId);
        Assert.Equal(configDto.AdjustmentPercentage, result.Value.AdjustmentPercentage);
        Assert.Equal(configDto.IsActive, result.Value.IsActive);
        Assert.NotNull(config.ModifiedAt);

        _repositoryMock.Verify(x => x.SaveRiskFactorConfigChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_EmptyId_ReturnsValidationError()
    {
        // Arrange
        var configId = Guid.Empty;
        var configDto = TestData.UpdateRiskFactorConfigDto();

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(configId, configDto, CancellationToken.None);

        // Assert
        AssertValidationError(result, RiskFactorConfigErrors.InvalidRiskFactorConfigId);

        _repositoryMock.Verify(x => x.GetRiskFactorConfigForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_NonExistingConfig_ReturnsNotFound()
    {
        // Arrange
        var configId = TestData.NonExistingId;
        var configDto = TestData.UpdateRiskFactorConfigDto();

        _repositoryMock.Setup(x => x.GetRiskFactorConfigForUpdateAsync(configId, It.IsAny<CancellationToken>())).ReturnsAsync((RiskFactorConfig?)null);

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(configId, configDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(RiskFactorConfigErrors.NotFound(configId).Code, result.Error.Code);

        _repositoryMock.Verify(x => x.ReferenceExistsAsync(It.IsAny<RiskFactorLevel>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.SaveRiskFactorConfigChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_NonExistingReference_ReturnsNotFound()
    {
        // Arrange
        var config = TestData.CreateRiskFactorConfig1();
        var configDto = TestData.UpdateRiskFactorConfigDto();

        SetupExistingConfigForUpdate(config);

        _repositoryMock.Setup(x => x.ReferenceExistsAsync(configDto.Level, configDto.ReferenceId, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(config.RiskFactorConfigId, configDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(RiskFactorConfigErrors.ReferenceNotFound.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.RiskFactorConfigExistsAsync(It.IsAny<RiskFactorLevel>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.SaveRiskFactorConfigChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_DuplicateConfig_ReturnsConflict()
    {
        // Arrange
        var config = TestData.CreateRiskFactorConfig1();
        var configDto = TestData.UpdateRiskFactorConfigDto();

        SetupExistingConfigForUpdate(config);
        SetupExistingReference(configDto.Level, configDto.ReferenceId);

        _repositoryMock.Setup(x => x.RiskFactorConfigExistsAsync(configDto.Level, configDto.ReferenceId, config.RiskFactorConfigId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(config.RiskFactorConfigId, configDto, CancellationToken.None);

        // Assert
        AssertConflict(result);

        _repositoryMock.Verify(x => x.SaveRiskFactorConfigChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_DuplicateCheck_ExcludesCurrentConfig()
    {
        // Arrange
        var config = TestData.CreateRiskFactorConfig1();

        var configDto = TestData.UpdateRiskFactorConfigDto() with
        {
            Level = config.Level,
            ReferenceId = config.ReferenceId
        };

        SetupExistingConfigForUpdate(config);
        SetupExistingReference(configDto.Level, configDto.ReferenceId);
        SetupNoDuplicateForUpdate(configDto.Level, configDto.ReferenceId, config.RiskFactorConfigId);

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(config.RiskFactorConfigId, configDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _repositoryMock.Verify(x => x.RiskFactorConfigExistsAsync(configDto.Level, configDto.ReferenceId, config.RiskFactorConfigId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_DuplicateOnSave_ReturnsConflict()
    {
        // Arrange
        var config = TestData.CreateRiskFactorConfig1();
        var configDto = TestData.UpdateRiskFactorConfigDto();

        SetupExistingConfigForUpdate(config);
        SetupExistingReference(configDto.Level, configDto.ReferenceId);
        SetupNoDuplicateForUpdate(configDto.Level, configDto.ReferenceId, config.RiskFactorConfigId);

        _repositoryMock.Setup(x => x.SaveRiskFactorConfigChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new DuplicateEntityException(nameof(RiskFactorConfig)));

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(config.RiskFactorConfigId, configDto, CancellationToken.None);

        // Assert
        AssertConflict(result);

        _repositoryMock.Verify(x => x.SaveRiskFactorConfigChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_InvalidLevel_ReturnsValidationError()
    {
        // Arrange
        var config = TestData.CreateRiskFactorConfig1();
        var configDto = TestData.UpdateRiskFactorConfigDto() with { Level = (RiskFactorLevel)999 };

        SetupExistingConfigForUpdate(config);

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(config.RiskFactorConfigId, configDto, CancellationToken.None);

        // Assert
        AssertValidationError(result, RiskFactorConfigErrors.InvalidLevel);

        _repositoryMock.Verify(x => x.ReferenceExistsAsync(It.IsAny<RiskFactorLevel>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.SaveRiskFactorConfigChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_EmptyReferenceId_ReturnsValidationError()
    {
        // Arrange
        var config = TestData.CreateRiskFactorConfig1();
        var configDto = TestData.UpdateRiskFactorConfigDto() with { ReferenceId = Guid.Empty };

        SetupExistingConfigForUpdate(config);

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(config.RiskFactorConfigId, configDto, CancellationToken.None);

        // Assert
        AssertValidationError(result, RiskFactorConfigErrors.InvalidReferenceId);

        _repositoryMock.Verify(x => x.ReferenceExistsAsync(It.IsAny<RiskFactorLevel>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.SaveRiskFactorConfigChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_PercentageBelowMinimum_ReturnsValidationError()
    {
        // Arrange
        var config = TestData.CreateRiskFactorConfig1();
        var configDto = TestData.UpdateRiskFactorConfigDto() with { AdjustmentPercentage = RiskFactorConfigConstraints.MinAdjustmentPercentage - 0.01m };

        SetupExistingConfigForUpdate(config);

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(config.RiskFactorConfigId, configDto, CancellationToken.None);

        // Assert
        AssertValidationError(result, RiskFactorConfigErrors.InvalidAdjustmentPercentage);

        _repositoryMock.Verify(x => x.ReferenceExistsAsync(It.IsAny<RiskFactorLevel>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.SaveRiskFactorConfigChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_PercentageAboveMaximum_ReturnsValidationError()
    {
        // Arrange
        var config = TestData.CreateRiskFactorConfig1();
        var configDto = TestData.UpdateRiskFactorConfigDto() with { AdjustmentPercentage = RiskFactorConfigConstraints.MaxAdjustmentPercentage + 0.01m };

        SetupExistingConfigForUpdate(config);

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(config.RiskFactorConfigId, configDto, CancellationToken.None);

        // Assert
        AssertValidationError(result, RiskFactorConfigErrors.InvalidAdjustmentPercentage);

        _repositoryMock.Verify(x => x.ReferenceExistsAsync(It.IsAny<RiskFactorLevel>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.SaveRiskFactorConfigChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_PercentageWithTooManyDecimals_ReturnsValidationError()
    {
        // Arrange
        var config = TestData.CreateRiskFactorConfig1();
        var configDto = TestData.UpdateRiskFactorConfigDto() with { AdjustmentPercentage = TestData.InvalidRiskFactorPercentageScale };

        SetupExistingConfigForUpdate(config);

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(config.RiskFactorConfigId, configDto, CancellationToken.None);

        // Assert
        AssertValidationError(result, RiskFactorConfigErrors.InvalidAdjustmentPercentageScale);

        _repositoryMock.Verify(x => x.ReferenceExistsAsync(It.IsAny<RiskFactorLevel>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.SaveRiskFactorConfigChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Helpers

    private void SetupExistingConfigForUpdate(RiskFactorConfig config)
    {
        _repositoryMock.Setup(x => x.GetRiskFactorConfigForUpdateAsync(config.RiskFactorConfigId, It.IsAny<CancellationToken>())).ReturnsAsync(config);
    }

    private void SetupExistingReference(RiskFactorLevel level, Guid referenceId)
    {
        _repositoryMock.Setup(x => x.ReferenceExistsAsync(level, referenceId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
    }

    private void SetupNoDuplicateForCreate(RiskFactorLevel level, Guid referenceId)
    {
        _repositoryMock.Setup(x => x.RiskFactorConfigExistsAsync(level, referenceId, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
    }

    private void SetupNoDuplicateForUpdate(RiskFactorLevel level, Guid referenceId, Guid configId)
    {
        _repositoryMock.Setup(x => x.RiskFactorConfigExistsAsync(level, referenceId, configId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
    }

    private void VerifyNoCreateDatabaseValidation()
    {
        _repositoryMock.Verify(x => x.ReferenceExistsAsync(It.IsAny<RiskFactorLevel>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.RiskFactorConfigExistsAsync(It.IsAny<RiskFactorLevel>(), It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.AddRiskFactorConfigAsync(It.IsAny<RiskFactorConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static void AssertValidationError(Result<RiskFactorConfigDto> result, Error expectedError)
    {
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(expectedError.Code, result.Error.Code);
    }

    private static void AssertConflict(Result<RiskFactorConfigDto> result)
    {
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(RiskFactorConfigErrors.AlreadyExists.Code, result.Error.Code);
    }

    #endregion
}
