using InsuranceApp.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace InsuranceApp.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly SqliteConnection _dbConnection = new("Data Source=:memory:;Foreign Keys=True");
    private InsuranceApiFactory? _apiFactory;

    protected HttpClient HttpClient { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _dbConnection.OpenAsync();

        // The production model uses PostgreSQL's C collation for exact identifiers.
        // This test-only collation supports the ASCII identifiers used by these tests.
        _dbConnection.CreateCollation("C", StringComparer.Ordinal.Compare);

        _apiFactory = new InsuranceApiFactory(_dbConnection);

        using (var scope = _apiFactory.Services.CreateScope())
        {
            await scope.ServiceProvider
                .GetRequiredService<InsuranceDbContext>()
                .Database
                .EnsureCreatedAsync();
        }

        HttpClient = _apiFactory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        HttpClient?.Dispose();

        try
        {
            if (_apiFactory is not null)
            {
                await _apiFactory.DisposeAsync();
            }
        }
        finally
        {
            // Closing the last connection destroys this test's database.
            await _dbConnection.DisposeAsync();
        }
    }

    protected async Task<T> QueryDatabaseAsync<T>(Func<InsuranceDbContext, Task<T>> query)
    {
        using var scope = _apiFactory!.Services.CreateScope();

        var insuranceDbContext = scope.ServiceProvider
            .GetRequiredService<InsuranceDbContext>();

        return await query(insuranceDbContext);
    }

    protected async Task<JsonObject> CreateClientAsync(JsonObject? request = null)
    {
        using var response = await HttpClient.PostAsJsonAsync(
            requestUri: "/api/brokers/clients",
            value: request ?? TestData.Client()
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return await ReadJsonAsync(response);
    }

    protected async Task<JsonObject> CreateBuildingAsync(Guid clientId, JsonObject? request = null)
    {
        using var response = await HttpClient.PostAsJsonAsync(
            requestUri: $"/api/brokers/clients/{clientId}/buildings",
            value: request ?? TestData.Building()
        );

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        return await ReadJsonAsync(response);
    }

    protected static async Task<JsonObject> ReadJsonAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadFromJsonAsync<JsonObject>();
        
        Assert.NotNull(body);
        
        return body;
    }

    protected static async Task<JsonObject> AssertProblemAsync(
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
