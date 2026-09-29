using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Currency;
using InsuranceApp.IntegrationTests.Common;
using InsuranceApp.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace InsuranceApp.IntegrationTests.Api.Controllers.Admin;

public sealed class CurrencyEndpointsTests(InsuranceAppWebApplicationFactory factory)
    : IntegrationTestBase(factory), IClassFixture<InsuranceAppWebApplicationFactory>
{
    #region Create Currency Tests

    [Fact]
    public async Task CreateCurrency_ValidRequest_ReturnsCreatedAndPersistsCurrency()
    {
        // Arrange
        var currency = TestData.CurrencyForCreate;
        var request = new CreateCurrencyDto(currency.Code, currency.Name, currency.ExchangeRateToBase, currency.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/admin/currencies", request);

        // Assert - HTTP
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdCurrency = await response.Content.ReadFromJsonAsync<CurrencyDto>();

        Assert.NotNull(createdCurrency);
        Assert.NotEqual(Guid.Empty, createdCurrency.CurrencyId);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal($"/api/admin/currencies/{createdCurrency.CurrencyId}", response.Headers.Location.AbsolutePath);
        Assert.Equal(request.Code, createdCurrency.Code);
        Assert.Equal(request.Name, createdCurrency.Name);
        Assert.Equal(request.ExchangeRateToBase, createdCurrency.ExchangeRateToBase);
        Assert.Equal(request.IsActive, createdCurrency.IsActive);

        // Assert - persistence
        var persistedCurrency = await DbContext.Currencies.AsNoTracking().FirstOrDefaultAsync(x => x.CurrencyId == createdCurrency.CurrencyId);

        Assert.NotNull(persistedCurrency);
        Assert.Equal(request.Code, persistedCurrency.Code);
        Assert.Equal(request.Name, persistedCurrency.Name);
        Assert.Equal(request.ExchangeRateToBase, persistedCurrency.ExchangeRateToBase);
        Assert.Equal(request.IsActive, persistedCurrency.IsActive);
    }

    [Fact]
    public async Task CreateCurrency_NormalizesCodeAndName()
    {
        // Arrange
        var currency = TestData.CurrencyForCreate;
        var request = new CreateCurrencyDto($" {currency.Code.ToLowerInvariant()} ", $" {currency.Name} ", currency.ExchangeRateToBase, currency.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/admin/currencies", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdCurrency = await response.Content.ReadFromJsonAsync<CurrencyDto>();

        Assert.NotNull(createdCurrency);
        Assert.Equal(request.Code.Trim().ToUpperInvariant(), createdCurrency.Code);
        Assert.Equal(request.Name.Trim(), createdCurrency.Name);
    }

    [Fact]
    public async Task CreateCurrency_MissingCode_ReturnsBadRequest()
    {
        // Arrange
        var currency = TestData.CurrencyForCreate;
        var request = new CreateCurrencyDto("", currency.Name, currency.ExchangeRateToBase, currency.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/admin/currencies", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal(CurrencyErrors.CodeRequired.Code, problem.Extensions["code"]?.ToString());
    }

    [Fact]
    public async Task CreateCurrency_InvalidCodeLength_ReturnsBadRequest()
    {
        // Arrange
        var currency = TestData.CurrencyForCreate;
        var request = new CreateCurrencyDto(currency.Code[..2], currency.Name, currency.ExchangeRateToBase, currency.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/admin/currencies", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCurrency_MissingName_ReturnsBadRequest()
    {
        // Arrange
        var currency = TestData.CurrencyForCreate;
        var request = new CreateCurrencyDto(currency.Code, "", currency.ExchangeRateToBase, currency.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/admin/currencies", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCurrency_InvalidExchangeRate_ReturnsBadRequest()
    {
        // Arrange
        var currency = TestData.CurrencyForCreate;
        var request = new CreateCurrencyDto(currency.Code, currency.Name, 0m, currency.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/admin/currencies", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCurrency_ExchangeRateWithTooManyDecimals_ReturnsBadRequest()
    {
        // Arrange
        var currency = TestData.CurrencyForCreate;
        var request = new CreateCurrencyDto(currency.Code, currency.Name, 4.12345m, currency.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/admin/currencies", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCurrency_DuplicateCode_ReturnsConflict()
    {
        // Arrange
        var currency = TestData.CurrencyForCreate;
        var request = new CreateCurrencyDto(currency.Code, currency.Name, currency.ExchangeRateToBase, currency.IsActive);

        var firstResponse = await HttpClient.PostAsJsonAsync("/api/admin/currencies", request);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act
        var secondResponse = await HttpClient.PostAsJsonAsync("/api/admin/currencies", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

        var problem = await secondResponse.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status409Conflict, problem.Status);
        Assert.Equal(CurrencyErrors.DuplicateCode.Code, problem.Extensions["code"]?.ToString());

        var count = await DbContext.Currencies.CountAsync(x => x.Code == request.Code);

        Assert.Equal(1, count);
    }

    #endregion

    #region Read Currency Tests

    [Fact]
    public async Task GetCurrencies_ReturnsOkWithCurrencies()
    {
        // Arrange
        await SeedAsync(TestData.CurrenciesList);

        // Act
        var response = await HttpClient.GetAsync("/api/admin/currencies");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var currencies = await response.Content.ReadFromJsonAsync<List<CurrencyDto>>();

        Assert.NotNull(currencies);
        Assert.Equal(TestData.CurrenciesList.Count, currencies.Count);

        foreach (var expectedCurrency in TestData.CurrenciesList)
        {
            Assert.Contains(
                currencies,
                currency =>
                    currency.CurrencyId == expectedCurrency.CurrencyId &&
                    currency.Code == expectedCurrency.Code &&
                    currency.Name == expectedCurrency.Name &&
                    currency.ExchangeRateToBase == expectedCurrency.ExchangeRateToBase &&
                    currency.IsActive == expectedCurrency.IsActive);
        }
    }

    [Fact]
    public async Task GetCurrencyById_ExistingCurrency_ReturnsOk()
    {
        // Arrange
        var currencyToSeed = TestData.CurrenciesList[0];
        await SeedAsync(currencyToSeed);

        // Act
        var response = await HttpClient.GetAsync($"/api/admin/currencies/{currencyToSeed.CurrencyId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var currency = await response.Content.ReadFromJsonAsync<CurrencyDto>();

        Assert.NotNull(currency);
        Assert.Equal(currencyToSeed.CurrencyId, currency.CurrencyId);
        Assert.Equal(currencyToSeed.Code, currency.Code);
        Assert.Equal(currencyToSeed.Name, currency.Name);
        Assert.Equal(currencyToSeed.ExchangeRateToBase, currency.ExchangeRateToBase);
        Assert.Equal(currencyToSeed.IsActive, currency.IsActive);
    }

    [Fact]
    public async Task GetCurrencyById_NonExistingCurrency_ReturnsNotFound()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/admin/currencies/{TestData.NonExistingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
    }

    [Fact]
    public async Task GetCurrencyById_EmptyCurrencyId_ReturnsBadRequest()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/admin/currencies/{Guid.Empty}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Update Currency Tests

    [Fact]
    public async Task UpdateCurrency_ValidRequest_ReturnsOkAndPersistsChanges()
    {
        // Arrange
        var currencyToUpdate = TestData.CurrencyForUpdate;
        await SeedAsync(currencyToUpdate);

        var request = new UpdateCurrencyDto(currencyToUpdate.Code, $"{currencyToUpdate.Name} Updated", currencyToUpdate.ExchangeRateToBase + 0.10m, false);

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/admin/currencies/{currencyToUpdate.CurrencyId}", request);

        // Assert - HTTP
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedCurrency = await response.Content.ReadFromJsonAsync<CurrencyDto>();

        Assert.NotNull(updatedCurrency);
        Assert.Equal(currencyToUpdate.CurrencyId, updatedCurrency.CurrencyId);
        Assert.Equal(request.Code, updatedCurrency.Code);
        Assert.Equal(request.Name, updatedCurrency.Name);
        Assert.Equal(request.ExchangeRateToBase, updatedCurrency.ExchangeRateToBase);
        Assert.Equal(request.IsActive, updatedCurrency.IsActive);

        // Assert - persistence
        var persistedCurrency = await DbContext.Currencies.AsNoTracking().FirstAsync(x => x.CurrencyId == currencyToUpdate.CurrencyId);

        Assert.Equal(request.Code, persistedCurrency.Code);
        Assert.Equal(request.Name, persistedCurrency.Name);
        Assert.Equal(request.ExchangeRateToBase, persistedCurrency.ExchangeRateToBase);
        Assert.Equal(request.IsActive, persistedCurrency.IsActive);
        Assert.NotNull(persistedCurrency.ModifiedAt);
    }

    [Fact]
    public async Task UpdateCurrency_NonExistingCurrency_ReturnsNotFound()
    {
        // Arrange
        var currency = TestData.CurrencyForUpdate;
        var request = new UpdateCurrencyDto(currency.Code, currency.Name, currency.ExchangeRateToBase, currency.IsActive);

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/admin/currencies/{TestData.NonExistingId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCurrency_EmptyCurrencyId_ReturnsBadRequest()
    {
        // Arrange
        var currency = TestData.CurrencyForUpdate;
        var request = new UpdateCurrencyDto(currency.Code, currency.Name, currency.ExchangeRateToBase, currency.IsActive);

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/admin/currencies/{Guid.Empty}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateCurrency_DuplicateCode_ReturnsConflict()
    {
        // Arrange
        await SeedAsync(TestData.CurrenciesList);

        var currencyToUpdate = TestData.CurrenciesList[0];
        var currencyWithDuplicateCode = TestData.CurrenciesList[1];

        var request = new UpdateCurrencyDto(currencyWithDuplicateCode.Code, currencyToUpdate.Name, currencyToUpdate.ExchangeRateToBase, currencyToUpdate.IsActive);

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/admin/currencies/{currencyToUpdate.CurrencyId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status409Conflict, problem.Status);
        Assert.Equal(CurrencyErrors.DuplicateCode.Code, problem.Extensions["code"]?.ToString());
    }

    #endregion
}
