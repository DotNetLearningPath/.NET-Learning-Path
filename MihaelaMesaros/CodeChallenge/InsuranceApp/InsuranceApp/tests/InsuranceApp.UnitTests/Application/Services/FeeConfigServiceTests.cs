using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.FeeConfig;
using InsuranceApp.Application.Services;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.UnitTests.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace InsuranceApp.UnitTests.Application.Services;

public sealed class FeeConfigServiceTests
{
    private readonly Mock<IFeeConfigRepository> _repositoryMock;
    private readonly Mock<ILogger<FeeConfigService>> _loggerMock;
    private readonly FeeConfigService _service;

    private readonly Guid _feeConfigId;
    private readonly CreateFeeConfigDto _validCreateDto;
    private readonly UpdateFeeConfigDto _validUpdateDto;
    private readonly FeeConfig _existingFeeConfig;

    public FeeConfigServiceTests()
    {
        _repositoryMock = new Mock<IFeeConfigRepository>();
        _loggerMock = new Mock<ILogger<FeeConfigService>>();

        _service = new FeeConfigService(
            _repositoryMock.Object,
            _loggerMock.Object);

        _feeConfigId = Guid.NewGuid();

        _validCreateDto = new CreateFeeConfigDto(
            "Standard broker fee",
            FeeType.BrokerCommission,
            2.50m,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            true);

        _validUpdateDto = new UpdateFeeConfigDto(
            "Standard broker fee",
            FeeType.BrokerCommission,
            2.50m,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            true);

        _existingFeeConfig = new FeeConfig
        {
            FeeConfigId = _feeConfigId,
            Name = "Standard broker fee",
            FeeType = FeeType.BrokerCommission,
            Percentage = 2.50m,
            EffectiveFrom = new DateTime(
                2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EffectiveTo = new DateTime(
                2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    #region Read Fee Config Tests

    [Fact]
    public async Task GetFeeConfigsAsync_ReturnsFeeConfigs()
    {
        // Arrange
        var adminFee = new FeeConfig
        {
            FeeConfigId = Guid.NewGuid(),
            Name = "Admin fee",
            FeeType = FeeType.AdminFee,
            Percentage = 1.50m,
            EffectiveFrom = new DateTime(
                2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EffectiveTo = new DateTime(
                2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var fees = new List<FeeConfig>
        {
            _existingFeeConfig,
            adminFee
        };

        _repositoryMock
            .Setup(x => x.GetFeeConfigsAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fees);

        // Act
        var result = await _service.GetFeeConfigsAsync(
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal("Standard broker fee", result.Value[0].Name);
        Assert.Equal("Admin fee", result.Value[1].Name);
    }

    [Fact]
    public async Task GetFeeConfigByIdAsync_ExistingFeeConfig_ReturnsSuccess()
    {
        // Arrange
        SetupExistingFeeConfigById();

        // Act
        var result = await _service.GetFeeConfigByIdAsync(
            _feeConfigId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(_feeConfigId, result.Value.FeeConfigId);
        Assert.Equal(_existingFeeConfig.Name, result.Value.Name);
        Assert.Equal(_existingFeeConfig.FeeType, result.Value.FeeType);
        Assert.Equal(_existingFeeConfig.Percentage, result.Value.Percentage);
    }

    [Fact]
    public async Task GetFeeConfigByIdAsync_NonExistingFeeConfig_ReturnsNotFound()
    {
        // Arrange
        var feeConfigId = TestConstants.NonExistingId;

        _repositoryMock
            .Setup(x => x.GetFeeConfigByIdAsync(
                feeConfigId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((FeeConfig?)null);

        // Act
        var result = await _service.GetFeeConfigByIdAsync(
            feeConfigId,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(
            FeeConfigErrors.NotFound(feeConfigId).Code,
            result.Error.Code);
    }

    [Fact]
    public async Task GetFeeConfigByIdAsync_EmptyId_ReturnsValidationError()
    {
        // Act
        var result = await _service.GetFeeConfigByIdAsync(
            Guid.Empty,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(
            FeeConfigErrors.InvalidFeeConfigId.Code,
            result.Error.Code);

        _repositoryMock.Verify(
            x => x.GetFeeConfigByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Create Fee Config Tests

    [Fact]
    public async Task CreateFeeConfigAsync_ValidFeeConfig_ReturnsSuccess()
    {
        // Act
        var result = await _service.CreateFeeConfigAsync(
            _validCreateDto,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(_validCreateDto.Name, result.Value.Name);
        Assert.Equal(_validCreateDto.FeeType, result.Value.FeeType);
        Assert.Equal(_validCreateDto.Percentage, result.Value.Percentage);
        Assert.Equal(_validCreateDto.EffectiveFrom, result.Value.EffectiveFrom);
        Assert.Equal(_validCreateDto.EffectiveTo, result.Value.EffectiveTo);
        Assert.Equal(_validCreateDto.IsActive, result.Value.IsActive);

        _repositoryMock.Verify(
            x => x.AddFeeConfigAsync(
                It.Is<FeeConfig>(fee =>
                    fee.Name == _validCreateDto.Name &&
                    fee.FeeType == _validCreateDto.FeeType &&
                    fee.Percentage == _validCreateDto.Percentage),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateFeeConfigAsync_ValidFeeConfig_TrimsName()
    {
        // Arrange
        var dto = _validCreateDto with
        {
            Name = "  Standard broker fee  "
        };

        // Act
        var result = await _service.CreateFeeConfigAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(
            "Standard broker fee",
            result.Value!.Name);

        _repositoryMock.Verify(
            x => x.AddFeeConfigAsync(
                It.Is<FeeConfig>(fee =>
                    fee.Name == "Standard broker fee"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateFeeConfigAsync_MissingName_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            Name = ""
        };

        await AssertInvalidCreateAsync(
            dto,
            FeeConfigErrors.NameRequired);
    }

    [Fact]
    public async Task CreateFeeConfigAsync_NameTooShort_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            Name = "AB"
        };

        await AssertInvalidCreateAsync(
            dto,
            FeeConfigErrors.InvalidNameLength);
    }

    [Fact]
    public async Task CreateFeeConfigAsync_NameTooLong_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            Name = new string('A', 201)
        };

        await AssertInvalidCreateAsync(
            dto,
            FeeConfigErrors.InvalidNameLength);
    }

    [Fact]
    public async Task CreateFeeConfigAsync_InvalidFeeType_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            FeeType = (FeeType)999
        };

        await AssertInvalidCreateAsync(
            dto,
            FeeConfigErrors.InvalidType);
    }

    [Fact]
    public async Task CreateFeeConfigAsync_InvalidPercentage_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            Percentage = 101m
        };

        await AssertInvalidCreateAsync(
            dto,
            FeeConfigErrors.InvalidPercentage);
    }

    [Fact]
    public async Task CreateFeeConfigAsync_PercentageWithTooManyDecimals_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            Percentage = 2.123m
        };

        await AssertInvalidCreateAsync(
            dto,
            FeeConfigErrors.InvalidPercentageScale);
    }

    [Fact]
    public async Task CreateFeeConfigAsync_InvalidEffectivePeriod_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            EffectiveFrom = new DateTime(
                2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),

            EffectiveTo = new DateTime(
                2026, 5, 31, 0, 0, 0, DateTimeKind.Utc)
        };

        await AssertInvalidCreateAsync(
            dto,
            FeeConfigErrors.InvalidEffectivePeriod);
    }

    #endregion

    #region Update Fee Config Tests

    [Fact]
    public async Task UpdateFeeConfigAsync_ValidFeeConfig_ReturnsUpdatedFeeConfig()
    {
        // Arrange
        var dto = _validUpdateDto with
        {
            Name = "Updated broker fee",
            Percentage = 5.50m,
            IsActive = false
        };

        SetupExistingFeeConfigForUpdate();

        // Act
        var result = await _service.UpdateFeeConfigAsync(
            _feeConfigId,
            dto,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Name, result.Value.Name);
        Assert.Equal(dto.FeeType, result.Value.FeeType);
        Assert.Equal(dto.Percentage, result.Value.Percentage);
        Assert.False(result.Value.IsActive);
        Assert.NotNull(_existingFeeConfig.ModifiedAt);

        _repositoryMock.Verify(
            x => x.SaveFeeConfigChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateFeeConfigAsync_EmptyId_ReturnsValidationError()
    {
        // Act
        var result = await _service.UpdateFeeConfigAsync(
            Guid.Empty,
            _validUpdateDto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(
            FeeConfigErrors.InvalidFeeConfigId.Code,
            result.Error.Code);

        _repositoryMock.Verify(
            x => x.GetFeeConfigForUpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateFeeConfigAsync_NonExistingFeeConfig_ReturnsNotFound()
    {
        // Arrange
        var feeConfigId = TestConstants.NonExistingId;

        _repositoryMock
            .Setup(x => x.GetFeeConfigForUpdateAsync(
                feeConfigId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((FeeConfig?)null);

        // Act
        var result = await _service.UpdateFeeConfigAsync(
            feeConfigId,
            _validUpdateDto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(
            FeeConfigErrors.NotFound(feeConfigId).Code,
            result.Error.Code);

        _repositoryMock.Verify(
            x => x.SaveFeeConfigChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateFeeConfigAsync_InvalidEffectivePeriod_ReturnsValidationError()
    {
        // Arrange
        var dto = _validUpdateDto with
        {
            EffectiveFrom = new DateTime(
                2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),

            EffectiveTo = new DateTime(
                2026, 5, 31, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var result = await _service.UpdateFeeConfigAsync(
            _feeConfigId,
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(
            FeeConfigErrors.InvalidEffectivePeriod.Code,
            result.Error.Code);

        _repositoryMock.Verify(
            x => x.GetFeeConfigForUpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Helpers

    private void SetupExistingFeeConfigById()
    {
        _repositoryMock
            .Setup(x => x.GetFeeConfigByIdAsync(
                _feeConfigId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingFeeConfig);
    }

    private void SetupExistingFeeConfigForUpdate()
    {
        _repositoryMock
            .Setup(x => x.GetFeeConfigForUpdateAsync(
                _feeConfigId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingFeeConfig);
    }

    private async Task AssertInvalidCreateAsync(
        CreateFeeConfigDto dto,
        Error expectedError)
    {
        var result = await _service.CreateFeeConfigAsync(
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(expectedError.Code, result.Error.Code);

        _repositoryMock.Verify(
            x => x.AddFeeConfigAsync(
                It.IsAny<FeeConfig>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion
}
