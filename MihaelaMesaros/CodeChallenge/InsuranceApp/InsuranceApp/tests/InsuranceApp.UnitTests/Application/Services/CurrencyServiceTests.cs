using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Currency;
using InsuranceApp.Application.Exceptions;
using InsuranceApp.Application.Services;
using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using InsuranceApp.UnitTests.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace InsuranceApp.UnitTests.Application.Services;

public sealed class CurrencyServiceTests
{
    private readonly Mock<ICurrencyRepository> _repositoryMock;
    private readonly Mock<ILogger<CurrencyService>> _loggerMock;
    private readonly CurrencyService _service;

    public CurrencyServiceTests()
    {
        _repositoryMock = new Mock<ICurrencyRepository>();
        _loggerMock = new Mock<ILogger<CurrencyService>>();

        _service = new CurrencyService(_repositoryMock.Object, _loggerMock.Object);
    }

    #region Read Currency Tests

    [Fact]
    public async Task GetCurrenciesAsync_ReturnsCurrencies()
    {
        // Arrange
        var currency1 = TestData.CreateCurrency1();
        var currency2 = TestData.CreateCurrency2();
        var currencies = new List<Currency> { currency1, currency2 };

        _repositoryMock.Setup(x => x.GetCurrenciesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(currencies);

        // Act
        var result = await _service.GetCurrenciesAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(currencies.Count, result.Value.Count);
        Assert.Equal(currency1.Code, result.Value[0].Code);
        Assert.Equal(currency2.Code, result.Value[1].Code);
    }

    [Fact]
    public async Task GetCurrencyByIdAsync_ExistingCurrency_ReturnsSuccess()
    {
        // Arrange
        var currency = TestData.CreateCurrency1();

        _repositoryMock.Setup(x => x.GetCurrencyByIdAsync(currency.CurrencyId, It.IsAny<CancellationToken>())).ReturnsAsync(currency);

        // Act
        var result = await _service.GetCurrencyByIdAsync(currency.CurrencyId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(currency.CurrencyId, result.Value.CurrencyId);
        Assert.Equal(currency.Code, result.Value.Code);
        Assert.Equal(currency.Name, result.Value.Name);
        Assert.Equal(currency.ExchangeRateToBase, result.Value.ExchangeRateToBase);
        Assert.Equal(currency.IsActive, result.Value.IsActive);
    }

    [Fact]
    public async Task GetCurrencyByIdAsync_NonExistingCurrency_ReturnsNotFound()
    {
        // Arrange
        var currencyId = TestData.NonExistingId;

        _repositoryMock.Setup(x => x.GetCurrencyByIdAsync(currencyId, It.IsAny<CancellationToken>())).ReturnsAsync((Currency?)null);

        // Act
        var result = await _service.GetCurrencyByIdAsync(currencyId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(CurrencyErrors.NotFound(currencyId).Code, result.Error.Code);
    }

    [Fact]
    public async Task GetCurrencyByIdAsync_EmptyCurrencyId_ReturnsValidationError()
    {
        // Arrange
        var currencyId = Guid.Empty;

        // Act
        var result = await _service.GetCurrencyByIdAsync(currencyId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(CurrencyErrors.InvalidCurrencyId.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.GetCurrencyByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Create Currency Tests

    [Fact]
    public async Task CreateCurrencyAsync_ValidCurrency_ReturnsSuccess()
    {
        // Arrange
        var currencyDto = TestData.CreateCurrencyDto();

        SetupCodeDoesNotExistForCreate(currencyDto);

        // Act
        var result = await _service.CreateCurrencyAsync(currencyDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(currencyDto.Code, result.Value.Code);
        Assert.Equal(currencyDto.Name, result.Value.Name);
        Assert.Equal(currencyDto.ExchangeRateToBase, result.Value.ExchangeRateToBase);
        Assert.Equal(currencyDto.IsActive, result.Value.IsActive);

        _repositoryMock.Verify(x => x.AddCurrencyAsync(
            It.Is<Currency>(currency =>
                currency.Code == currencyDto.Code &&
                currency.Name == currencyDto.Name &&
                currency.ExchangeRateToBase == currencyDto.ExchangeRateToBase &&
                currency.IsActive == currencyDto.IsActive),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateCurrencyAsync_ValidCurrency_NormalizesCodeAndName()
    {
        // Arrange
        var currencyDto = TestData.CreateCurrencyDto();
        var normalizedCurrency = TestData.CreateCurrency2();

        var dto = currencyDto with
        {
            Code = $" {normalizedCurrency.Code.ToLowerInvariant()} ",
            Name = $" {normalizedCurrency.Name} "
        };

        _repositoryMock.Setup(x => x.CurrencyCodeExistsAsync(normalizedCurrency.Code, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

        // Act
        var result = await _service.CreateCurrencyAsync(dto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(normalizedCurrency.Code, result.Value.Code);
        Assert.Equal(normalizedCurrency.Name, result.Value.Name);

        _repositoryMock.Verify(x => x.AddCurrencyAsync(
            It.Is<Currency>(currency =>
                currency.Code == normalizedCurrency.Code &&
                currency.Name == normalizedCurrency.Name),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateCurrencyAsync_MissingCode_ReturnsValidationError()
    {
        // Arrange
        var currencyDto = TestData.CreateCurrencyDto() with { Code = "" };

        // Act & Assert
        await AssertInvalidCreateCurrencyAsync(currencyDto, CurrencyErrors.CodeRequired);
    }

    [Theory]
    [InlineData("R")]
    [InlineData("RO")]
    [InlineData("EURO")]
    public async Task CreateCurrencyAsync_InvalidCodeLength_ReturnsValidationError(string code)
    {
        // Arrange
        var currencyDto = TestData.CreateCurrencyDto() with { Code = code };

        // Act & Assert
        await AssertInvalidCreateCurrencyAsync(currencyDto, CurrencyErrors.InvalidCodeLength);
    }

    [Fact]
    public async Task CreateCurrencyAsync_MissingName_ReturnsValidationError()
    {
        // Arrange
        var currencyDto = TestData.CreateCurrencyDto() with { Name = "" };

        // Act & Assert
        await AssertInvalidCreateCurrencyAsync(currencyDto, CurrencyErrors.NameRequired);
    }

    [Fact]
    public async Task CreateCurrencyAsync_NameTooShort_ReturnsValidationError()
    {
        // Arrange
        var currencyDto = TestData.CreateCurrencyDto() with { Name = new string('A', CurrencyConstraints.NameMinLength - 1) };

        // Act & Assert
        await AssertInvalidCreateCurrencyAsync(currencyDto, CurrencyErrors.InvalidNameLength);
    }

    [Fact]
    public async Task CreateCurrencyAsync_InvalidExchangeRate_ReturnsValidationError()
    {
        // Arrange
        var currencyDto = TestData.CreateCurrencyDto() with { ExchangeRateToBase = CurrencyConstraints.MinExchangeRate - 0.01m };

        // Act & Assert
        await AssertInvalidCreateCurrencyAsync(currencyDto, CurrencyErrors.InvalidExchangeRate);
    }

    [Fact]
    public async Task CreateCurrencyAsync_ExchangeRateWithTooManyDecimals_ReturnsValidationError()
    {
        // Arrange
        var currencyDto = TestData.CreateCurrencyDto() with { ExchangeRateToBase = TestData.InvalidExchangeRateScale };

        // Act & Assert
        await AssertInvalidCreateCurrencyAsync(currencyDto, CurrencyErrors.InvalidExchangeRateScale);
    }

    [Fact]
    public async Task CreateCurrencyAsync_DuplicateCode_ReturnsConflict()
    {
        // Arrange
        var currencyDto = TestData.CreateCurrencyDto();

        _repositoryMock.Setup(x => x.CurrencyCodeExistsAsync(currencyDto.Code, null, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _service.CreateCurrencyAsync(currencyDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(CurrencyErrors.DuplicateCode.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.AddCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateCurrencyAsync_DuplicateOnInsert_ReturnsConflict()
    {
        // Arrange
        var currencyDto = TestData.CreateCurrencyDto();

        SetupCodeDoesNotExistForCreate(currencyDto);

        _repositoryMock.Setup(x => x.AddCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>())).ThrowsAsync(new DuplicateEntityException(nameof(Currency)));

        // Act
        var result = await _service.CreateCurrencyAsync(currencyDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(CurrencyErrors.DuplicateCode.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.AddCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Update Currency Tests

    [Fact]
    public async Task UpdateCurrencyAsync_ValidCurrency_ReturnsUpdatedCurrency()
    {
        // Arrange
        var currency = TestData.CreateCurrency1();
        var currencyDto = TestData.UpdateCurrencyDto();

        SetupExistingCurrencyForUpdate(currency);
        SetupCodeDoesNotExistForUpdate(currencyDto, currency);

        // Act
        var result = await _service.UpdateCurrencyAsync(currency.CurrencyId, currencyDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(currencyDto.Code, result.Value.Code);
        Assert.Equal(currencyDto.Name, result.Value.Name);
        Assert.Equal(currencyDto.ExchangeRateToBase, result.Value.ExchangeRateToBase);
        Assert.Equal(currencyDto.IsActive, result.Value.IsActive);
        Assert.NotNull(currency.ModifiedAt);

        _repositoryMock.Verify(x => x.SaveCurrencyChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateCurrencyAsync_EmptyCurrencyId_ReturnsValidationError()
    {
        // Arrange
        var currencyId = Guid.Empty;
        var currencyDto = TestData.UpdateCurrencyDto();

        // Act
        var result = await _service.UpdateCurrencyAsync(currencyId, currencyDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(CurrencyErrors.InvalidCurrencyId.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.GetCurrencyForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCurrencyAsync_NonExistingCurrency_ReturnsNotFound()
    {
        // Arrange
        var currencyId = TestData.NonExistingId;
        var currencyDto = TestData.UpdateCurrencyDto();

        _repositoryMock.Setup(x => x.GetCurrencyForUpdateAsync(currencyId, It.IsAny<CancellationToken>())).ReturnsAsync((Currency?)null);

        // Act
        var result = await _service.UpdateCurrencyAsync(currencyId, currencyDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(CurrencyErrors.NotFound(currencyId).Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SaveCurrencyChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCurrencyAsync_DuplicateCode_ReturnsConflict()
    {
        // Arrange
        var currency = TestData.CreateCurrency1();
        var currencyDto = TestData.UpdateCurrencyDto();

        SetupExistingCurrencyForUpdate(currency);

        _repositoryMock.Setup(x => x.CurrencyCodeExistsAsync(currencyDto.Code, currency.CurrencyId, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _service.UpdateCurrencyAsync(currency.CurrencyId, currencyDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(CurrencyErrors.DuplicateCode.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SaveCurrencyChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCurrencyAsync_DuplicateOnSave_ReturnsConflict()
    {
        // Arrange
        var currency = TestData.CreateCurrency1();
        var currencyDto = TestData.UpdateCurrencyDto();

        SetupExistingCurrencyForUpdate(currency);
        SetupCodeDoesNotExistForUpdate(currencyDto, currency);

        _repositoryMock.Setup(x => x.SaveCurrencyChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new DuplicateEntityException(nameof(Currency)));

        // Act
        var result = await _service.UpdateCurrencyAsync(currency.CurrencyId, currencyDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(CurrencyErrors.DuplicateCode.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SaveCurrencyChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Helpers

    private void SetupExistingCurrencyForUpdate(Currency currency)
    {
        _repositoryMock.Setup(x => x.GetCurrencyForUpdateAsync(currency.CurrencyId, It.IsAny<CancellationToken>())).ReturnsAsync(currency);
    }

    private void SetupCodeDoesNotExistForCreate(CreateCurrencyDto currencyDto)
    {
        _repositoryMock.Setup(x => x.CurrencyCodeExistsAsync(currencyDto.Code, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);
    }

    private void SetupCodeDoesNotExistForUpdate(UpdateCurrencyDto currencyDto, Currency currency)
    {
        _repositoryMock.Setup(x => x.CurrencyCodeExistsAsync(currencyDto.Code, currency.CurrencyId, It.IsAny<CancellationToken>())).ReturnsAsync(false);
    }

    private async Task AssertInvalidCreateCurrencyAsync(CreateCurrencyDto currencyDto, Error expectedError)
    {
        var result = await _service.CreateCurrencyAsync(currencyDto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(expectedError.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.CurrencyCodeExistsAsync(It.IsAny<string>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.AddCurrencyAsync(It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
