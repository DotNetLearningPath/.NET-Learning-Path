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
    private readonly FeeConfigService _service;

    public FeeConfigServiceTests()
    {
        _repositoryMock = new Mock<IFeeConfigRepository>();
        var loggerMock = new Mock<ILogger<FeeConfigService>>();

        _service = new FeeConfigService(
            _repositoryMock.Object,
            loggerMock.Object);
    }

    #region Read Fee Config Tests

    [Fact]
    public async Task GetFeeConfigsAsync_ReturnsFeeConfigs()
    {
        // Arrange
        var fees = new List<FeeConfig>
        {
            CreateFeeConfigEntity(
                name: "Broker fee",
                feeType: FeeType.BrokerCommission),

            CreateFeeConfigEntity(
                name: "Admin fee",
                feeType: FeeType.AdminFee)
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
        Assert.Equal("Broker fee", result.Value[0].Name);
        Assert.Equal("Admin fee", result.Value[1].Name);
    }

    [Fact]
    public async Task GetFeeConfigByIdAsync_ExistingFeeConfig_ReturnsSuccess()
    {
        // Arrange
        var fee = CreateFeeConfigEntity();

        _repositoryMock
            .Setup(x => x.GetFeeConfigByIdAsync(
                fee.FeeConfigId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fee);

        // Act
        var result = await _service.GetFeeConfigByIdAsync(
            fee.FeeConfigId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(fee.FeeConfigId, result.Value.FeeConfigId);
        Assert.Equal(fee.Name, result.Value.Name);
        Assert.Equal(fee.FeeType, result.Value.FeeType);
        Assert.Equal(fee.Percentage, result.Value.Percentage);
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
        // Arrange
        var dto = CreateValidFeeConfigDto();

        // Act
        var result = await _service.CreateFeeConfigAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Name, result.Value.Name);
        Assert.Equal(dto.FeeType, result.Value.FeeType);
        Assert.Equal(dto.Percentage, result.Value.Percentage);
        Assert.Equal(dto.EffectiveFrom, result.Value.EffectiveFrom);
        Assert.Equal(dto.EffectiveTo, result.Value.EffectiveTo);
        Assert.Equal(dto.IsActive, result.Value.IsActive);

        _repositoryMock.Verify(
            x => x.AddFeeConfigAsync(
                It.Is<FeeConfig>(fee =>
                    fee.Name == dto.Name &&
                    fee.FeeType == dto.FeeType &&
                    fee.Percentage == dto.Percentage),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateFeeConfigAsync_ValidFeeConfig_TrimsName()
    {
        // Arrange
        var dto = CreateValidFeeConfigDto() with
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
        var dto = CreateValidFeeConfigDto() with
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
        var dto = CreateValidFeeConfigDto() with
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
        var dto = CreateValidFeeConfigDto() with
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
        // FeeType remains an enum, so an invalid int value is used.
        var dto = CreateValidFeeConfigDto() with
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
        var dto = CreateValidFeeConfigDto() with
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
        var dto = CreateValidFeeConfigDto() with
        {
            Percentage = 2.12345m
        };

        await AssertInvalidCreateAsync(
            dto,
            FeeConfigErrors.InvalidPercentageScale);
    }

    [Fact]
    public async Task CreateFeeConfigAsync_InvalidEffectivePeriod_ReturnsValidationError()
    {
        var dto = CreateValidFeeConfigDto() with
        {
            EffectiveFrom = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            EffectiveTo = new DateTime(2026, 5, 31, 0, 0, 0, DateTimeKind.Utc)
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
        var fee = CreateFeeConfigEntity();

        var dto = new UpdateFeeConfigDto(
            "Updated broker fee",
            FeeType.BrokerCommission,
            5.5000m,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            false);

        _repositoryMock
            .Setup(x => x.GetFeeConfigForUpdateAsync(
                fee.FeeConfigId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(fee);

        // Act
        var result = await _service.UpdateFeeConfigAsync(
            fee.FeeConfigId,
            dto,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Name, result.Value.Name);
        Assert.Equal(dto.FeeType, result.Value.FeeType);
        Assert.Equal(dto.Percentage, result.Value.Percentage);
        Assert.False(result.Value.IsActive);
        Assert.NotNull(fee.ModifiedAt);

        _repositoryMock.Verify(
            x => x.SaveFeeConfigChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateFeeConfigAsync_EmptyId_ReturnsValidationError()
    {
        // Arrange
        var dto = CreateValidUpdateFeeConfigDto();

        // Act
        var result = await _service.UpdateFeeConfigAsync(
            Guid.Empty,
            dto,
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
        var dto = CreateValidUpdateFeeConfigDto();

        _repositoryMock
            .Setup(x => x.GetFeeConfigForUpdateAsync(
                feeConfigId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((FeeConfig?)null);

        // Act
        var result = await _service.UpdateFeeConfigAsync(
            feeConfigId,
            dto,
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
        var dto = CreateValidUpdateFeeConfigDto() with
        {
            EffectiveFrom = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            EffectiveTo = new DateTime(2026, 5, 31, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var result = await _service.UpdateFeeConfigAsync(
            Guid.NewGuid(),
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

    private static CreateFeeConfigDto CreateValidFeeConfigDto()
    {
        return new CreateFeeConfigDto(
            "Standard broker fee",
            FeeType.BrokerCommission,
            2.5000m,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            true);
    }

    private static UpdateFeeConfigDto CreateValidUpdateFeeConfigDto()
    {
        return new UpdateFeeConfigDto(
            "Standard broker fee",
            FeeType.BrokerCommission,
            2.5000m,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            true);
    }

    private static FeeConfig CreateFeeConfigEntity(
        Guid? feeConfigId = null,
        string name = "Standard broker fee",
        FeeType feeType = FeeType.BrokerCommission)
    {
        return new FeeConfig
        {
            FeeConfigId = feeConfigId ?? Guid.NewGuid(),
            Name = name,
            FeeType = feeType,
            Percentage = 2.5000m,
            EffectiveFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EffectiveTo = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
