using System.Net;
using System.Net.Http.Json;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Client;
using InsuranceApp.IntegrationTests.Common;
using InsuranceApp.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.IntegrationTests.Api.Controllers.Broker;

public sealed class ClientEndpointsTests(InsuranceAppWebApplicationFactory factory)
    : IntegrationTestBase(factory), IClassFixture<InsuranceAppWebApplicationFactory>
{
    #region Create Client Tests

    [Fact]
    public async Task CreateClient_ValidRequest_ReturnsCreatedAndPersistsClient()
    {
        // Arrange
        var request = TestData.ClientDtoForCreate;

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/brokers/clients", request);

        // Assert - HTTP
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdClient = await response.Content.ReadFromJsonAsync<ClientDto>();

        Assert.NotNull(createdClient);
        Assert.NotEqual(Guid.Empty, createdClient.ClientId);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal($"/api/brokers/clients/{createdClient.ClientId}", response.Headers.Location.AbsolutePath);
        Assert.Equal(request.ClientType, createdClient.ClientType);
        Assert.Equal(request.Name, createdClient.Name);
        Assert.Equal(request.IdentificationNumber, createdClient.IdentificationNumber);
        Assert.Equal(request.Email, createdClient.Email);
        Assert.Equal(request.Phone, createdClient.Phone);
        Assert.Equal(request.Address, createdClient.Address);

        // Assert - persistence
        var persistedClient = await DbContext.Clients.AsNoTracking().FirstOrDefaultAsync(x => x.ClientId == createdClient.ClientId);

        Assert.NotNull(persistedClient);
        Assert.Equal(request.ClientType, persistedClient.ClientType);
        Assert.Equal(request.Name, persistedClient.Name);
        Assert.Equal(request.IdentificationNumber, persistedClient.IdentificationNumber);
        Assert.Equal(request.Email, persistedClient.Email);
        Assert.Equal(request.Phone, persistedClient.Phone);
        Assert.Equal(request.Address, persistedClient.Address);
    }

    [Fact]
    public async Task CreateClient_MissingName_ReturnsBadRequest()
    {
        // Arrange
        var request = TestData.ClientDtoForCreate with
        {
            Name = ""
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/brokers/clients", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
    }

    [Fact]
    public async Task CreateClient_InvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var request = TestData.ClientDtoForCreate with
        {
            Email = "invalid-email"
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/brokers/clients", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateClient_DuplicateIdentificationNumber_ReturnsConflict()
    {
        // Arrange
        var request = TestData.ClientDtoForCreate;

        var firstResponse = await HttpClient.PostAsJsonAsync("/api/brokers/clients", request);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act
        var secondResponse = await HttpClient.PostAsJsonAsync("/api/brokers/clients", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

        var count = await DbContext.Clients.CountAsync(x => x.IdentificationNumber == request.IdentificationNumber);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task CreateClient_ConcurrentDuplicateIdentificationNumber_OnlyOneIsCreated()
    {
        // Arrange
        var request = TestData.ClientDtoForCreate;

        // Act
        var task1 = HttpClient.PostAsJsonAsync("/api/brokers/clients", request);
        var task2 = HttpClient.PostAsJsonAsync("/api/brokers/clients", request);

        var responses = await Task.WhenAll(task1, task2);

        // Assert
        Assert.Contains(responses, x => x.StatusCode == HttpStatusCode.Created);
        Assert.Contains(responses, x => x.StatusCode == HttpStatusCode.Conflict);

        var count = await DbContext.Clients.CountAsync(x => x.IdentificationNumber == request.IdentificationNumber);

        Assert.Equal(1, count);
    }

    #endregion

    #region Read Client Tests

    [Fact]
    public async Task GetClientById_ExistingClient_ReturnsOk()
    {
        // Arrange
        await SeedAsync(TestData.ClientForRead);

        // Act
        var response = await HttpClient.GetAsync($"/api/brokers/clients/{TestData.ClientForRead.ClientId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var client = await response.Content.ReadFromJsonAsync<ClientDto>();

        Assert.NotNull(client);
        Assert.Equal(TestData.ClientForRead.ClientId, client.ClientId);
        Assert.Equal(TestData.ClientForRead.ClientType, client.ClientType);
        Assert.Equal(TestData.ClientForRead.Name, client.Name);
        Assert.Equal(TestData.ClientForRead.IdentificationNumber, client.IdentificationNumber);
        Assert.Equal(TestData.ClientForRead.Email, client.Email);
        Assert.Equal(TestData.ClientForRead.Phone, client.Phone);
        Assert.Equal(TestData.ClientForRead.Address, client.Address);
    }

    [Fact]
    public async Task GetClientById_NonExistingClient_ReturnsNotFound()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/brokers/clients/{TestData.NonExistingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
    }

    [Fact]
    public async Task GetClientById_EmptyClientId_ReturnsBadRequest()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/brokers/clients/{Guid.Empty}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Search Clients Tests

    [Fact]
    public async Task SearchClients_ByPartialName_ReturnsMatchingClients()
    {
        // Arrange
        await SeedAsync(TestData.ClientsForSearch);

        var expectedClients = TestData.ClientsForSearch.Where(x => x.Name.Contains("John", StringComparison.OrdinalIgnoreCase)).ToList();
        var searchName = "John";

        // Act
        var response = await HttpClient.GetAsync($"/api/brokers/clients?name={searchName}&pageNumber=1&pageSize=50");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ClientDto>>();

        Assert.NotNull(result);
        Assert.Equal(expectedClients.Count, result.Items.Count);
        Assert.Equal(expectedClients.Count, result.TotalCount);
        Assert.All(result.Items, x => Assert.Contains(searchName, x.Name, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task SearchClients_ByExactIdentifier_ReturnsMatchingClient()
    {
        // Arrange
        await SeedAsync(TestData.ClientsForSearch);

        var expectedClient = TestData.ClientsForSearch[0];

        // Act
        var response = await HttpClient.GetAsync($"/api/brokers/clients?identifier={expectedClient.IdentificationNumber}&pageNumber=1&pageSize=50");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ClientDto>>();

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Equal(expectedClient.IdentificationNumber, result.Items[0].IdentificationNumber);
    }

    [Fact]
    public async Task SearchClients_Pagination_ReturnsRequestedPage()
    {
        // Arrange
        await SeedAsync(TestData.ClientsForSearch);

        const int pageNumber = 1;
        const int pageSize = 2;

        // Act
        var response = await HttpClient.GetAsync($"/api/brokers/clients?pageNumber={pageNumber}&pageSize={pageSize}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<ClientDto>>();

        Assert.NotNull(result);
        Assert.Equal(pageSize, result.Items.Count);
        Assert.Equal(TestData.ClientsForSearch.Count, result.TotalCount);
        Assert.Equal(pageNumber, result.PageNumber);
        Assert.Equal(pageSize, result.PageSize);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SearchClients_InvalidPageNumber_ReturnsBadRequest(int pageNumber)
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/brokers/clients?pageNumber={pageNumber}&pageSize=50");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task SearchClients_InvalidPageSize_ReturnsBadRequest(int pageSize)
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/brokers/clients?pageNumber=1&pageSize={pageSize}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Update Client Tests

    [Fact]
    public async Task UpdateClient_ValidRequest_ReturnsOkAndPersistsChanges()
    {
        // Arrange
        await SeedAsync(TestData.ClientForRead);

        var request = TestData.ClientDtoForUpdate;

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/brokers/clients/{TestData.ClientForRead.ClientId}", request);

        // Assert - HTTP
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedClient = await response.Content.ReadFromJsonAsync<ClientDto>();

        Assert.NotNull(updatedClient);
        Assert.Equal(TestData.ClientForRead.ClientId, updatedClient.ClientId);
        Assert.Equal(request.Name, updatedClient.Name);
        Assert.Equal(request.Email, updatedClient.Email);
        Assert.Equal(request.Phone, updatedClient.Phone);
        Assert.Equal(request.Address, updatedClient.Address);
        Assert.Equal(TestData.ClientForRead.IdentificationNumber, updatedClient.IdentificationNumber);

        // Assert - persistence
        var persistedClient = await DbContext.Clients.AsNoTracking().FirstAsync(x => x.ClientId == TestData.ClientForRead.ClientId);

        Assert.Equal(request.Name, persistedClient.Name);
        Assert.Equal(request.Email, persistedClient.Email);
        Assert.Equal(request.Phone, persistedClient.Phone);
        Assert.Equal(request.Address, persistedClient.Address);
        Assert.Equal(TestData.ClientForRead.IdentificationNumber, persistedClient.IdentificationNumber);
        Assert.NotNull(persistedClient.ModifiedAt);
    }

    [Fact]
    public async Task UpdateClient_NonExistingClient_ReturnsNotFound()
    {
        // Arrange
        var request = TestData.ClientDtoForUpdate;

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/brokers/clients/{TestData.NonExistingId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateClient_EmptyClientId_ReturnsBadRequest()
    {
        // Arrange
        var request = TestData.ClientDtoForUpdate;

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/brokers/clients/{Guid.Empty}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateClient_MissingName_ReturnsBadRequest()
    {
        // Arrange
        await SeedAsync(TestData.ClientForRead);

        var request = TestData.ClientDtoForUpdate with
        {
            Name = ""
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/brokers/clients/{TestData.ClientForRead.ClientId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateClient_InvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        await SeedAsync(TestData.ClientForRead);

        var request = TestData.ClientDtoForUpdate with
        {
            Email = "invalid-email"
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/brokers/clients/{TestData.ClientForRead.ClientId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion
}
