using InsuranceApp.Domain.Brokers;
using InsuranceApp.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace InsuranceApp.IntegrationTests;

public sealed class BrokersEndpointsTests : IntegrationTestBase
{
    [Theory]
    [InlineData("Active", BrokerStatus.Active)]
    [InlineData("Inactive", BrokerStatus.Inactive)]
    public async Task CreateBroker_ValidRequest_PersistsFieldsAndReturnsLocation(string status, BrokerStatus expectedStatus)
    {
        // Arrange
        var request = TestData.Broker(
            code: " br-001 ",
            status: status
        );

        // Act
        using var response = await HttpClient.PostAsJsonAsync(
            requestUri: "/api/admin/brokers",
            value: request
        );

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        
        var body = await ReadJsonAsync(response);
        var brokerId = body["id"]!.GetValue<Guid>();
        
        Assert.NotEqual(Guid.Empty, brokerId);
        Assert.NotNull(response.Headers.Location);
        Assert.EndsWith($"/api/admin/brokers/{brokerId}", response.Headers.Location.ToString());
        
        Assert.Equal("BR-001", body["code"]!.GetValue<string>());
        Assert.Equal(status, body["status"]!.GetValue<string>());
        
        var saved = await QueryDatabaseAsync(db => db.Brokers.AsNoTracking().SingleAsync());
        Assert.Equal(brokerId, saved.Id);
        Assert.Equal("BR-001", saved.Code);
        Assert.Equal("Test Broker", saved.Name);
        Assert.Equal("broker@example.com", saved.Email);
        Assert.Equal("123", saved.Phone);
        Assert.Equal(expectedStatus, saved.Status);
        
        using var getResponse = await HttpClient.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.True(JsonNode.DeepEquals(body, await ReadJsonAsync(getResponse)));
    }

    [Theory]
    [InlineData("email")]
    [InlineData("status")]
    [InlineData("missing-status")]
    [InlineData("code")]
    public async Task CreateBroker_InvalidRequest_ReturnsBadRequestWithoutSaving(string scenario)
    {
        // Arrange
        var request = TestData.Broker();
        
        switch (scenario)
        {
            case "email":
                request["email"] = "invalid";
                break;

            case "status":
                request["status"] = 0;
                break;

            case "missing-status":
                request.Remove("status");
                break;

            case "code":
                request["code"] = " ";
                break;
        }

        // Act
        using var response = await HttpClient.PostAsJsonAsync(
            requestUri: "/api/admin/brokers",
            value: request
        );

        // Assert
        var problemBody = await AssertProblemAsync(response, HttpStatusCode.BadRequest);
        
        Assert.NotEmpty(problemBody["errors"]!.AsObject());
        Assert.Equal(0, await QueryDatabaseAsync(db => db.Brokers.CountAsync()));
    }

