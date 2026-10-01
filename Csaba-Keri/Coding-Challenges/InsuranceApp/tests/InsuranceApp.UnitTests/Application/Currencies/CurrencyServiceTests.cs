using FluentValidation;
using FluentValidation.Results;
using InsuranceApp.Application.Common.Exceptions;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Common.Validation;
using InsuranceApp.Application.Currencies;
using InsuranceApp.Application.Currencies.Commands;
using InsuranceApp.Domain.Currencies;
using InsuranceApp.UnitTests.Common;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace InsuranceApp.UnitTests.Application.Currencies;

public sealed class CurrencyServiceTests
{
    private readonly ICurrencyRepository _currencyRepository;
    private readonly IValidator<CreateCurrencyCommand> _createValidator;
    private readonly IValidator<UpdateCurrencyCommand> _updateValidator;
    private readonly IValidator<PageQuery> _pageValidator;
    private readonly ILogger<CurrencyService> _logger;

    private readonly CurrencyService _currencyService;

    public CurrencyServiceTests()
    {
        _currencyRepository = Substitute.For<ICurrencyRepository>();

        _createValidator = ValidatorMocks.CreatePassing<CreateCurrencyCommand>();
        _updateValidator = ValidatorMocks.CreatePassing<UpdateCurrencyCommand>();
        _pageValidator = ValidatorMocks.CreatePassing<PageQuery>();

        _logger = Substitute.For<ILogger<CurrencyService>>();

        _currencyService = new(_currencyRepository, _createValidator, _updateValidator, _pageValidator, _logger);
    }

    [Fact]
    public async Task CreateCurrencyAsync_ValidCommand_SavesCurrencyAndReturnsResult()
    {
        // Arrange
        var command = CreateCommand(
            code: " eur ",
            name: "Euro",
            exchangeRateToBase: 5m,
            isActive: true
        );

        // Act
        var result = await _currencyService.CreateCurrencyAsync(command, CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("EUR", result.Code);
        Assert.Equal("Euro", result.Name);
        Assert.Equal(5m, result.ExchangeRateToBase);
        Assert.True(result.IsActive);

        await _createValidator.Received(1).ValidateAsync(
            Arg.Is<IValidationContext>(context =>
                ReferenceEquals(context.InstanceToValidate, command) && context.ThrowOnFailures
            ),
            CancellationToken.None
        );
        
        await _currencyRepository.Received(1).AddCurrencyAsync(
            Arg.Is<Currency>(received =>
                received.Id == result.Id
                && received.Code == "EUR"
            ),
            CancellationToken.None
        );
    }

    [Fact]
    public async Task CreateCurrencyAsync_ValidationFails_DoesNotAccessRepository()
    {
        // Arrange
        var command = CreateCommand(
            code: "EUR",
            name: "Euro",
            exchangeRateToBase: -1m,
            isActive: true
        );

        var exception = ValidationExceptionFactory.Create("ExchangeRateToBase", "Invalid rate.");

        _createValidator.ValidateAsync(Arg.Any<IValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<ValidationResult>(exception));

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _currencyService.CreateCurrencyAsync(command, CancellationToken.None)
        );
        
        Assert.Empty(_currencyRepository.ReceivedCalls());
    }

    [Fact]
    public async Task UpdateCurrencyAsync_ValidCommand_PreservesIdentityAndUpdatesValues()
    {
        // Arrange
        var currency = CreateDomainCurrency();

        var command = UpdateCommand(
            currencyId: currency.Id,
            name: "Updated",
            exchangeRateToBase: 5.25m,
            isActive: false
        );

        _currencyRepository.GetCurrencyByIdAsync(currency.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Currency?>(currency));

        // Act
        var result = await _currencyService.UpdateCurrencyAsync(command, CancellationToken.None);

        // Assert
        Assert.Equal(currency.Id, result.Id);
        Assert.Equal(currency.Code, result.Code);
        Assert.Equal("Updated", result.Name);
        Assert.Equal(5.25m, result.ExchangeRateToBase);
        Assert.False(result.IsActive);

        await _updateValidator.Received(1).ValidateAsync(
            Arg.Is<IValidationContext>(context =>
                ReferenceEquals(context.InstanceToValidate, command) && context.ThrowOnFailures
            ),
            CancellationToken.None
        );

        await _currencyRepository.Received(1).UpdateCurrencyAsync(currency, CancellationToken.None);
    }

