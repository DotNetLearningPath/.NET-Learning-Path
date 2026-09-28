using InsuranceApp.Domain.Clients;
using InsuranceApp.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace InsuranceApp.IntegrationTests;

public sealed class ClientsEndpointsTests : IntegrationTestBase
{
    [Theory]
    [InlineData("Individual", ClientType.Individual)]
    [InlineData("Company", ClientType.Company)]
    public async Task CreateClient_ValidRequest_PersistsFieldsAndReturnsLocation(string type, ClientType expectedType)
    {
        // Arrange
        var request = TestData.Client(
            type: type,
            identificationNumber: "CLIENT-001",
            name: "Alice Example"
        );

        // Act
        using var response = await HttpClient.PostAsJsonAsync(
            requestUri: "/api/brokers/clients",
            value: request
        );

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var body = await ReadJsonAsync(response);
        var clientId = body["id"]!.GetValue<Guid>();

        Assert.NotEqual(Guid.Empty, clientId);
        Assert.NotNull(response.Headers.Location);
        Assert.EndsWith($"/api/brokers/clients/{clientId}", response.Headers.Location.ToString());
        
        foreach (var field in request)
        {
            Assert.True(JsonNode.DeepEquals(field.Value, body[field.Key]), field.Key);
        }
        
        var saved = await QueryDatabaseAsync(db => db.Clients.AsNoTracking().SingleAsync());
        Assert.Equal(clientId, saved.Id);
        Assert.Equal(expectedType, saved.Type);
        Assert.Equal("CLIENT-001", saved.IdentificationNumber);
        Assert.Equal("Alice Example", saved.Name);
        Assert.Equal("test@example.com", saved.Email);
        Assert.Equal("+40 700 123 456", saved.Phone);
        Assert.Equal("Main Street 10", saved.PrimaryAddress);

        using var getResponse = await HttpClient.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.True(JsonNode.DeepEquals(body, await ReadJsonAsync(getResponse)));
    }