    [Fact]
    public async Task UpdateBroker_ValidRequest_PreservesCodeAndStatus()
    {
        // Arrange
        var originalBroker = await CreateBrokerAsync();
        var brokerId = originalBroker["id"]!.GetValue<Guid>();

        var request = TestData.Broker(
            code: "NEW-CODE",
            name: "Updated",
            email: "updated@example.com",
            phone: "456",
            status: "Active"
        );

        // Act
        using var response = await HttpClient.PutAsJsonAsync(
            requestUri: $"/api/admin/brokers/{brokerId}",
            value: request
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var saved = await QueryDatabaseAsync(db => db.Brokers.AsNoTracking().SingleAsync());
        Assert.Equal(brokerId, saved.Id);
        Assert.Equal("BR-001", saved.Code);
        Assert.Equal(BrokerStatus.Active, saved.Status);
        Assert.Equal("Updated", saved.Name);
        Assert.Equal("updated@example.com", saved.Email);
        Assert.Equal("456", saved.Phone);
        
        using var getResponse = await HttpClient.GetAsync($"/api/admin/brokers/{brokerId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.True(JsonNode.DeepEquals(await ReadJsonAsync(response), await ReadJsonAsync(getResponse)));
    }

    [Fact]
    public async Task UpdateBroker_InvalidEmail_ReturnsBadRequestAndLeavesDataUnchanged()
    {
        // Arrange
        var originalBroker = await CreateBrokerAsync();
        var brokerId = originalBroker["id"]!.GetValue<Guid>();

        var request = TestData.Broker(
            name: "Changed",
            email: "invalid",
            phone: "456"
        );

        // Act
        using var response = await HttpClient.PutAsJsonAsync(
            requestUri: $"/api/admin/brokers/{brokerId}",
            value: request
        );

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.BadRequest, "Email");

        using var getResponse = await HttpClient.GetAsync($"/api/admin/brokers/{brokerId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.True(JsonNode.DeepEquals(originalBroker, await ReadJsonAsync(getResponse)));
    }

    [Fact]
    public async Task ActivateBrokerThenDeactivateBroker_RepeatedRequests_PersistsStatus()
    {
        // Arrange
        var createdBroker = await CreateBrokerAsync();
        var brokerId = createdBroker["id"]!.GetValue<Guid>();

        // Act
        using var activation = await HttpClient.PostAsync(
            requestUri: $"/api/admin/brokers/{brokerId}/activate",
            content: null
        );

        using var repeatedActivation = await HttpClient.PostAsync(
            requestUri: $"/api/admin/brokers/{brokerId}/activate",
            content: null
        );

        var activated = await QueryDatabaseAsync(db => db.Brokers.AsNoTracking().SingleAsync());

        using var deactivation = await HttpClient.PostAsync(
            requestUri: $"/api/admin/brokers/{brokerId}/deactivate",
            content: null
        );

        using var repeatedDeactivation = await HttpClient.PostAsync(
            requestUri: $"/api/admin/brokers/{brokerId}/deactivate",
            content: null
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, activation.StatusCode);
        Assert.Equal(HttpStatusCode.OK, repeatedActivation.StatusCode);
        Assert.Equal(BrokerStatus.Active, activated.Status);
        Assert.Equal("Active", (await ReadJsonAsync(activation))["status"]!.GetValue<string>());
        
        Assert.Equal(HttpStatusCode.OK, deactivation.StatusCode);
        Assert.Equal(HttpStatusCode.OK, repeatedDeactivation.StatusCode);
        
        var saved = await QueryDatabaseAsync(db => db.Brokers.AsNoTracking().SingleAsync());
        Assert.Equal(BrokerStatus.Inactive, saved.Status);
        Assert.Equal("Test Broker", saved.Name);
        Assert.Equal("broker@example.com", saved.Email);
    }

    [Fact]
    public async Task GetBrokers_PagedRequest_ReturnsOrderedPageIncludingInactiveBrokers()
    {
        // Arrange
        var zeta = await CreateBrokerAsync(TestData.Broker(code: "Z", name: "Zeta"));
        await CreateBrokerAsync(TestData.Broker(code: "A", name: "Alpha"));

        // Act
        using var lastPageResponse = await HttpClient.GetAsync("/api/admin/brokers?pageNumber=2&pageSize=1");
        using var beyondLastPageResponse = await HttpClient.GetAsync("/api/admin/brokers?pageNumber=3&pageSize=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, lastPageResponse.StatusCode);
        
        var lastPage = await ReadJsonAsync(lastPageResponse);
        Assert.Equal(2, lastPage["totalCount"]!.GetValue<int>());
        Assert.Equal(2, lastPage["pageNumber"]!.GetValue<int>());
        Assert.Equal(1, lastPage["pageSize"]!.GetValue<int>());
        
        var item = Assert.Single(lastPage["items"]!.AsArray());
        Assert.Equal(zeta["name"]!.GetValue<string>(), item!["name"]!.GetValue<string>());
        Assert.Equal(zeta["status"]!.GetValue<string>(), item["status"]!.GetValue<string>());
        
        Assert.Equal(HttpStatusCode.OK, beyondLastPageResponse.StatusCode);
        
        var emptyPage = await ReadJsonAsync(beyondLastPageResponse);
        Assert.Empty(emptyPage["items"]!.AsArray());
        Assert.Equal(2, emptyPage["totalCount"]!.GetValue<int>());
    }

    [Theory]
    [InlineData("?pageSize=101")]
    [InlineData("/not-a-guid")]
    [InlineData("/00000000-0000-0000-0000-000000000000")]
    public async Task GetBrokers_InvalidInput_ReturnsBadRequest(string suffix)
    {
        // Arrange
        string url = $"/api/admin/brokers{suffix}";

        // Act
        using var response = await HttpClient.GetAsync(url);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("GET", "")]
    [InlineData("PUT", "")]
    [InlineData("POST", "/activate")]
    [InlineData("POST", "/deactivate")]
    public async Task BrokerEndpoint_UnknownId_ReturnsNotFound(string method, string suffix)
    {
        // Arrange
        using var request = new HttpRequestMessage(
            method: new HttpMethod(method),
            requestUri: $"/api/admin/brokers/{Guid.NewGuid()}{suffix}"
        );

        if (method == "PUT")
        {
            request.Content = JsonContent.Create(new
            {
                name = "Broker",
                email = "broker@example.com",
                phone = "123"
            });
        }

        // Act
        using var response = await HttpClient.SendAsync(request);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound);
    }
}
