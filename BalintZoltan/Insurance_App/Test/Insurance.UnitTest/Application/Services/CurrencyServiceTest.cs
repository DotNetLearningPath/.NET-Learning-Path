using Insurance.Application.DTO.Common;
using Insurance.Application.DTO.Currencies;
using Insurance.Application.Exceptions;
using Insurance.Application.Services;
using Insurance.Domain.Entities;
using Insurance.UnitTest.Application.Fakes;

namespace Insurance.UnitTest.Application.Services;

public sealed class CurrencyServiceTest
{
    private readonly FakeCurrencyRepository _repository;
    private readonly CurrencyService _service;

    public CurrencyServiceTest()
    {
        _repository = new FakeCurrencyRepository();
        _service = new CurrencyService(_repository);
    }

    [Fact]
    public async Task CreateCurrencyAsync_Should_Add_Currency_And_Return_Dto()
    {
        // Arrange
        var request = new CreateCurrencyRequest
        {
            Code = " usd ",
            Name = " United States Dollar ",
            ExchangeRateToBase = 1.25m,
            IsActive = true
        };

        // Act
        var result = await _service.CreateCurrencyAsync(request, CancellationToken.None);

        // Assert
        var savedCurrency = Assert.Single(_repository.Storage);
        Assert.Equal(savedCurrency.Id, result.Id);
        Assert.Equal("USD", result.Code);
        Assert.Equal("United States Dollar", result.Name);
        Assert.Equal(1.25m, result.ExchangeRateToBase);
        Assert.True(result.IsActive);
        Assert.Equal(1, _repository.AddCallCount);
    }

    [Fact]
    public async Task CreateCurrencyAsync_Should_Reject_Duplicate_Code()
    {
        // Arrange
        await _repository.AddCurrencyAsync(
            new Currency("USD", "Existing Dollar", 1.25m),
            CancellationToken.None);

        var request = new CreateCurrencyRequest
        {
            Code = " usd ",
            Name = "Another Dollar",
            ExchangeRateToBase = 1.3m
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateCurrencyAsync(request, CancellationToken.None));

        // Assert
        Assert.Equal("A currency with this code already exists.", exception.Message);
        Assert.Single(_repository.Storage);
    }

    [Fact]
    public async Task GetCurrencyByIdAsync_Should_Return_Mapped_Dto_When_Found()
    {
        // Arrange
        var currency = await AddCurrencyAsync();

        // Act
        var result = await _service.GetCurrencyByIdAsync(currency.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(currency.Id, result.Id);
        Assert.Equal(currency.Code, result.Code);
        Assert.Equal(currency.Name, result.Name);
    }

    [Fact]
    public async Task GetCurrencyByIdAsync_Should_Return_Null_When_Not_Found()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = await _service.GetCurrencyByIdAsync(id, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ListCurrenciesAsync_Should_Return_Mapped_Paged_Result()
    {
        // Arrange
        await AddCurrencyAsync("USD", "United States Dollar", 1.25m);
        await AddCurrencyAsync("EUR", "Euro", 0.92m);

        var pagination = new PaginationRequest
        {
            PageNumber = 1,
            PageSize = 1
        };

        // Act
        var result = await _service.ListCurrenciesAsync(pagination, CancellationToken.None);

        // Assert
        var currency = Assert.Single(result.Items);
        Assert.Equal("EUR", currency.Code);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(2, result.TotalCount);
    }

    [Fact]
    public async Task UpdateCurrencyAsync_Should_Update_Currency_And_Return_Dto()
    {
        // Arrange
        var currency = await AddCurrencyAsync();
        var request = new UpdateCurrencyRequest
        {
            Code = " eur ",
            Name = " Euro ",
            ExchangeRateToBase = 0.92m
        };

        // Act
        var result = await _service.UpdateCurrencyAsync(
            currency.Id,
            request,
            CancellationToken.None);

        // Assert
        Assert.Equal(currency.Id, result.Id);
        Assert.Equal("EUR", result.Code);
        Assert.Equal("Euro", result.Name);
        Assert.Equal(0.92m, result.ExchangeRateToBase);
        Assert.Equal(1, _repository.UpdateCallCount);
    }

    [Fact]
    public async Task UpdateCurrencyAsync_Should_Throw_When_Currency_Does_Not_Exist()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateCurrencyAsync(id, CreateUpdateRequest(), CancellationToken.None));

        // Assert
        Assert.Equal("Currency was not found.", exception.Message);
        Assert.Equal(0, _repository.UpdateCallCount);
    }

    [Fact]
    public async Task UpdateCurrencyAsync_Should_Reject_Duplicate_Code()
    {
        // Arrange
        var firstCurrency = await AddCurrencyAsync("USD", "United States Dollar", 1.25m);
        var secondCurrency = await AddCurrencyAsync("EUR", "Euro", 0.92m);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateCurrencyAsync(
                secondCurrency.Id,
                new UpdateCurrencyRequest
                {
                    Code = firstCurrency.Code,
                    Name = "Updated Euro",
                    ExchangeRateToBase = 0.91m
                },
                CancellationToken.None));

        // Assert
        Assert.Equal("A currency with this code already exists.", exception.Message);
        Assert.Equal("EUR", secondCurrency.Code);
        Assert.Equal(0, _repository.UpdateCallCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Active_State_Commands_Should_Set_Expected_State(bool activate)
    {
        // Arrange
        var currency = await AddCurrencyAsync(
            "USD",
            "United States Dollar",
            1.25m,
            !activate);

        // Act
        var result = activate
            ? await _service.ActivateCurrencyAsync(currency.Id, CancellationToken.None)
            : await _service.DeactivateCurrencyAsync(currency.Id, CancellationToken.None);

        // Assert
        Assert.Equal(activate, result.IsActive);
        Assert.Equal(1, _repository.UpdateCallCount);
    }

    private async Task<Currency> AddCurrencyAsync(
        string code = "USD",
        string name = "United States Dollar",
        decimal exchangeRate = 1.25m,
        bool isActive = true)
    {
        var currency = new Currency(code, name, exchangeRate, isActive);
        await _repository.AddCurrencyAsync(currency, CancellationToken.None);
        return currency;
    }

    private static UpdateCurrencyRequest CreateUpdateRequest() => new()
    {
        Code = "EUR",
        Name = "Euro",
        ExchangeRateToBase = 0.92m
    };
}
