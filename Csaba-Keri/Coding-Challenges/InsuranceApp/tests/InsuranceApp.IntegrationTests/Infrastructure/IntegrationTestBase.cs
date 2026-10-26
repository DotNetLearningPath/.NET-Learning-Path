using InsuranceApp.Infrastructure.Persistence;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace InsuranceApp.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase(IntegrationTestFixture fixture) : IAsyncLifetime
{
    private readonly IntegrationTestFixture _fixture = fixture;

    private protected HttpClient HttpClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _fixture.ResetDatabaseAsync();
        HttpClient = _fixture.CreateHttpClient();
    }

    public Task DisposeAsync()
    {
        HttpClient?.Dispose();

        return Task.CompletedTask;
    }

    private protected async Task<T> QueryDatabaseAsync<T>(Func<InsuranceDbContext, Task<T>> query)
    {
        using var scope = _fixture.Services.CreateScope();

        var insuranceDbContext = scope.ServiceProvider
            .GetRequiredService<InsuranceDbContext>();

        return await query(insuranceDbContext);
    }

    private protected async Task<JsonObject> CreateClientAsync(JsonObject? request = null)
    {
        using var response = await HttpClient.PostAsJsonAsync(
            requestUri: "/api/brokers/clients",
            value: request ?? TestData.Client()
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return await ReadJsonAsync(response);
    }

    private protected async Task<JsonObject> CreateBuildingAsync(Guid clientId, JsonObject? request = null)
    {
        using var response = await HttpClient.PostAsJsonAsync(
            requestUri: $"/api/brokers/clients/{clientId}/buildings",
            value: request ?? TestData.Building()
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return await ReadJsonAsync(response);
    }

    private protected async Task<JsonObject> CreateBrokerAsync(JsonObject? request = null)
    {
        using var response = await HttpClient.PostAsJsonAsync(
            requestUri: "/api/admin/brokers",
            value: request ?? TestData.Broker()
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return await ReadJsonAsync(response);
    }

    private protected async Task<JsonObject> CreateCurrencyAsync(JsonObject? request = null)
    {
        using var response = await HttpClient.PostAsJsonAsync(
            requestUri: "/api/admin/currencies",
            value: request ?? TestData.Currency()
        );
        
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        return await ReadJsonAsync(response);
    }

    private protected async Task<JsonObject> CreateFeeAsync(JsonObject? request = null)
    {
        using var response = await HttpClient.PostAsJsonAsync(
            requestUri: "/api/admin/fees",
            value: request ?? TestData.Fee()
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return await ReadJsonAsync(response);
    }

    private protected static async Task<JsonObject> ReadJsonAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<JsonObject>();
        
        Assert.NotNull(body);
        
        return body;
    }

    private protected static async Task<JsonObject> AssertProblemAsync(
        HttpResponseMessage response,
        HttpStatusCode statusCode,
        string? errorKey = null
    )
    {
        Assert.Equal(statusCode, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

        var body = await ReadJsonAsync(response);
        Assert.Equal((int)statusCode, body["status"]!.GetValue<int>());
        
        if (errorKey is not null)
        {
            Assert.NotEmpty(body["errors"]![errorKey]!.AsArray());
        }
        
        return body;
    }
}
