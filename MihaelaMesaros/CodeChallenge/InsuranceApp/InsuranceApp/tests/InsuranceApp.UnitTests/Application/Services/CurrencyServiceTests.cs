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
    private readonly Mock<ILogger<CurrencyService>> _loggerMock;
    private readonly CurrencyService _service;

    private readonly Guid _currencyId;
    private readonly CreateCurrencyDto _validCreateDto;
    private readonly UpdateCurrencyDto _validUpdateDto;
    private readonly Currency _existingCurrency;

    public CurrencyServiceTests()
    {
        _repositoryMock = new Mock<ICurrencyRepository>();
        _loggerMock = new Mock<ILogger<CurrencyService>>();

        _service = new CurrencyService(
            _repositoryMock.Object,
            _loggerMock.Object);

        _currencyId = Guid.NewGuid();

        _validCreateDto = new CreateCurrencyDto(
            "RON",
            "Romanian Leu",
            1.00m,
            true);

        _validUpdateDto = new UpdateCurrencyDto(
            "RON",
            "Romanian Leu",
            1.00m,
            true);

        _existingCurrency = new Currency
        {
            CurrencyId = _currencyId,
            Code = "RON",
            Name = "Romanian Leu",
            ExchangeRateToBase = 1.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    #region Read Currency Tests

    [Fact]
    public async Task GetCurrenciesAsync_ReturnsCurrencies()
    {
        // Arrange
        var euro = new Currency
        {
            CurrencyId = Guid.NewGuid(),
            Code = "EUR",
            Name = "Euro",
            ExchangeRateToBase = 4.97m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var currencies = new List<Currency>
        {
            _existingCurrency,
            euro
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
        SetupExistingCurrencyById();

        // Act
        var result = await _service.GetCurrencyByIdAsync(
            _currencyId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(_currencyId, result.Value.CurrencyId);
        Assert.Equal(_existingCurrency.Code, result.Value.Code);
        Assert.Equal(_existingCurrency.Name, result.Value.Name);
        Assert.Equal(
            _existingCurrency.ExchangeRateToBase,
            result.Value.ExchangeRateToBase);
        Assert.Equal(_existingCurrency.IsActive, result.Value.IsActive);
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
        SetupCodeDoesNotExistForCreate();

        // Act
        var result = await _service.CreateCurrencyAsync(
            _validCreateDto,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(_validCreateDto.Code, result.Value.Code);
        Assert.Equal(_validCreateDto.Name, result.Value.Name);
        Assert.Equal(
            _validCreateDto.ExchangeRateToBase,
            result.Value.ExchangeRateToBase);
        Assert.Equal(_validCreateDto.IsActive, result.Value.IsActive);

        _repositoryMock.Verify(
            x => x.AddCurrencyAsync(
                It.Is<Currency>(currency =>
                    currency.Code == _validCreateDto.Code &&
                    currency.Name == _validCreateDto.Name &&
                    currency.ExchangeRateToBase ==
                        _validCreateDto.ExchangeRateToBase),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateCurrencyAsync_ValidCurrency_NormalizesCodeAndName()
    {
        // Arrange
        var dto = _validCreateDto with
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
        var dto = _validCreateDto with
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
        var dto = _validCreateDto with
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
        var dto = _validCreateDto with
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
        var dto = _validCreateDto with
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
        var dto = _validCreateDto with
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
        var dto = _validCreateDto with
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
        _repositoryMock
            .Setup(x => x.CurrencyCodeExistsAsync(
                _validCreateDto.Code,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateCurrencyAsync(
            _validCreateDto,
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
        SetupCodeDoesNotExistForCreate();

        _repositoryMock
            .Setup(x => x.AddCurrencyAsync(
                It.IsAny<Currency>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new DuplicateEntityException(nameof(Currency)));

        // Act
        var result = await _service.CreateCurrencyAsync(
            _validCreateDto,
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
        var dto = _validUpdateDto with
        {
            Code = "EUR",
            Name = "Euro",
            ExchangeRateToBase = 4.97m
        };

        SetupExistingCurrencyForUpdate();

        _repositoryMock
            .Setup(x => x.CurrencyCodeExistsAsync(
                dto.Code,
                _currencyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.UpdateCurrencyAsync(
            _currencyId,
            dto,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal("EUR", result.Value.Code);
        Assert.Equal("Euro", result.Value.Name);
        Assert.Equal(4.97m, result.Value.ExchangeRateToBase);
        Assert.NotNull(_existingCurrency.ModifiedAt);

        _repositoryMock.Verify(
            x => x.SaveCurrencyChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateCurrencyAsync_EmptyCurrencyId_ReturnsValidationError()
    {
        // Act
        var result = await _service.UpdateCurrencyAsync(
            Guid.Empty,
            _validUpdateDto,
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

        _repositoryMock
            .Setup(x => x.GetCurrencyForUpdateAsync(
                currencyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Currency?)null);

        // Act
        var result = await _service.UpdateCurrencyAsync(
            currencyId,
            _validUpdateDto,
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
        var dto = _validUpdateDto with
        {
            Code = "EUR"
        };

        SetupExistingCurrencyForUpdate();

        _repositoryMock
            .Setup(x => x.CurrencyCodeExistsAsync(
                "EUR",
                _currencyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.UpdateCurrencyAsync(
            _currencyId,
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
        SetupExistingCurrencyForUpdate();
        SetupCodeDoesNotExistForUpdate();

        _repositoryMock
            .Setup(x => x.SaveCurrencyChangesAsync(
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new DuplicateEntityException(nameof(Currency)));

        // Act
        var result = await _service.UpdateCurrencyAsync(
            _currencyId,
            _validUpdateDto,
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

    private void SetupExistingCurrencyById()
    {
        _repositoryMock
            .Setup(x => x.GetCurrencyByIdAsync(
                _currencyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingCurrency);
    }

    private void SetupExistingCurrencyForUpdate()
    {
        _repositoryMock
            .Setup(x => x.GetCurrencyForUpdateAsync(
                _currencyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingCurrency);
    }

    private void SetupCodeDoesNotExistForCreate()
    {
        _repositoryMock
            .Setup(x => x.CurrencyCodeExistsAsync(
                _validCreateDto.Code,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private void SetupCodeDoesNotExistForUpdate()
    {
        _repositoryMock
            .Setup(x => x.CurrencyCodeExistsAsync(
                _validUpdateDto.Code,
                _currencyId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

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

    #endregion
}
