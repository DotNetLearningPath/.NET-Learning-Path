using InsuranceApp.Domain.Currencies;
using InsuranceApp.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace InsuranceApp.IntegrationTests;

public sealed class CurrenciesEndpointsTests : IntegrationTestBase
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateCurrency_ValidRequest_PersistsAllFieldsAndReturnsLocation(bool isActive)
    {
        // Arrange
        var request = TestData.Currency(
            code: " eur ",
            name: "Euro",
            exchangeRateToBase: 5.12345678m,
            isActive: isActive
        );

        // Act
        using var response = await HttpClient.PostAsJsonAsync(
            requestUri: "/api/admin/currencies",
            value: request
        );

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var body = await ReadJsonAsync(response);
        var currencyId = body["id"]!.GetValue<Guid>();
        
        Assert.NotEqual(Guid.Empty, currencyId);
        Assert.NotNull(response.Headers.Location);
        Assert.EndsWith($"/api/admin/currencies/{currencyId}", response.Headers.Location.ToString());
        Assert.Equal("EUR", body["code"]!.GetValue<string>());
        Assert.Equal("Euro", body["name"]!.GetValue<string>());
        Assert.Equal(5.12345678m, body["exchangeRateToBase"]!.GetValue<decimal>());
        Assert.Equal(isActive, body["isActive"]!.GetValue<bool>());
        
        var saved = await QueryDatabaseAsync(db => db.Currencies.AsNoTracking().SingleAsync());
        Assert.Equal(currencyId, saved.Id);
        Assert.Equal("EUR", saved.Code);
        Assert.Equal("Euro", saved.Name);
        Assert.Equal(5.12345678m, saved.ExchangeRateToBase);
        Assert.Equal(isActive, saved.IsActive);
        
        using var getResponse = await HttpClient.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.True(JsonNode.DeepEquals(body, await ReadJsonAsync(getResponse)));
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1.123456789")]
    [InlineData("10000000000")]
    public async Task CreateCurrency_InvalidRate_ReturnsBadRequestWithoutSaving(string rate)
    {
        // Arrange
        var exchangeRateToBase = decimal.Parse(rate, CultureInfo.InvariantCulture);
        var request = TestData.Currency(exchangeRateToBase: exchangeRateToBase);

        // Act
        using var response = await HttpClient.PostAsJsonAsync(
            requestUri: "/api/admin/currencies",
            value: request
        );

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.BadRequest, "ExchangeRateToBase");
        
        Assert.Equal(0, await QueryDatabaseAsync(db => db.Currencies.CountAsync()));
    }

    [Theory]
    [InlineData("code")]
    [InlineData("name")]
    [InlineData("exchangeRateToBase")]
    [InlineData("isActive")]
    public async Task CreateCurrency_MissingRequiredField_ReturnsBadRequest(string requiredField)
    {
        // Arrange
        var request = TestData.Currency();
        request.Remove(requiredField);

        // Act
        using var response = await HttpClient.PostAsJsonAsync(
            requestUri: "/api/admin/currencies",
            value: request
        );

        // Assert
        var problem = await AssertProblemAsync(response, HttpStatusCode.BadRequest);
        Assert.NotEmpty(problem["errors"]!.AsObject());

        Assert.Equal(0, await QueryDatabaseAsync(db => db.Currencies.CountAsync()));
    }

    [Fact]
    public async Task CreateCurrency_BaseCurrencyWithWrongRate_ReturnsBadRequest()
    {
        // Arrange
        var request = TestData.Currency(
            code: CurrencyRules.BaseCurrencyCode,
            exchangeRateToBase: 2m
        );

        // Act
        using var response = await HttpClient.PostAsJsonAsync(
            requestUri: "/api/admin/currencies",
            value: request
        );

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.BadRequest, "ExchangeRateToBase");
        
        Assert.Equal(0, await QueryDatabaseAsync(db => db.Currencies.CountAsync()));
    }

    [Fact]
    public async Task UpdateCurrency_ValidRequest_PersistsChangesAndPreservesCode()
    {
        // Arrange
        var createdCurrency = await CreateCurrencyAsync();
        var currencyId = createdCurrency["id"]!.GetValue<Guid>();

        var request = TestData.Currency(
            code: "USD",
            name: "Updated Euro",
            exchangeRateToBase: 5.25m,
            isActive: false
        );

        // Act
        using var response = await HttpClient.PutAsJsonAsync(
            requestUri: $"/api/admin/currencies/{currencyId}",
            value: request
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var saved = await QueryDatabaseAsync(db => db.Currencies.AsNoTracking().SingleAsync());
        Assert.Equal(currencyId, saved.Id);
        Assert.Equal("EUR", saved.Code);
        Assert.Equal("Updated Euro", saved.Name);
        Assert.Equal(5.25m, saved.ExchangeRateToBase);
        Assert.False(saved.IsActive);
        
        using var getResponse = await HttpClient.GetAsync($"/api/admin/currencies/{currencyId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.True(JsonNode.DeepEquals(await ReadJsonAsync(response), await ReadJsonAsync(getResponse)));
    }

    [Fact]
    public async Task UpdateCurrency_InvalidRate_LeavesAllFieldsUnchanged()
    {
        // Arrange
        var createdCurrency = await CreateCurrencyAsync();
        var currencyId = createdCurrency["id"]!.GetValue<Guid>();

        var request = TestData.Currency(
            name: "Changed",
            exchangeRateToBase: -1m,
            isActive: false
        );

        // Act
        using var response = await HttpClient.PutAsJsonAsync(
            requestUri: $"/api/admin/currencies/{currencyId}",
            value: request
        );

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.BadRequest, "ExchangeRateToBase");
        
        using var getResponse = await HttpClient.GetAsync($"/api/admin/currencies/{currencyId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.True(JsonNode.DeepEquals(createdCurrency, await ReadJsonAsync(getResponse)));
    }

    [Fact]
    public async Task UpdateCurrency_BaseCurrencyWithWrongRate_LeavesDataUnchanged()
    {
        // Arrange
        var initialCurrency = TestData.Currency(
            code: CurrencyRules.BaseCurrencyCode,
            name: "Base Currency",
            exchangeRateToBase: 1m
        );

        var createdCurrency = await CreateCurrencyAsync(initialCurrency);
        var currencyId = createdCurrency["id"]!.GetValue<Guid>();

        var request = TestData.Currency(
            name: "Changed",
            exchangeRateToBase: 2m,
            isActive: false
        );

        // Act
        using var response = await HttpClient.PutAsJsonAsync(
            requestUri: $"/api/admin/currencies/{currencyId}",
            value: request
        );

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.BadRequest, "ExchangeRateToBase");
        
        var saved = await QueryDatabaseAsync(db => db.Currencies.AsNoTracking().SingleAsync());
        Assert.Equal(CurrencyRules.BaseCurrencyCode, saved.Code);
        Assert.Equal("Base Currency", saved.Name);
        Assert.Equal(1m, saved.ExchangeRateToBase);
        Assert.True(saved.IsActive);
    }

    [Fact]
    public async Task UpdateCurrency_InactiveCurrency_CanBeReactivated()
    {
        // Arrange
        var initialCurrency = TestData.Currency(isActive: false);
        var createdCurrency = await CreateCurrencyAsync(initialCurrency);
        var currencyId = createdCurrency["id"]!.GetValue<Guid>();

        var request = TestData.Currency(isActive: true);

        // Act
        using var response = await HttpClient.PutAsJsonAsync(
            requestUri: $"/api/admin/currencies/{currencyId}",
            value: request
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.True((await ReadJsonAsync(response))["isActive"]!.GetValue<bool>());
        Assert.True((await QueryDatabaseAsync(db => db.Currencies.AsNoTracking().SingleAsync())).IsActive);
    }

    [Fact]
    public async Task GetCurrencies_PagedRequest_ReturnsCodeOrderAndIncludesInactiveCurrencies()
    {
        // Arrange
        await CreateCurrencyAsync(TestData.Currency(code: "CCC", isActive: false));
        await CreateCurrencyAsync(TestData.Currency(code: "BBB"));
        await CreateCurrencyAsync(TestData.Currency(code: "AAA"));

        // Act
        using var lastPageResponse = await HttpClient.GetAsync("/api/admin/currencies?pageNumber=3&pageSize=1");
        using var beyondLastPageResponse = await HttpClient.GetAsync("/api/admin/currencies?pageNumber=4&pageSize=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, lastPageResponse.StatusCode);
        
        var page = await ReadJsonAsync(lastPageResponse);
        Assert.Equal(3, page["totalCount"]!.GetValue<int>());
        Assert.Equal(3, page["pageNumber"]!.GetValue<int>());
        Assert.Equal(1, page["pageSize"]!.GetValue<int>());

        var item = Assert.Single(page["items"]!.AsArray());
        Assert.Equal("CCC", item!["code"]!.GetValue<string>());
        Assert.False(item["isActive"]!.GetValue<bool>());
        
        Assert.Equal(HttpStatusCode.OK, beyondLastPageResponse.StatusCode);
        
        var emptyPage = await ReadJsonAsync(beyondLastPageResponse);
        Assert.Empty(emptyPage["items"]!.AsArray());
        Assert.Equal(3, emptyPage["totalCount"]!.GetValue<int>());
    }

    [Theory]
    [InlineData("?pageSize=101")]
    [InlineData("/not-a-guid")]
    [InlineData("/00000000-0000-0000-0000-000000000000")]
    public async Task GetCurrencies_InvalidInput_ReturnsBadRequest(string suffix)
    {
        // Arrange
        var url = $"/api/admin/currencies{suffix}";

        // Act
        using var response = await HttpClient.GetAsync(url);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    public async Task CurrencyEndpoint_UnknownId_ReturnsNotFound(string method)
    {
        // Arrange
        using var request = new HttpRequestMessage(
            method: new HttpMethod(method),
            requestUri: $"/api/admin/currencies/{Guid.NewGuid()}"
        );
        
        if (method == "PUT")
        {
            request.Content = JsonContent.Create(new
            {
                name = "Euro",
                exchangeRateToBase = 5m,
                isActive = true
            });
        }

        // Act
        using var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound);
    }
}