    [Fact]
    public async Task UpdateCurrencyAsync_BaseCurrencyRateNotOne_ThrowsValidationErrorWithoutSaving()
    {
        // Arrange
        var currency = CreateDomainCurrency(
            code: CurrencyRules.BaseCurrencyCode,
            name: "Base Currency",
            exchangeRateToBase: 1m,
            isActive: true
        );

        var command = UpdateCommand(
            currencyId: currency.Id,
            name: "Changed",
            exchangeRateToBase: 2m,
            isActive: false
        );

        _currencyRepository.GetCurrencyByIdAsync(currency.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Currency?>(currency));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => _currencyService.UpdateCurrencyAsync(command, CancellationToken.None)
        );
        
        Assert.Contains(exception.Errors, error => error.PropertyName == "ExchangeRateToBase");
        Assert.Equal("Base Currency", currency.Name);
        Assert.True(currency.IsActive);
        
        await _currencyRepository.DidNotReceive().UpdateCurrencyAsync(Arg.Any<Currency>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCurrencyByIdAsync_ExistingId_ReturnsAllFields()
    {
        // Arrange
        var currency = CreateDomainCurrency();

        _currencyRepository.GetCurrencyByIdAsync(currency.Id, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<Currency?>(currency));

        // Act
        var result = await _currencyService.GetCurrencyByIdAsync(currency.Id, CancellationToken.None);

        // Assert
        Assert.Equal(currency.Id, result.Id);
        Assert.Equal(currency.Code, result.Code);
        Assert.Equal(currency.Name, result.Name);
        Assert.Equal(currency.ExchangeRateToBase, result.ExchangeRateToBase);
        Assert.Equal(currency.IsActive, result.IsActive);
    }

    [Fact]
    public async Task GetCurrencyByIdAsync_UnknownId_ThrowsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<EntityNotFoundException>(
            () => _currencyService.GetCurrencyByIdAsync(id, CancellationToken.None)
        );
    }

    [Fact]
    public async Task GetCurrencyByIdAsync_EmptyId_DoesNotAccessRepository()
    {
        // Arrange
        var id = Guid.Empty;

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(
            () => _currencyService.GetCurrencyByIdAsync(id, CancellationToken.None)
        );

        Assert.Empty(_currencyRepository.ReceivedCalls());
    }

    [Fact]
    public async Task GetCurrenciesAsync_ValidQuery_PreservesPagination()
    {
        // Arrange
        var currency = CreateDomainCurrency();

        var pageNumber = 2;
        var pageSize = 1;
        var totalCount = 3L;

        var query = new PageQuery(pageNumber, pageSize);
        var page = new PagedResult<Currency>([currency], pageNumber, pageSize, totalCount);

        _currencyRepository.GetCurrenciesAsync(query, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(page));

        // Act
        var result = await _currencyService.GetCurrenciesAsync(query, CancellationToken.None);

        // Assert
        Assert.Equal(pageNumber, result.PageNumber);
        Assert.Equal(pageSize, result.PageSize);
        Assert.Equal(totalCount, result.TotalCount);
        Assert.Equal(currency.Id, Assert.Single(result.Items).Id);

        await _pageValidator.Received(1).ValidateAsync(
            Arg.Is<IValidationContext>(context =>
                ReferenceEquals(context.InstanceToValidate, query) && context.ThrowOnFailures
            ),
            CancellationToken.None
        );
    }

    private static CreateCurrencyCommand CreateCommand(
        string code = "EUR",
        string name = "Euro",
        decimal exchangeRateToBase = 5m,
        bool isActive = true
    )
    {
        return new(
            Code: code,
            Name: name,
            ExchangeRateToBase: exchangeRateToBase,
            IsActive: isActive
        );
    }

    private static UpdateCurrencyCommand UpdateCommand(
        Guid currencyId,
        string name = "Updated",
        decimal exchangeRateToBase = 5.28m,
        bool isActive = true
    )
    {
        return new(
            CurrencyId: currencyId,
            Name: name,
            ExchangeRateToBase: exchangeRateToBase,
            IsActive: isActive
        );
    }

    private static Currency CreateDomainCurrency(
        Guid? currencyId = null,
        string code = "EUR",
        string name = "Euro",
        decimal exchangeRateToBase = 5m,
        bool isActive = true
    )
    {
        return new(
            id: currencyId ?? Guid.NewGuid(),
            code: code,
            name: name,
            exchangeRateToBase: exchangeRateToBase,
            isActive: isActive
        );
    }
}
