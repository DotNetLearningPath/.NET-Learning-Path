using InsuranceApp.Domain.Buildings;
using InsuranceApp.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace InsuranceApp.IntegrationTests;

public sealed class BuildingsEndpointsTests : IntegrationTestBase
{
    [Fact]
    public async Task CreateBuilding_ValidRequest_PersistsFieldsAndReturnsGeography()
    {
        // Arrange
        var client = await CreateClientAsync();
        var clientId = client["id"]!.GetValue<Guid>();
        var request = TestData.Building();

        // Act
        using var response = await HttpClient.PostAsJsonAsync(
            requestUri: $"/api/brokers/clients/{clientId}/buildings",
            value: request
        );

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await ReadJsonAsync(response);
        var buildingId = body["id"]!.GetValue<Guid>();

        Assert.NotEqual(Guid.Empty, buildingId);
        Assert.NotNull(response.Headers.Location);
        Assert.EndsWith($"/api/brokers/buildings/{buildingId}", response.Headers.Location.ToString());
        Assert.Equal(clientId, body["clientId"]!.GetValue<Guid>());
        
        foreach (var field in request)
        {
            Assert.True(JsonNode.DeepEquals(field.Value, body[field.Key]), field.Key);
        }

        Assert.Equal("Cluj-Napoca", body["geography"]!["city"]!["name"]!.GetValue<string>());
        Assert.Equal("Cluj", body["geography"]!["county"]!["name"]!.GetValue<string>());
        Assert.Equal("Romania", body["geography"]!["country"]!["name"]!.GetValue<string>());

        var saved = await QueryDatabaseAsync(db => db.Buildings.AsNoTracking().SingleAsync());
        Assert.Equal(buildingId, saved.Id);
        Assert.Equal(clientId, saved.ClientId);
        Assert.Equal(BuildingType.Residential, saved.Type);
        Assert.Equal(Guid.Parse(TestData.ClujNapocaId), saved.CityId);
        Assert.Equal("Main Street", saved.Street);
        Assert.Equal("12A", saved.Number);
        Assert.Equal(2005, saved.ConstructionYear);
        Assert.Equal(2, saved.NumberOfFloors);
        Assert.Equal(125.50m, saved.SurfaceArea);
        Assert.Equal(250000m, saved.InsuredValue);

        using var getResponse = await HttpClient.GetAsync(response.Headers.Location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.True(JsonNode.DeepEquals(body, await ReadJsonAsync(getResponse)));
    }

    [Theory]
    [InlineData("unknown-city", "Address.CityId")]
    [InlineData("future-year", "ConstructionYear")]
    [InlineData("negative-area", "SurfaceArea")]
    public async Task CreateBuilding_InvalidRequest_ReturnsBadRequestWithoutSaving(string scenario, string errorKey)
    {
        // Arrange
        var client = await CreateClientAsync();
        var clientId = client["id"]!.GetValue<Guid>();
        var request = TestData.Building();

        switch (scenario)
        {
            case "unknown-city":
                request["address"]!["cityId"] = Guid.NewGuid();
                break;
            
            case "future-year":
                request["constructionYear"] = DateTime.UtcNow.Year + 1;
                break;
            
            case "negative-area":
                request["surfaceArea"] = -1m;
                break;
        }

        // Act
        using var response = await HttpClient.PostAsJsonAsync(
            requestUri: $"/api/brokers/clients/{clientId}/buildings",
            value: request
        );

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.BadRequest, errorKey);
        Assert.Equal(0, await QueryDatabaseAsync(db => db.Buildings.CountAsync()));
    }

    [Fact]
    public async Task CreateBuilding_UnknownClient_ReturnsNotFoundWithoutSaving()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var request = TestData.Building();

