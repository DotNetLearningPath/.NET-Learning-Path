using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.RiskFactorConfig;
using InsuranceApp.Application.Exceptions;
using InsuranceApp.Application.Services;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.UnitTests.Common;
using Microsoft.Extensions.Logging;
using Moq;
using System.Globalization;

namespace InsuranceApp.UnitTests.Application.Services;

public sealed class RiskFactorConfigServiceTests
{
    private readonly Mock<IRiskFactorConfigRepository> _repositoryMock;
    private readonly Mock<ILogger<RiskFactorConfigService>> _loggerMock;
    private readonly RiskFactorConfigService _service;

    private readonly Guid _riskFactorConfigId;
    private readonly Guid _referenceId;

    private readonly CreateRiskFactorConfigDto _validCreateDto;
    private readonly UpdateRiskFactorConfigDto _validUpdateDto;
    private readonly RiskFactorConfig _existingConfig;

    public RiskFactorConfigServiceTests()
    {
        _repositoryMock = new Mock<IRiskFactorConfigRepository>();
        _loggerMock = new Mock<ILogger<RiskFactorConfigService>>();

        _service = new RiskFactorConfigService(
            _repositoryMock.Object,
            _loggerMock.Object);


        _riskFactorConfigId = Guid.NewGuid();
        _referenceId = Guid.NewGuid();

        _validCreateDto = new CreateRiskFactorConfigDto(
            RiskFactorLevel.Country,
            _referenceId,
            5.25m,
            true);

        _validUpdateDto = new UpdateRiskFactorConfigDto(
            RiskFactorLevel.County,
            _referenceId,
            -2.50m,
            true);

        _existingConfig = new RiskFactorConfig
        {
            RiskFactorConfigId = _riskFactorConfigId,
            Level = RiskFactorLevel.Country,
            ReferenceId = _referenceId,
            AdjustmentPercentage = 5.25m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    #region Get All Tests

    [Fact]
    public async Task GetRiskFactorConfigsAsync_ReturnsMappedConfigurations()
    {
        // Arrange
        var secondConfig = new RiskFactorConfig
        {
            RiskFactorConfigId = Guid.NewGuid(),
            Level = RiskFactorLevel.City,
            ReferenceId = Guid.NewGuid(),
            AdjustmentPercentage = -2.50m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var configs = new List<RiskFactorConfig>
        {
            _existingConfig,
            secondConfig
        };

        _repositoryMock
            .Setup(x => x.GetRiskFactorConfigsAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(configs);

        // Act
        var result = await _service.GetRiskFactorConfigsAsync(
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);

        Assert.Equal(
            _existingConfig.RiskFactorConfigId,
            result.Value[0].RiskFactorConfigId);

        Assert.Equal(
            secondConfig.RiskFactorConfigId,
            result.Value[1].RiskFactorConfigId);
    }

    #endregion

    #region Get By Id Tests

    [Fact]
    public async Task GetRiskFactorConfigByIdAsync_ExistingConfig_ReturnsSuccess()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetRiskFactorConfigByIdAsync(
                _riskFactorConfigId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingConfig);

        // Act
        var result = await _service.GetRiskFactorConfigByIdAsync(
            _riskFactorConfigId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(
            _existingConfig.RiskFactorConfigId,
            result.Value.RiskFactorConfigId);

        Assert.Equal(
            _existingConfig.Level,
            result.Value.Level);

        Assert.Equal(
            _existingConfig.ReferenceId,
            result.Value.ReferenceId);

        Assert.Equal(
            _existingConfig.AdjustmentPercentage,
            result.Value.AdjustmentPercentage);

        Assert.Equal(
            _existingConfig.IsActive,
            result.Value.IsActive);
    }

    [Fact]
    public async Task GetRiskFactorConfigByIdAsync_EmptyId_ReturnsValidationError()
    {
        // Act
        var result = await _service.GetRiskFactorConfigByIdAsync(
            Guid.Empty,
            CancellationToken.None);

        // Assert
        AssertValidationError(
            result,
            RiskFactorConfigErrors.InvalidRiskFactorConfigId);

        _repositoryMock.Verify(
            x => x.GetRiskFactorConfigByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetRiskFactorConfigByIdAsync_NonExistingConfig_ReturnsNotFound()
    {
        // Arrange
        var configId = TestConstants.NonExistingId;

        _repositoryMock
            .Setup(x => x.GetRiskFactorConfigByIdAsync(
                configId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RiskFactorConfig?)null);

        // Act
        var result = await _service.GetRiskFactorConfigByIdAsync(
            configId,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);

        Assert.Equal(
            RiskFactorConfigErrors.NotFound(configId).Code,
            result.Error.Code);
    }

    #endregion

    #region Create Tests

    [Fact]
    public async Task CreateRiskFactorConfigAsync_ValidConfig_ReturnsSuccess()
    {
        // Arrange
        SetupExistingReference(
            _validCreateDto.Level,
            _validCreateDto.ReferenceId);

        SetupNoDuplicate(
            _validCreateDto.Level,
            _validCreateDto.ReferenceId);

        // Act
        var result = await _service.CreateRiskFactorConfigAsync(
            _validCreateDto,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.NotEqual(
            Guid.Empty,
            result.Value.RiskFactorConfigId);

        Assert.Equal(
            _validCreateDto.Level,
            result.Value.Level);

        Assert.Equal(
            _validCreateDto.ReferenceId,
            result.Value.ReferenceId);

        Assert.Equal(
            _validCreateDto.AdjustmentPercentage,
            result.Value.AdjustmentPercentage);

        Assert.Equal(
            _validCreateDto.IsActive,
            result.Value.IsActive);

        _repositoryMock.Verify(
            x => x.AddRiskFactorConfigAsync(
                It.Is<RiskFactorConfig>(config =>
                    config.Level == _validCreateDto.Level &&
                    config.ReferenceId == _validCreateDto.ReferenceId &&
                    config.AdjustmentPercentage ==
                        _validCreateDto.AdjustmentPercentage &&
                    config.IsActive == _validCreateDto.IsActive),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateRiskFactorConfigAsync_NegativePercentage_ReturnsSuccess()
    {
        // Arrange
        var dto = _validCreateDto with
        {
            AdjustmentPercentage = -5.25m
        };

        SetupExistingReference(
            dto.Level,
            dto.ReferenceId);

        SetupNoDuplicate(
            dto.Level,
            dto.ReferenceId);

        // Act
        var result = await _service.CreateRiskFactorConfigAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(
            -5.25m,
            result.Value!.AdjustmentPercentage);
    }

    [Fact]
    public async Task CreateRiskFactorConfigAsync_InvalidLevel_ReturnsValidationError()
    {
        // Arrange
        var dto = _validCreateDto with
        {
            Level = (RiskFactorLevel)999
        };

        // Act
        var result = await _service.CreateRiskFactorConfigAsync(
            dto,
            CancellationToken.None);

        // Assert
        AssertValidationError(
            result,
            RiskFactorConfigErrors.InvalidLevel);

        VerifyNoDatabaseValidation();
    }

    [Fact]
    public async Task CreateRiskFactorConfigAsync_EmptyReferenceId_ReturnsValidationError()
    {
        // Arrange
        var dto = _validCreateDto with
        {
            ReferenceId = Guid.Empty
        };

        // Act
        var result = await _service.CreateRiskFactorConfigAsync(
            dto,
            CancellationToken.None);

        // Assert
        AssertValidationError(
            result,
            RiskFactorConfigErrors.InvalidReferenceId);

        VerifyNoDatabaseValidation();
    }

    [Theory]
    [InlineData("-100.01")]
    [InlineData("100.01")]
    public async Task CreateRiskFactorConfigAsync_PercentageOutsideRange_ReturnsValidationError(
        string percentage)
    {
        // Arrange
        var dto = _validCreateDto with
        {
            AdjustmentPercentage = decimal.Parse(
                percentage,
                CultureInfo.InvariantCulture)
        };

        // Act
        var result = await _service.CreateRiskFactorConfigAsync(
            dto,
            CancellationToken.None);

        // Assert
        AssertValidationError(
            result,
            RiskFactorConfigErrors.InvalidAdjustmentPercentage);

        VerifyNoDatabaseValidation();
    }

    [Fact]
    public async Task CreateRiskFactorConfigAsync_PercentageWithTooManyDecimals_ReturnsValidationError()
    {
        // Arrange
        var dto = _validCreateDto with
        {
            AdjustmentPercentage = 5.123m
        };

        // Act
        var result = await _service.CreateRiskFactorConfigAsync(
            dto,
            CancellationToken.None);

        // Assert
        AssertValidationError(
            result,
            RiskFactorConfigErrors.InvalidAdjustmentPercentageScale);

        VerifyNoDatabaseValidation();
    }

    [Fact]
    public async Task CreateRiskFactorConfigAsync_NonExistingReference_ReturnsNotFound()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.ReferenceExistsAsync(
                _validCreateDto.Level,
                _validCreateDto.ReferenceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.CreateRiskFactorConfigAsync(
            _validCreateDto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);

        Assert.Equal(
            RiskFactorConfigErrors.ReferenceNotFound.Code,
            result.Error.Code);

        _repositoryMock.Verify(
            x => x.RiskFactorConfigExistsAsync(
                It.IsAny<RiskFactorLevel>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            x => x.AddRiskFactorConfigAsync(
                It.IsAny<RiskFactorConfig>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateRiskFactorConfigAsync_DuplicateConfig_ReturnsConflict()
    {
        // Arrange
        SetupExistingReference(
            _validCreateDto.Level,
            _validCreateDto.ReferenceId);

        _repositoryMock
            .Setup(x => x.RiskFactorConfigExistsAsync(
                _validCreateDto.Level,
                _validCreateDto.ReferenceId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateRiskFactorConfigAsync(
            _validCreateDto,
            CancellationToken.None);

        // Assert
        AssertConflict(result);

        _repositoryMock.Verify(
            x => x.AddRiskFactorConfigAsync(
                It.IsAny<RiskFactorConfig>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateRiskFactorConfigAsync_DuplicateOnInsert_ReturnsConflict()
    {
        // Arrange
        SetupExistingReference(
            _validCreateDto.Level,
            _validCreateDto.ReferenceId);

        SetupNoDuplicate(
            _validCreateDto.Level,
            _validCreateDto.ReferenceId);

        _repositoryMock
            .Setup(x => x.AddRiskFactorConfigAsync(
                It.IsAny<RiskFactorConfig>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new DuplicateEntityException(
                    nameof(RiskFactorConfig)));

        // Act
        var result = await _service.CreateRiskFactorConfigAsync(
            _validCreateDto,
            CancellationToken.None);

        // Assert
        AssertConflict(result);

        _repositoryMock.Verify(
            x => x.AddRiskFactorConfigAsync(
                It.IsAny<RiskFactorConfig>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_ValidConfig_ReturnsSuccess()
    {
        // Arrange
        var referenceId = Guid.NewGuid();

        var dto = _validUpdateDto with
        {
            Level = RiskFactorLevel.City,
            ReferenceId = referenceId,
            AdjustmentPercentage = -3.25m,
            IsActive = false
        };

        SetupExistingConfigForUpdate();

        SetupExistingReference(
            dto.Level,
            dto.ReferenceId);

        SetupNoDuplicateForUpdate(
            dto.Level,
            dto.ReferenceId);

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(
            _riskFactorConfigId,
            dto,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(dto.Level, result.Value.Level);
        Assert.Equal(dto.ReferenceId, result.Value.ReferenceId);

        Assert.Equal(
            dto.AdjustmentPercentage,
            result.Value.AdjustmentPercentage);

        Assert.False(result.Value.IsActive);
        Assert.NotNull(_existingConfig.ModifiedAt);

        _repositoryMock.Verify(
            x => x.SaveRiskFactorConfigChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_EmptyId_ReturnsValidationError()
    {
        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(
            Guid.Empty,
            _validUpdateDto,
            CancellationToken.None);

        // Assert
        AssertValidationError(
            result,
            RiskFactorConfigErrors.InvalidRiskFactorConfigId);

        _repositoryMock.Verify(
            x => x.GetRiskFactorConfigForUpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_NonExistingConfig_ReturnsNotFound()
    {
        // Arrange
        var configId = TestConstants.NonExistingId;

        _repositoryMock
            .Setup(x => x.GetRiskFactorConfigForUpdateAsync(
                configId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RiskFactorConfig?)null);

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(
            configId,
            _validUpdateDto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);

        Assert.Equal(
            RiskFactorConfigErrors.NotFound(configId).Code,
            result.Error.Code);

        _repositoryMock.Verify(
            x => x.ReferenceExistsAsync(
                It.IsAny<RiskFactorLevel>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_NonExistingReference_ReturnsNotFound()
    {
        // Arrange
        SetupExistingConfigForUpdate();

        _repositoryMock
            .Setup(x => x.ReferenceExistsAsync(
                _validUpdateDto.Level,
                _validUpdateDto.ReferenceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(
            _riskFactorConfigId,
            _validUpdateDto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);

        Assert.Equal(
            RiskFactorConfigErrors.ReferenceNotFound.Code,
            result.Error.Code);

        _repositoryMock.Verify(
            x => x.SaveRiskFactorConfigChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_DuplicateConfig_ReturnsConflict()
    {
        // Arrange
        SetupExistingConfigForUpdate();

        SetupExistingReference(
            _validUpdateDto.Level,
            _validUpdateDto.ReferenceId);

        _repositoryMock
            .Setup(x => x.RiskFactorConfigExistsAsync(
                _validUpdateDto.Level,
                _validUpdateDto.ReferenceId,
                _riskFactorConfigId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(
            _riskFactorConfigId,
            _validUpdateDto,
            CancellationToken.None);

        // Assert
        AssertConflict(result);

        _repositoryMock.Verify(
            x => x.SaveRiskFactorConfigChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_DuplicateCheck_ExcludesCurrentConfig()
    {
        // Arrange
        var dto = new UpdateRiskFactorConfigDto(
            _existingConfig.Level,
            _existingConfig.ReferenceId,
            7.50m,
            true);

        SetupExistingConfigForUpdate();

        SetupExistingReference(
            dto.Level,
            dto.ReferenceId);

        SetupNoDuplicateForUpdate(
            dto.Level,
            dto.ReferenceId);

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(
            _riskFactorConfigId,
            dto,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _repositoryMock.Verify(
            x => x.RiskFactorConfigExistsAsync(
                dto.Level,
                dto.ReferenceId,
                _riskFactorConfigId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_DuplicateOnSave_ReturnsConflict()
    {
        // Arrange
        SetupExistingConfigForUpdate();

        SetupExistingReference(
            _validUpdateDto.Level,
            _validUpdateDto.ReferenceId);

        SetupNoDuplicateForUpdate(
            _validUpdateDto.Level,
            _validUpdateDto.ReferenceId);

        _repositoryMock
            .Setup(x => x.SaveRiskFactorConfigChangesAsync(
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new DuplicateEntityException(
                    nameof(RiskFactorConfig)));

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(
            _riskFactorConfigId,
            _validUpdateDto,
            CancellationToken.None);

        // Assert
        AssertConflict(result);

        _repositoryMock.Verify(
            x => x.SaveRiskFactorConfigChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_InvalidLevel_ReturnsValidationError()
    {
        // Arrange
        var dto = _validUpdateDto with
        {
            Level = (RiskFactorLevel)999
        };

        SetupExistingConfigForUpdate();

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(
            _riskFactorConfigId,
            dto,
            CancellationToken.None);

        // Assert
        AssertValidationError(
            result,
            RiskFactorConfigErrors.InvalidLevel);

        _repositoryMock.Verify(
            x => x.ReferenceExistsAsync(
                It.IsAny<RiskFactorLevel>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_EmptyReferenceId_ReturnsValidationError()
    {
        // Arrange
        var dto = _validUpdateDto with
        {
            ReferenceId = Guid.Empty
        };

        SetupExistingConfigForUpdate();

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(
            _riskFactorConfigId,
            dto,
            CancellationToken.None);

        // Assert
        AssertValidationError(
            result,
            RiskFactorConfigErrors.InvalidReferenceId);

        _repositoryMock.Verify(
            x => x.ReferenceExistsAsync(
                It.IsAny<RiskFactorLevel>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("-100.01")]
    [InlineData("100.01")]
    public async Task UpdateRiskFactorConfigAsync_PercentageOutsideRange_ReturnsValidationError(
        string percentage)
    {
        // Arrange
        var dto = _validUpdateDto with
        {
            AdjustmentPercentage = decimal.Parse(
                percentage,
                CultureInfo.InvariantCulture)
        };

        SetupExistingConfigForUpdate();

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(
            _riskFactorConfigId,
            dto,
            CancellationToken.None);

        // Assert
        AssertValidationError(
            result,
            RiskFactorConfigErrors.InvalidAdjustmentPercentage);
    }

    [Fact]
    public async Task UpdateRiskFactorConfigAsync_PercentageWithTooManyDecimals_ReturnsValidationError()
    {
        // Arrange
        var dto = _validUpdateDto with
        {
            AdjustmentPercentage = 5.123m
        };

        SetupExistingConfigForUpdate();

        // Act
        var result = await _service.UpdateRiskFactorConfigAsync(
            _riskFactorConfigId,
            dto,
            CancellationToken.None);

        // Assert
        AssertValidationError(
            result,
            RiskFactorConfigErrors.InvalidAdjustmentPercentageScale);
    }

    #endregion

    #region Helpers

    private void SetupExistingConfigForUpdate()
    {
        _repositoryMock
            .Setup(x => x.GetRiskFactorConfigForUpdateAsync(
                _riskFactorConfigId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingConfig);
    }

    private void SetupExistingReference(
        RiskFactorLevel level,
        Guid referenceId)
    {
        _repositoryMock
            .Setup(x => x.ReferenceExistsAsync(
                level,
                referenceId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private void SetupNoDuplicate(
        RiskFactorLevel level,
        Guid referenceId)
    {
        _repositoryMock
            .Setup(x => x.RiskFactorConfigExistsAsync(
                level,
                referenceId,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private void SetupNoDuplicateForUpdate(
        RiskFactorLevel level,
        Guid referenceId)
    {
        _repositoryMock
            .Setup(x => x.RiskFactorConfigExistsAsync(
                level,
                referenceId,
                _riskFactorConfigId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private void VerifyNoDatabaseValidation()
    {
        _repositoryMock.Verify(
            x => x.ReferenceExistsAsync(
                It.IsAny<RiskFactorLevel>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            x => x.RiskFactorConfigExistsAsync(
                It.IsAny<RiskFactorLevel>(),
                It.IsAny<Guid>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static void AssertValidationError(
        Result<RiskFactorConfigDto> result,
        Error expectedError)
    {
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(expectedError.Code, result.Error.Code);
    }

    private static void AssertConflict(
        Result<RiskFactorConfigDto> result)
    {
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);

        Assert.Equal(
            RiskFactorConfigErrors.AlreadyExists.Code,
            result.Error.Code);
    }

    #endregion
}