    [Fact]
    public async Task CreateClient_InvalidEmailThenCorrectedRequest_PersistsOnlyCorrectEmail()
    {
        // Arrange
        var request = TestData.Client(
            identificationNumber: "EMAIL-001",
            email: "invalid-email"
        );

        // Act
        using var invalidResponse = await HttpClient.PostAsJsonAsync(
            requestUri: "/api/brokers/clients",
            value: request
        );

        var countAfterInvalidRequest = await QueryDatabaseAsync(db => db.Clients.CountAsync());

        request["email"] = "example@gmail.com";
        
        using var validResponse = await HttpClient.PostAsJsonAsync(
            requestUri: "/api/brokers/clients",
            value: request
        );

        // Assert
        await AssertProblemAsync(invalidResponse, HttpStatusCode.BadRequest, "Email");
        
        Assert.Equal(0, countAfterInvalidRequest);
        Assert.Equal(HttpStatusCode.Created, validResponse.StatusCode);

        var body = await ReadJsonAsync(validResponse);
        Assert.Equal("example@gmail.com", body["email"]!.GetValue<string>());
        
        var saved = await QueryDatabaseAsync(db => db.Clients.AsNoTracking().SingleAsync());
        Assert.Equal("example@gmail.com", saved.Email);
        
        using var getResponse = await HttpClient.GetAsync("/api/brokers/clients");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        
        var page = await ReadJsonAsync(getResponse);
        var item = Assert.Single(page["items"]!.AsArray());
        
        Assert.Equal("example@gmail.com", item!["email"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"type\":0,\"identificationNumber\":\"NUMERIC\",\"name\":\"Test\",\"email\":\"test@example.com\",\"phone\":\"123\"}")]
    public async Task CreateClient_InvalidJsonContract_ReturnsBadRequestWithoutSaving(string json)
    {
        // Arrange
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

        // Act
        using var response = await HttpClient.PostAsync(
            requestUri: "/api/brokers/clients",
            content: content
        );

        // Assert
        var problem = await AssertProblemAsync(response, HttpStatusCode.BadRequest);

        Assert.NotEmpty(problem["errors"]!.AsObject());
        Assert.Equal(0, await QueryDatabaseAsync(db => db.Clients.CountAsync()));
    }

    [Fact]
    public async Task UpdateClient_ValidRequest_PersistsChangesAndPreservesIdentity()
    {
        // Arrange
        var originalClient = await CreateClientAsync(TestData.Client(
            identificationNumber: "IMMUTABLE-001")
        );
        
        var clientId = originalClient["id"]!.GetValue<Guid>();

        var request = TestData.Client(
            type: "Company",
            identificationNumber: "SHOULD-NOT-CHANGE",
            name: "Updated Client",
            email: "updated@example.com",
            phone: "+36 30 123 4567",
            primaryAddress: null
        );

        // Act
        using var response = await HttpClient.PutAsJsonAsync(
            requestUri: $"/api/brokers/clients/{clientId}",
            value: request
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var body = await ReadJsonAsync(response);
        Assert.Equal(clientId, body["id"]!.GetValue<Guid>());
        Assert.Equal("IMMUTABLE-001", body["identificationNumber"]!.GetValue<string>());
        Assert.Equal("Individual", body["type"]!.GetValue<string>());
        Assert.Equal("updated@example.com", body["email"]!.GetValue<string>());
        
        var saved = await QueryDatabaseAsync(db => db.Clients.AsNoTracking().SingleAsync());
        Assert.Equal("Updated Client", saved.Name);
        Assert.Equal("updated@example.com", saved.Email);
        Assert.Equal("+36 30 123 4567", saved.Phone);
        Assert.Null(saved.PrimaryAddress);
        Assert.Equal("IMMUTABLE-001", saved.IdentificationNumber);
        Assert.Equal(ClientType.Individual, saved.Type);
        
        using var getResponse = await HttpClient.GetAsync($"/api/brokers/clients/{clientId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.True(JsonNode.DeepEquals(body, await ReadJsonAsync(getResponse)));
    }

    [Fact]
    public async Task UpdateClient_InvalidEmail_ReturnsBadRequestAndLeavesDataUnchanged()
    {
        // Arrange
        var originalClient = await CreateClientAsync();
        var clientId = originalClient["id"]!.GetValue<Guid>();

        var request = TestData.Client(
            name: "Changed",
            email: "invalid",
            phone: "123",
            primaryAddress: "Changed"
        );

        // Act
        using var response = await HttpClient.PutAsJsonAsync(
            requestUri: $"/api/brokers/clients/{clientId}",
            value: request
        );

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.BadRequest, "Email");
        
        using var getResponse = await HttpClient.GetAsync($"/api/brokers/clients/{clientId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.True(JsonNode.DeepEquals(originalClient, await ReadJsonAsync(getResponse)));
    }

    [Fact]
    public async Task SearchClients_PagedRequest_ReturnsOrderedPageAndTotalCount()
    {
        // Arrange
        await CreateClientAsync(TestData.Client(name: "Charlie"));
        var bravo = await CreateClientAsync(TestData.Client(name: "Bravo"));
        await CreateClientAsync(TestData.Client(name: "Alpha"));

        // Act
        using var response = await HttpClient.GetAsync("/api/brokers/clients?pageNumber=2&pageSize=1");
        using var beyondLastPage = await HttpClient.GetAsync("/api/brokers/clients?pageNumber=2147483647&pageSize=100");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var page = await ReadJsonAsync(response);
        Assert.Equal(2, page["pageNumber"]!.GetValue<int>());
        Assert.Equal(1, page["pageSize"]!.GetValue<int>());
        Assert.Equal(3, page["totalCount"]!.GetValue<int>());
        Assert.True(JsonNode.DeepEquals(bravo, Assert.Single(page["items"]!.AsArray())));
        
        Assert.Equal(HttpStatusCode.OK, beyondLastPage.StatusCode);
        
        var emptyPage = await ReadJsonAsync(beyondLastPage);
        Assert.Empty(emptyPage["items"]!.AsArray());
        Assert.Equal(3, emptyPage["totalCount"]!.GetValue<int>());
    }

    [Fact]
    public async Task SearchClients_IdentifierFilter_UsesExactMatch()
    {
        // Arrange
        var expectedClient = await CreateClientAsync(TestData.Client(
            identificationNumber: "ABC-123")
        );
        
        await CreateClientAsync(TestData.Client(
            identificationNumber: "ABC-1234")
        );

        // Act
        using var response = await HttpClient.GetAsync("/api/brokers/clients?identifier=ABC-123");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var page = await ReadJsonAsync(response);
        Assert.Equal(1, page["totalCount"]!.GetValue<int>());
        Assert.True(JsonNode.DeepEquals(expectedClient, Assert.Single(page["items"]!.AsArray())));
    }
}
