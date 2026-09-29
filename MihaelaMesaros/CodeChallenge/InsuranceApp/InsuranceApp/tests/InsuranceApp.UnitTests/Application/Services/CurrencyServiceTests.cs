using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Currency;
using InsuranceApp.Application.Exceptions;
using InsuranceApp.Application.Services;
using InsuranceApp.Domain.Entities;
using InsuranceApp.UnitTests.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace InsuranceApp.UnitTests.Application.Services;

public sealed class CurrencyServiceTests
{
    private readonly Mock<ICurrencyRepository> _repositoryMock;
    private readonly CurrencyService _service;

    public CurrencyServiceTests()
    {
        _repositoryMock = new Mock<ICurrencyRepository>();

        var loggerMock = new Mock<ILogger<CurrencyService>>();

        _service = new CurrencyService(
            _repositoryMock.Object,
            loggerMock.Object);
    }

    #region Read Currency Tests

    [Fact]
    public async Task GetCurrenciesAsync_ReturnsCurrencies()
    {
        // Arrange
        var currencies = new List<Currency>
        {
            CreateCurrencyEntity(
                code: "RON",
                name: "Romanian Leu"),

            CreateCurrencyEntity(
                code: "EUR",
                name: "Euro",
                exchangeRate: 4.97m)
        };

        _repositoryMock
            .Setup(x => x.GetCurrenciesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currencies);

        // Act
        var result = await _service.GetCurrenciesAsync(
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal("RON", result.Value[0].Code);
        Assert.Equal("EUR", result.Value[1].Code);
    }

    [Fact]
    public async Task GetCurrencyByIdAsync_ExistingCurrency_ReturnsSuccess()
    {
        // Arrange
        var currency = CreateCurrencyEntity();

        _repositoryMock
            .Setup(x => x.GetCurrencyByIdAsync(
                currency.CurrencyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        // Act
        var result = await _service.GetCurrencyByIdAsync(
            currency.CurrencyId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(currency.CurrencyId, result.Value.CurrencyId);
        Assert.Equal(currency.Code, result.Value.Code);
        Assert.Equal(currency.Name, result.Value.Name);
        Assert.Equal(
            currency.ExchangeRateToBase,
            result.Value.ExchangeRateToBase);
        Assert.Equal(currency.IsActive, result.Value.IsActive);
    }

    [Fact]
    public async Task GetCurrencyByIdAsync_NonExistingCurrency_ReturnsNotFound()
    {
        // Arrange
        var currencyId = TestConstants.NonExistingId;

        _repositoryMock
            .Setup(x => x.GetCurrencyByIdAsync(
                currencyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency?)null);

        // Act
        var result = await _service.GetCurrencyByIdAsync(
            currencyId,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(
            CurrencyErrors.NotFound(currencyId).Code,
            result.Error.Code);
    }

    [Fact]
    public async Task GetCurrencyByIdAsync_EmptyCurrencyId_ReturnsValidationError()
    {
        // Act
        var result = await _service.GetCurrencyByIdAsync(
            Guid.Empty,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(
            CurrencyErrors.InvalidCurrencyId.Code,
            result.Error.Code);

        _repositoryMock.Verify(
            x => x.GetCurrencyByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Create Currency Tests

    [Fact]
    public async Task CreateCurrencyAsync_ValidCurrency_ReturnsSuccess()
    {
        // Arrange
        var dto = CreateValidCurrencyDto();

        _repositoryMock
            .Setup(x => x.CurrencyCodeExistsAsync(
                dto.Code,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.CreateCurrencyAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(dto.Code, result.Value.Code);
        Assert.Equal(dto.Name, result.Value.Name);
        Assert.Equal(
            dto.ExchangeRateToBase,
            result.Value.ExchangeRateToBase);
        Assert.Equal(dto.IsActive, result.Value.IsActive);

        _repositoryMock.Verify(
            x => x.AddCurrencyAsync(
                It.Is<Currency>(currency =>
                    currency.Code == dto.Code &&
                    currency.Name == dto.Name &&
                    currency.ExchangeRateToBase ==
                        dto.ExchangeRateToBase),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateCurrencyAsync_ValidCurrency_NormalizesCodeAndName()
    {
        // Arrange
        var dto = CreateValidCurrencyDto() with
        {
            Code = " eur ",
            Name = " Euro "
        };

        _repositoryMock
            .Setup(x => x.CurrencyCodeExistsAsync(
                "EUR",
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.CreateCurrencyAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("EUR", result.Value!.Code);
        Assert.Equal("Euro", result.Value.Name);

        _repositoryMock.Verify(
            x => x.AddCurrencyAsync(
                It.Is<Currency>(currency =>
                    currency.Code == "EUR" &&
                    currency.Name == "Euro"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateCurrencyAsync_MissingCode_ReturnsValidationError()
    {
        var dto = CreateValidCurrencyDto() with
        {
            Code = ""
        };

        await AssertInvalidCreateAsync(
            dto,
            CurrencyErrors.CodeRequired);
    }

    [Theory]
    [InlineData("R")]
    [InlineData("RO")]
    [InlineData("EURO")]
    public async Task CreateCurrencyAsync_InvalidCodeLength_ReturnsValidationError(
        string code)
    {
        var dto = CreateValidCurrencyDto() with
        {
            Code = code
        };

        await AssertInvalidCreateAsync(
            dto,
            CurrencyErrors.InvalidCodeLength);
    }

    [Fact]
    public async Task CreateCurrencyAsync_MissingName_ReturnsValidationError()
    {
        var dto = CreateValidCurrencyDto() with
        {
            Name = ""
        };

        await AssertInvalidCreateAsync(
            dto,
            CurrencyErrors.NameRequired);
    }

    [Fact]
    public async Task CreateCurrencyAsync_NameTooShort_ReturnsValidationError()
    {
        var dto = CreateValidCurrencyDto() with
        {
            Name = "A"
        };

        await AssertInvalidCreateAsync(
            dto,
            CurrencyErrors.InvalidNameLength);
    }

    [Fact]
    public async Task CreateCurrencyAsync_InvalidExchangeRate_ReturnsValidationError()
    {
        var dto = CreateValidCurrencyDto() with
        {
            ExchangeRateToBase = 0
        };

        await AssertInvalidCreateAsync(
            dto,
            CurrencyErrors.InvalidExchangeRate);
    }

    [Fact]
    public async Task CreateCurrencyAsync_ExchangeRateWithTooManyDecimals_ReturnsValidationError()
    {
        var dto = CreateValidCurrencyDto() with
        {
            ExchangeRateToBase = 4.12345m
        };

        await AssertInvalidCreateAsync(
            dto,
            CurrencyErrors.InvalidExchangeRateScale);
    }

    [Fact]
    public async Task CreateCurrencyAsync_DuplicateCode_ReturnsConflict()
    {
        // Arrange
        var dto = CreateValidCurrencyDto();

        _repositoryMock
            .Setup(x => x.CurrencyCodeExistsAsync(
                dto.Code,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateCurrencyAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal(
            CurrencyErrors.DuplicateCode.Code,
            result.Error.Code);

        _repositoryMock.Verify(
            x => x.AddCurrencyAsync(
                It.IsAny<Currency>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateCurrencyAsync_DuplicateOnInsert_ReturnsConflict()
    {
        // Arrange
        var dto = CreateValidCurrencyDto();

        _repositoryMock
            .Setup(x => x.CurrencyCodeExistsAsync(
                dto.Code,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(x => x.AddCurrencyAsync(
                It.IsAny<Currency>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new DuplicateEntityException(nameof(Currency)));

        // Act
        var result = await _service.CreateCurrencyAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal(
            CurrencyErrors.DuplicateCode.Code,
            result.Error.Code);
    }

    #endregion

    #region Update Currency Tests

    [Fact]
    public async Task UpdateCurrencyAsync_ValidCurrency_ReturnsUpdatedCurrency()
    {
        // Arrange
        var currency = CreateCurrencyEntity();

        var dto = new UpdateCurrencyDto(
            "EUR",
            "Euro",
            4.97m,
            true);

        _repositoryMock
            .Setup(x => x.GetCurrencyForUpdateAsync(
                currency.CurrencyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        _repositoryMock
            .Setup(x => x.CurrencyCodeExistsAsync(
                dto.Code,
                currency.CurrencyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.UpdateCurrencyAsync(
            currency.CurrencyId,
            dto,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal("EUR", result.Value.Code);
        Assert.Equal("Euro", result.Value.Name);
        Assert.Equal(4.97m, result.Value.ExchangeRateToBase);
        Assert.NotNull(currency.ModifiedAt);

        _repositoryMock.Verify(
            x => x.SaveCurrencyChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateCurrencyAsync_EmptyCurrencyId_ReturnsValidationError()
    {
        // Arrange
        var dto = CreateValidUpdateCurrencyDto();

        // Act
        var result = await _service.UpdateCurrencyAsync(
            Guid.Empty,
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(
            CurrencyErrors.InvalidCurrencyId.Code,
            result.Error.Code);

        _repositoryMock.Verify(
            x => x.GetCurrencyForUpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateCurrencyAsync_NonExistingCurrency_ReturnsNotFound()
    {
        // Arrange
        var currencyId = TestConstants.NonExistingId;
        var dto = CreateValidUpdateCurrencyDto();

        _repositoryMock
            .Setup(x => x.GetCurrencyForUpdateAsync(
                currencyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency?)null);

        // Act
        var result = await _service.UpdateCurrencyAsync(
            currencyId,
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Equal(
            CurrencyErrors.NotFound(currencyId).Code,
            result.Error.Code);
    }

    [Fact]
    public async Task UpdateCurrencyAsync_DuplicateCode_ReturnsConflict()
    {
        // Arrange
        var currency = CreateCurrencyEntity();

        var dto = CreateValidUpdateCurrencyDto() with
        {
            Code = "EUR"
        };

        _repositoryMock
            .Setup(x => x.GetCurrencyForUpdateAsync(
                currency.CurrencyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        _repositoryMock
            .Setup(x => x.CurrencyCodeExistsAsync(
                "EUR",
                currency.CurrencyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.UpdateCurrencyAsync(
            currency.CurrencyId,
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal(
            CurrencyErrors.DuplicateCode.Code,
            result.Error.Code);

        _repositoryMock.Verify(
            x => x.SaveCurrencyChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateCurrencyAsync_DuplicateOnSave_ReturnsConflict()
    {
        // Arrange
        var currency = CreateCurrencyEntity();
        var dto = CreateValidUpdateCurrencyDto();

        _repositoryMock
            .Setup(x => x.GetCurrencyForUpdateAsync(
                currency.CurrencyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(currency);

        _repositoryMock
            .Setup(x => x.CurrencyCodeExistsAsync(
                dto.Code,
                currency.CurrencyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(x => x.SaveCurrencyChangesAsync(
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new DuplicateEntityException(nameof(Currency)));

        // Act
        var result = await _service.UpdateCurrencyAsync(
            currency.CurrencyId,
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.Error!.Type);
        Assert.Equal(
            CurrencyErrors.DuplicateCode.Code,
            result.Error.Code);
    }

    #endregion

    #region Helpers

    private async Task AssertInvalidCreateAsync(
        CreateCurrencyDto dto,
        Error expectedError)
    {
        var result = await _service.CreateCurrencyAsync(
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(expectedError.Code, result.Error!.Code);

        _repositoryMock.Verify(
            x => x.AddCurrencyAsync(
                It.IsAny<Currency>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static CreateCurrencyDto CreateValidCurrencyDto()
    {
        return new CreateCurrencyDto(
            "RON",
            "Romanian Leu",
            1.0000m,
            true);
    }

    private static UpdateCurrencyDto CreateValidUpdateCurrencyDto()
    {
        return new UpdateCurrencyDto(
            "RON",
            "Romanian Leu",
            1.0000m,
            true);
    }

    private static Currency CreateCurrencyEntity(
        Guid? currencyId = null,
        string code = "RON",
        string name = "Romanian Leu",
        decimal exchangeRate = 1.0000m)
    {
        return new Currency
        {
            CurrencyId = currencyId ?? Guid.NewGuid(),
            Code = code,
            Name = name,
            ExchangeRateToBase = exchangeRate,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    #endregion
}