        // Act
        using var response = await HttpClient.PostAsJsonAsync(
            requestUri: $"/api/brokers/clients/{clientId}/buildings",
            value: request
        );

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound);
        Assert.Equal(0, await QueryDatabaseAsync(db => db.Buildings.CountAsync()));
    }

    [Fact]
    public async Task UpdateBuilding_ValidRequest_PersistsChangesAndPreservesOwner()
    {
        // Arrange
        var client = await CreateClientAsync();
        var clientId = client["id"]!.GetValue<Guid>();
        var anotherClient = await CreateClientAsync();

        var originalBuilding = await CreateBuildingAsync(clientId);
        var originalBuildingId = originalBuilding["id"]!.GetValue<Guid>();

        var request = TestData.Building(
            type: "Office",
            cityId: TestData.DebrecenId,
            street: "Updated Street",
            number: "99B",
            constructionYear: 2010,
            numberOfFloors: 3,
            surfaceArea: 222.25m,
            insuredValue: 310000.75m
        );

        request["clientId"] = anotherClient["id"]!.GetValue<Guid>();

        // Act
        using var response = await HttpClient.PutAsJsonAsync(
            requestUri: $"/api/brokers/buildings/{originalBuildingId}",
            value: request
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await ReadJsonAsync(response);
        Assert.Equal(clientId, body["clientId"]!.GetValue<Guid>());
        Assert.Equal("Debrecen", body["geography"]!["city"]!["name"]!.GetValue<string>());
        Assert.Equal("Hajdú-Bihar", body["geography"]!["county"]!["name"]!.GetValue<string>());
        Assert.Equal("Hungary", body["geography"]!["country"]!["name"]!.GetValue<string>());

        var saved = await QueryDatabaseAsync(db => db.Buildings.AsNoTracking().SingleAsync());
        Assert.Equal(clientId, saved.ClientId);
        Assert.Equal(BuildingType.Office, saved.Type);
        Assert.Equal(Guid.Parse(TestData.DebrecenId), saved.CityId);
        Assert.Equal("Updated Street", saved.Street);
        Assert.Equal("99B", saved.Number);
        Assert.Equal(2010, saved.ConstructionYear);
        Assert.Equal(3, saved.NumberOfFloors);
        Assert.Equal(222.25m, saved.SurfaceArea);
        Assert.Equal(310000.75m, saved.InsuredValue);

        using var getResponse = await HttpClient.GetAsync($"/api/brokers/buildings/{originalBuildingId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.True(JsonNode.DeepEquals(body, await ReadJsonAsync(getResponse)));
    }

    [Fact]
    public async Task UpdateBuilding_UnknownCity_ReturnsBadRequestAndLeavesDataUnchanged()
    {
        // Arrange
        var client = await CreateClientAsync();

        var originalBuilding = await CreateBuildingAsync(client["id"]!.GetValue<Guid>());
        var originalBuildingId = originalBuilding["id"]!.GetValue<Guid>();

        var request = TestData.Building(
            type: "Industrial",
            cityId: Guid.NewGuid().ToString()
        );

        // Act
        using var response = await HttpClient.PutAsJsonAsync(
            requestUri: $"/api/brokers/buildings/{originalBuildingId}",
            value: request
        );

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.BadRequest, "Address.CityId");
        
        using var getResponse = await HttpClient.GetAsync($"/api/brokers/buildings/{originalBuildingId}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.True(JsonNode.DeepEquals(originalBuilding, await ReadJsonAsync(getResponse)));
    }

    [Fact]
    public async Task GetBuildingsByClientId_ReturnsOnlyThatClientsBuildings()
    {
        // Arrange
        var firstClient = await CreateClientAsync();
        var firstClientId = firstClient["id"]!.GetValue<Guid>();

        var secondClient = await CreateClientAsync();
        var secondClientId = secondClient["id"]!.GetValue<Guid>();
        
        var expectedBuilding = await CreateBuildingAsync(firstClientId);
        await CreateBuildingAsync(secondClientId);

        // Act
        using var response = await HttpClient.GetAsync($"/api/brokers/clients/{firstClientId}/buildings?pageSize=1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await ReadJsonAsync(response);
        Assert.Equal(1, page["totalCount"]!.GetValue<int>());
        Assert.Equal(1, page["pageSize"]!.GetValue<int>());

        var item = Assert.Single(page["items"]!.AsArray());
        Assert.Equal(expectedBuilding["id"]!.GetValue<Guid>(), item!["id"]!.GetValue<Guid>());
        Assert.Equal(firstClientId, item["clientId"]!.GetValue<Guid>());
        Assert.Equal("Residential", item["type"]!.GetValue<string>());
        Assert.True(JsonNode.DeepEquals(expectedBuilding["address"], item["address"]));
    }
}
