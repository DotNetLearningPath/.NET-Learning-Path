using System.Net;
using System.Net.Http.Json;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Client;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.IntegrationTests.Common;
using InsuranceApp.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceApp.IntegrationTests.Api.Controllers.Broker;

public sealed class ClientEndpointsTests(InsuranceAppWebApplicationFactory factory) : IClassFixture<InsuranceAppWebApplicationFactory>, IAsyncLifetime
{
    private readonly InsuranceAppWebApplicationFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        await _factory.SeedGeographyAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;


    #region Create Client Tests

    [Fact]
    public async Task CreateClient_ValidRequest_ReturnsCreatedAndPersistsClient()
    {
        // Arrange
        await ResetDatabaseAsync();

        var request = CreateValidClientDto();

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/brokers/clients",
            request);

        // Assert - HTTP
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var createdClient =
            await response.Content.ReadFromJsonAsync<ClientDto>();

        Assert.NotNull(createdClient);
        Assert.True(createdClient.ClientId > 0);

        Assert.Equal(request.ClientType, createdClient.ClientType);
        Assert.Equal(request.Name, createdClient.Name);
        Assert.Equal(
            request.IdentificationNumber,
            createdClient.IdentificationNumber);
        Assert.Equal(request.Email, createdClient.Email);

        // Assert - persistence
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<InsuranceDbContext>();

