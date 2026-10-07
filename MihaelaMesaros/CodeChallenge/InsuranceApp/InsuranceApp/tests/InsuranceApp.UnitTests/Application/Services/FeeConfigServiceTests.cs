using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.FeeConfig;
using InsuranceApp.Application.Services;
using InsuranceApp.Domain.Constants;
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

    public FeeConfigServiceTests()
    {
        _repositoryMock = new Mock<IFeeConfigRepository>();
        _loggerMock = new Mock<ILogger<FeeConfigService>>();

        _service = new FeeConfigService(_repositoryMock.Object, _loggerMock.Object);
    }

    #region Read Fee Config Tests

    [Fact]
    public async Task GetFeeConfigsAsync_ReturnsFeeConfigs()
    {
        // Arrange
        var feeConfig1 = TestData.CreateFeeConfig1();
        var feeConfig2 = TestData.CreateFeeConfig2();
        var feeConfigs = new List<FeeConfig> { feeConfig1, feeConfig2 };

        _repositoryMock.Setup(x => x.GetFeeConfigsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(feeConfigs);

        // Act
        var result = await _service.GetFeeConfigsAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(feeConfigs.Count, result.Value.Count);
        Assert.Equal(feeConfig1.Name, result.Value[0].Name);
        Assert.Equal(feeConfig2.Name, result.Value[1].Name);
    }

    [Fact]
    public async Task GetFeeConfigByIdAsync_ExistingFeeConfig_ReturnsSuccess()
    {
        // Arrange
        var feeConfig = TestData.CreateFeeConfig1();

        _repositoryMock.Setup(x => x.GetFeeConfigByIdAsync(feeConfig.FeeConfigId, It.IsAny<CancellationToken>())).ReturnsAsync(feeConfig);

        // Act
        var result = await _service.GetFeeConfigByIdAsync(feeConfig.FeeConfigId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(feeConfig.FeeConfigId, result.Value.FeeConfigId);
        Assert.Equal(feeConfig.Name, result.Value.Name);
        Assert.Equal(feeConfig.FeeType, result.Value.FeeType);
        Assert.Equal(feeConfig.Percentage, result.Value.Percentage);
        Assert.Equal(feeConfig.EffectiveFrom, result.Value.EffectiveFrom);
        Assert.Equal(feeConfig.EffectiveTo, result.Value.EffectiveTo);
        Assert.Equal(feeConfig.IsActive, result.Value.IsActive);
    }

    [Fact]
    public async Task GetFeeConfigByIdAsync_NonExistingFeeConfig_ReturnsNotFound()
    {
        // Arrange
        var feeConfigId = TestData.NonExistingId;

        _repositoryMock.Setup(x => x.GetFeeConfigByIdAsync(feeConfigId, It.IsAny<CancellationToken>())).ReturnsAsync((FeeConfig?)null);

        // Act
        var result = await _service.GetFeeConfigByIdAsync(feeConfigId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(FeeConfigErrors.NotFound(feeConfigId).Code, result.Error.Code);
    }

    [Fact]
    public async Task GetFeeConfigByIdAsync_EmptyId_ReturnsValidationError()
    {
        // Arrange
        var feeConfigId = Guid.Empty;

        // Act
        var result = await _service.GetFeeConfigByIdAsync(feeConfigId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(FeeConfigErrors.InvalidFeeConfigId.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.GetFeeConfigByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Create Fee Config Tests

    [Fact]
    public async Task CreateFeeConfigAsync_ValidFeeConfig_ReturnsSuccess()
    {
        // Arrange
        var feeConfigDto = TestData.CreateFeeConfigDto();

        // Act
        var result = await _service.CreateFeeConfigAsync(feeConfigDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(feeConfigDto.Name, result.Value.Name);
        Assert.Equal(feeConfigDto.FeeType, result.Value.FeeType);
        Assert.Equal(feeConfigDto.Percentage, result.Value.Percentage);
        Assert.Equal(feeConfigDto.EffectiveFrom, result.Value.EffectiveFrom);
        Assert.Equal(feeConfigDto.EffectiveTo, result.Value.EffectiveTo);
        Assert.Equal(feeConfigDto.IsActive, result.Value.IsActive);

        _repositoryMock.Verify(x => x.AddFeeConfigAsync(
            It.Is<FeeConfig>(feeConfig =>
                feeConfig.Name == feeConfigDto.Name &&
                feeConfig.FeeType == feeConfigDto.FeeType &&
                feeConfig.Percentage == feeConfigDto.Percentage &&
                feeConfig.EffectiveFrom == feeConfigDto.EffectiveFrom &&
                feeConfig.EffectiveTo == feeConfigDto.EffectiveTo &&
                feeConfig.IsActive == feeConfigDto.IsActive),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateFeeConfigAsync_ValidFeeConfig_TrimsName()
    {
        // Arrange
        var feeConfigDto = TestData.CreateFeeConfigDto();

        var dto = feeConfigDto with
        {
            Name = $"  {feeConfigDto.Name}  "
        };

        // Act
        var result = await _service.CreateFeeConfigAsync(dto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(feeConfigDto.Name, result.Value.Name);

        _repositoryMock.Verify(x => x.AddFeeConfigAsync(It.Is<FeeConfig>(feeConfig => feeConfig.Name == feeConfigDto.Name), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateFeeConfigAsync_MissingName_ReturnsValidationError()
    {
        // Arrange
        var feeConfigDto = TestData.CreateFeeConfigDto() with { Name = "" };

        // Act & Assert
        await AssertInvalidCreateFeeConfigAsync(feeConfigDto, FeeConfigErrors.NameRequired);
    }

    [Fact]
    public async Task CreateFeeConfigAsync_NameTooShort_ReturnsValidationError()
    {
        // Arrange
        var feeConfigDto = TestData.CreateFeeConfigDto() with { Name = new string('A', FeeConfigConstraints.NameMinLength - 1) };

        // Act & Assert
        await AssertInvalidCreateFeeConfigAsync(feeConfigDto, FeeConfigErrors.InvalidNameLength);
    }

    [Fact]
    public async Task CreateFeeConfigAsync_NameTooLong_ReturnsValidationError()
    {
        // Arrange
        var feeConfigDto = TestData.CreateFeeConfigDto() with { Name = new string('A', FeeConfigConstraints.NameMaxLength + 1) };

        // Act & Assert
        await AssertInvalidCreateFeeConfigAsync(feeConfigDto, FeeConfigErrors.InvalidNameLength);
    }

    [Fact]
    public async Task CreateFeeConfigAsync_InvalidFeeType_ReturnsValidationError()
    {
        // Arrange
        var feeConfigDto = TestData.CreateFeeConfigDto() with { FeeType = (FeeType)999 };

        // Act & Assert
        await AssertInvalidCreateFeeConfigAsync(feeConfigDto, FeeConfigErrors.InvalidType);
    }

    [Fact]
    public async Task CreateFeeConfigAsync_PercentageBelowMin_ReturnsValidationError()
    {
        // Arrange
        var feeConfigDto = TestData.CreateFeeConfigDto() with { Percentage = FeeConfigConstraints.MinPercentage - 1m };

        // Act & Assert
        await AssertInvalidCreateFeeConfigAsync(feeConfigDto, FeeConfigErrors.InvalidPercentage);
    }

    [Fact]
    public async Task CreateFeeConfigAsync_PercentageAboveMax_ReturnsValidationError()
    {
        // Arrange
        var feeConfigDto = TestData.CreateFeeConfigDto() with { Percentage = FeeConfigConstraints.MaxPercentage + 1m };

        // Act & Assert
        await AssertInvalidCreateFeeConfigAsync(feeConfigDto, FeeConfigErrors.InvalidPercentage);
    }

    [Fact]
    public async Task CreateFeeConfigAsync_PercentageWithTooManyDecimals_ReturnsValidationError()
    {
        // Arrange
        var feeConfigDto = TestData.CreateFeeConfigDto() with { Percentage = TestData.InvalidFeePercentageScale };

        // Act & Assert
        await AssertInvalidCreateFeeConfigAsync(feeConfigDto, FeeConfigErrors.InvalidPercentageScale);
    }

    [Fact]
    public async Task CreateFeeConfigAsync_InvalidEffectivePeriod_ReturnsValidationError()
    {
        // Arrange
        var feeConfigDto = TestData.CreateFeeConfigDto();

        var dto = feeConfigDto with
        {
            EffectiveFrom = TestData.FeeEffectiveTo,
            EffectiveTo = TestData.FeeEffectiveFrom
        };

        // Act & Assert
        await AssertInvalidCreateFeeConfigAsync(dto, FeeConfigErrors.InvalidEffectivePeriod);
    }

    #endregion

    #region Update Fee Config Tests

    [Fact]
    public async Task UpdateFeeConfigAsync_ValidFeeConfig_ReturnsUpdatedFeeConfig()
    {
        // Arrange
        var feeConfig = TestData.CreateFeeConfig1();
        var feeConfigDto = TestData.UpdateFeeConfigDto();

        SetupExistingFeeConfigForUpdate(feeConfig);

        // Act
        var result = await _service.UpdateFeeConfigAsync(feeConfig.FeeConfigId, feeConfigDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(feeConfigDto.Name, result.Value.Name);
        Assert.Equal(feeConfigDto.FeeType, result.Value.FeeType);
        Assert.Equal(feeConfigDto.Percentage, result.Value.Percentage);
        Assert.Equal(feeConfigDto.EffectiveFrom, result.Value.EffectiveFrom);
        Assert.Equal(feeConfigDto.EffectiveTo, result.Value.EffectiveTo);
        Assert.Equal(feeConfigDto.IsActive, result.Value.IsActive);
        Assert.NotNull(feeConfig.ModifiedAt);

        _repositoryMock.Verify(x => x.SaveFeeConfigChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateFeeConfigAsync_EmptyId_ReturnsValidationError()
    {
        // Arrange
        var feeConfigId = Guid.Empty;
        var feeConfigDto = TestData.UpdateFeeConfigDto();

        // Act
        var result = await _service.UpdateFeeConfigAsync(feeConfigId, feeConfigDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(FeeConfigErrors.InvalidFeeConfigId.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.GetFeeConfigForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.SaveFeeConfigChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateFeeConfigAsync_NonExistingFeeConfig_ReturnsNotFound()
    {
        // Arrange
        var feeConfigId = TestData.NonExistingId;
        var feeConfigDto = TestData.UpdateFeeConfigDto();

        _repositoryMock.Setup(x => x.GetFeeConfigForUpdateAsync(feeConfigId, It.IsAny<CancellationToken>())).ReturnsAsync((FeeConfig?)null);

        // Act
        var result = await _service.UpdateFeeConfigAsync(feeConfigId, feeConfigDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(FeeConfigErrors.NotFound(feeConfigId).Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SaveFeeConfigChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateFeeConfigAsync_InvalidEffectivePeriod_ReturnsValidationError()
    {
        // Arrange
        var feeConfig = TestData.CreateFeeConfig1();
        var updateFeeConfigDto = TestData.UpdateFeeConfigDto();

        var feeConfigDto = updateFeeConfigDto with
        {
            EffectiveFrom = TestData.FeeEffectiveTo,
            EffectiveTo = TestData.FeeEffectiveFrom
        };

        // Act & Assert
        await AssertInvalidUpdateFeeConfigAsync(feeConfig, feeConfigDto, FeeConfigErrors.InvalidEffectivePeriod);
    }

    #endregion


    #region Helpers

    private void SetupExistingFeeConfigForUpdate(FeeConfig feeConfig)
    {
        _repositoryMock.Setup(x => x.GetFeeConfigForUpdateAsync(feeConfig.FeeConfigId, It.IsAny<CancellationToken>())).ReturnsAsync(feeConfig);
    }

    private async Task AssertInvalidCreateFeeConfigAsync(CreateFeeConfigDto feeConfigDto, Error expectedError)
    {
        var result = await _service.CreateFeeConfigAsync(feeConfigDto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(expectedError.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.AddFeeConfigAsync(It.IsAny<FeeConfig>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private async Task AssertInvalidUpdateFeeConfigAsync(FeeConfig feeConfig, UpdateFeeConfigDto feeConfigDto, Error expectedError)
    {
        var result = await _service.UpdateFeeConfigAsync(feeConfig.FeeConfigId, feeConfigDto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(expectedError.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.GetFeeConfigForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.SaveFeeConfigChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