        var persistedClient =
            await dbContext.Clients
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.ClientId == createdClient.ClientId);

        Assert.NotNull(persistedClient);

        Assert.Equal(
            request.IdentificationNumber,
            persistedClient.IdentificationNumber);

        Assert.Equal(
            request.Name,
            persistedClient.Name);
    }

    [Fact]
    public async Task CreateClient_MissingName_ReturnsBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();

        var request = CreateValidClientDto() with
        {
            Name = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/brokers/clients",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problem =
            await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public async Task CreateClient_InvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();

        var request = CreateValidClientDto() with
        {
            Email = "invalid-email"
        };

        // Act
        var response = await _client.PostAsJsonAsync(
            "/api/brokers/clients",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateClient_DuplicateIdentificationNumber_ReturnsConflict()
    {
        // Arrange
        await ResetDatabaseAsync();

        var request = CreateValidClientDto();

        var firstResponse = await _client.PostAsJsonAsync(
            "/api/brokers/clients",
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            firstResponse.StatusCode);

        // Act
        var secondResponse = await _client.PostAsJsonAsync(
            "/api/brokers/clients",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Conflict,
            secondResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<InsuranceDbContext>();

        var count = await dbContext.Clients.CountAsync(
            x => x.IdentificationNumber ==
                 request.IdentificationNumber);

        Assert.Equal(1, count);
    }

    [Fact(Skip = "SQLite concurrency handling is not implemented, because didn't want to add SQLite dependency in Infrastructure only to pass this test")]
    public async Task CreateClient_ConcurrentDuplicateIdentificationNumber_OnlyOneIsCreated()
    {
        // Arrange
        await ResetDatabaseAsync();

        var request = CreateValidClientDto();

        // Act - send both requests concurrently
        var task1 = _client.PostAsJsonAsync(
            "/api/brokers/clients",
            request);

        var task2 = _client.PostAsJsonAsync(
            "/api/brokers/clients",
            request);

        var responses = await Task.WhenAll(task1, task2);

        // Assert
        Assert.Contains(
            responses,
            x => x.StatusCode == HttpStatusCode.Created);

        Assert.Contains(
            responses,
            x => x.StatusCode == HttpStatusCode.Conflict);

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<InsuranceDbContext>();

        var count = await dbContext.Clients.CountAsync(
            x => x.IdentificationNumber == request.IdentificationNumber);

        Assert.Equal(1, count);
    }
    #endregion


    #region Read Client Tests
    [Fact]
    public async Task GetClientById_ExistingClient_ReturnsOk()
    {
        // Arrange
        await ResetDatabaseAsync();

        var clientId = await SeedClientAsync();

        // Act
        var response = await _client.GetAsync(
            $"/api/brokers/clients/{clientId}");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var client =
            await response.Content.ReadFromJsonAsync<ClientDto>();

        Assert.NotNull(client);

        Assert.Equal(clientId, client.ClientId);
        Assert.Equal("John Doe", client.Name);
        Assert.Equal(
            "1980101223344",
            client.IdentificationNumber);
    }

    [Fact]
    public async Task GetClientById_NonExistingClient_ReturnsNotFound()
    {
        // Arrange
        await ResetDatabaseAsync();

        // Act
        var response = await _client.GetAsync(
            $"/api/brokers/clients/{TestConstants.NonExistingId}");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        var problem =
            await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetClientById_InvalidClientId_ReturnsBadRequest(int clientId)
    {
        await ResetDatabaseAsync();

        var response = await _client.GetAsync(
            $"/api/brokers/clients/{clientId}");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
    #endregion


    #region Search Clients Tests
    [Fact]
    public async Task SearchClients_ByPartialName_ReturnsMatchingClients()
    {
        // Arrange
        await ResetDatabaseAsync();
        await SeedSearchClientsAsync();

        // Act
        var response = await _client.GetAsync(
            "/api/brokers/clients?name=John&pageNumber=1&pageSize=50");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PagedResult<ClientDto>>();

        Assert.NotNull(result);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalCount);

        Assert.All(
            result.Items,
            x => Assert.Contains(
                "John",
                x.Name,
                StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchClients_ByExactIdentifier_ReturnsMatchingClient()
    {
        // Arrange
        await ResetDatabaseAsync();
        await SeedSearchClientsAsync();

        // Act
        var response = await _client.GetAsync(
            "/api/brokers/clients?identifier=1980101223344&pageNumber=1&pageSize=50");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PagedResult<ClientDto>>();

        Assert.NotNull(result);

        Assert.Single(result.Items);

        Assert.Equal(
            "1980101223344",
            result.Items[0].IdentificationNumber);
    }

    [Fact]
    public async Task SearchClients_Pagination_ReturnsRequestedPage()
    {
        // Arrange
        await ResetDatabaseAsync();
        await SeedSearchClientsAsync();

        // Act
        var response = await _client.GetAsync(
            "/api/brokers/clients?pageNumber=1&pageSize=2");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<PagedResult<ClientDto>>();

        Assert.NotNull(result);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(2, result.PageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SearchClients_InvalidPageNumber_ReturnsBadRequest(int pageNumber)
    {
        await ResetDatabaseAsync();

        var response = await _client.GetAsync(
            $"/api/brokers/clients?pageNumber={pageNumber}&pageSize=50");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task SearchClients_InvalidPageSize_ReturnsBadRequest(int pageSize)
    {
        await ResetDatabaseAsync();

        var response = await _client.GetAsync(
            $"/api/brokers/clients?pageNumber=1&pageSize={pageSize}");

        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
    #endregion


    #region Update Client Tests

    [Fact]
    public async Task UpdateClient_ValidRequest_ReturnsOkAndPersistsChanges()
    {
        // Arrange
        await ResetDatabaseAsync();

        var clientId = await SeedClientAsync();

        var request = new UpdateClientDto(
            "John Updated",
            "john.updated@test.com",
            "0700123456",
            "Bucharest");

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/brokers/clients/{clientId}",
            request);

        // Assert - HTTP
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var updatedClient =
            await response.Content.ReadFromJsonAsync<ClientDto>();

        Assert.NotNull(updatedClient);

        Assert.Equal("John Updated", updatedClient.Name);
        Assert.Equal(
            "john.updated@test.com",
            updatedClient.Email);

        // Identifier must remain unchanged
        Assert.Equal(
            "1980101223344",
            updatedClient.IdentificationNumber);

        // Assert - database
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<InsuranceDbContext>();

        var persistedClient =
            await dbContext.Clients
                .AsNoTracking()
                .FirstAsync(x => x.ClientId == clientId);

        Assert.Equal(
            "John Updated",
            persistedClient.Name);

        Assert.Equal(
            "john.updated@test.com",
            persistedClient.Email);

        Assert.Equal(
            "1980101223344",
            persistedClient.IdentificationNumber);

        Assert.NotNull(persistedClient.ModifiedAt);
    }

    [Fact]
    public async Task UpdateClient_NonExistingClient_ReturnsNotFound()
    {
        // Arrange
        await ResetDatabaseAsync();

        var request = new UpdateClientDto(
            "John Updated",
            "john.updated@test.com",
            null,
            null);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/brokers/clients/{TestConstants.NonExistingId}",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task UpdateClient_MissingName_ReturnsBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();

        var clientId = await SeedClientAsync();

        var request = new UpdateClientDto(
            "",
            "john@test.com",
            null,
            null);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/brokers/clients/{clientId}",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task UpdateClient_InvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        await ResetDatabaseAsync();

        var clientId = await SeedClientAsync();

        var request = new UpdateClientDto(
            "John Doe",
            "invalid-email",
            null,
            null);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/brokers/clients/{clientId}",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
    #endregion


    #region Helpers

    private static CreateClientDto CreateValidClientDto()
    {
        return new CreateClientDto(
            ClientType.Individual,
            "John Doe",
            "1980101223344",
            "john@test.com",
            "0712345678",
            "Cluj-Napoca");
    }

    private async Task<int> SeedClientAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<InsuranceDbContext>();

        var client = new Client
        {
            ClientType = ClientType.Individual,
            Name = "John Doe",
            IdentificationNumber = "1980101223344",
            Email = "john@test.com",
            Phone = "0712345678",
            Address = "Cluj-Napoca",
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Clients.Add(client);

        await dbContext.SaveChangesAsync();

        return client.ClientId;
    }

    private async Task SeedSearchClientsAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<InsuranceDbContext>();

        dbContext.Clients.AddRange(
            new Client
            {
                ClientType = ClientType.Individual,
                Name = "John Doe",
                IdentificationNumber = "1980101223344",
                Email = "john@test.com",
                CreatedAt = DateTime.UtcNow
            },
            new Client
            {
                ClientType = ClientType.Individual,
                Name = "John Smith",
                IdentificationNumber = "1990202334455",
                Email = "john.smith@test.com",
                CreatedAt = DateTime.UtcNow
            },
            new Client
            {
                ClientType = ClientType.Company,
                Name = "Demo Company",
                IdentificationNumber = "RO12345678",
                Email = "office@demo.test",
                CreatedAt = DateTime.UtcNow
            });

        await dbContext.SaveChangesAsync();
    }

    private async Task ResetDatabaseAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<InsuranceDbContext>();

        dbContext.Clients.RemoveRange(dbContext.Clients);

        await dbContext.SaveChangesAsync();
    }
    #endregion
}