using InsuranceApp.Application.DTOs.Building;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.IntegrationTests.Common;
using InsuranceApp.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace InsuranceApp.IntegrationTests.Api.Controllers.Broker;

public sealed class BuildingEndpointsTests(InsuranceAppWebApplicationFactory factory) : IClassFixture<InsuranceAppWebApplicationFactory>, IAsyncLifetime
{
    private readonly InsuranceAppWebApplicationFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        await _factory.SeedGeographyAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;


    #region Get Building By Id Tests

    [Fact]
    public async Task GetBuildingById_ExistingBuilding_ReturnsOk()
    {
        // Arrange
        var buildingId = await SeedBuildingAsync();

        // Act
        var response = await _client.GetAsync($"/api/brokers/buildings/{buildingId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var building = await response.Content.ReadFromJsonAsync<BuildingDto>();

        Assert.NotNull(building);
        Assert.Equal(buildingId, building.BuildingId);
        Assert.Equal("Memorandumului", building.AddressStreet);
        Assert.Equal("25A", building.AddressStreetNumber);
    }

    [Fact]
    public async Task GetBuildingById_NonExistingBuilding_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/brokers/buildings/{TestConstants.NonExistingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Get Buildings By Client Tests

    [Fact]
    public async Task GetBuildingsByClient_ExistingClient_ReturnsOkWithBuildings()
    {
        // Arrange
        var clientId = await SeedClientAsync();
        await SeedBuildingAsync(clientId);

        // Act
        var response = await _client.GetAsync($"/api/brokers/clients/{clientId}/buildings");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var buildings = await response.Content.ReadFromJsonAsync<List<BuildingDto>>();

        Assert.NotNull(buildings);
        Assert.NotEmpty(buildings);
        Assert.All(buildings, building => Assert.Equal(clientId, building.ClientId));
    }

    [Fact]
    public async Task GetBuildingsByClient_ClientWithoutBuildings_ReturnsEmptyList()
    {
        // Arrange
        var clientId = await SeedClientAsync();

        // Act
        var response = await _client.GetAsync($"/api/brokers/clients/{clientId}/buildings");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var buildings = await response.Content.ReadFromJsonAsync<List<BuildingDto>>();

        Assert.NotNull(buildings);
        Assert.Empty(buildings);
    }

    [Fact]
    public async Task GetBuildingsByClient_NonExistingClient_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/brokers/clients/{TestConstants.NonExistingId}/buildings");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Create Building Tests

    [Fact]
    public async Task CreateBuilding_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var clientId = await SeedClientAsync();

        var request = CreateValidBuildingDto();

        // Act
        var response = await _client.PostAsJsonAsync($"/api/brokers/clients/{clientId}/buildings", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var building = await response.Content.ReadFromJsonAsync<BuildingDto>();

        Assert.NotNull(building);
        Assert.Equal(clientId, building.ClientId);
        Assert.Equal(request.CityId, building.CityId);
        Assert.Equal(request.AddressStreet, building.AddressStreet);
        Assert.Equal(request.AddressStreetNumber, building.AddressStreetNumber);
        Assert.Equal(request.BuildingType, building.BuildingType);
        Assert.Equal(request.SurfaceArea, building.SurfaceArea);
        Assert.Equal(request.InsuredValue, building.InsuredValue);

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();

        var persistedBuilding = await dbContext.Buildings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.BuildingId == building.BuildingId);

        Assert.NotNull(persistedBuilding);
        Assert.Equal(clientId, persistedBuilding.ClientId);
    }

    [Fact]
    public async Task CreateBuilding_NonExistingClient_ReturnsNotFound()
    {
        // Arrange
        var request = CreateValidBuildingDto();

        // Act
        var response = await _client.PostAsJsonAsync($"/api/brokers/clients/{TestConstants.NonExistingId}/buildings", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateBuilding_NonExistingCity_ReturnsNotFound()
    {
        // Arrange
        var clientId = await SeedClientAsync();

        var request = CreateValidBuildingDto() with
        {
            CityId = TestConstants.NonExistingId
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/brokers/clients/{clientId}/buildings", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateBuilding_InvalidSurfaceArea_ReturnsBadRequest()
    {
        // Arrange
        var clientId = await SeedClientAsync();

        var request = CreateValidBuildingDto() with
        {
            SurfaceArea = 0
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/brokers/clients/{clientId}/buildings", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateBuilding_SurfaceAreaWithTooManyDecimals_ReturnsBadRequest()
    {
        // Arrange
        var clientId = await SeedClientAsync();

        var request = CreateValidBuildingDto() with
        {
            SurfaceArea = 123.456m
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/brokers/clients/{clientId}/buildings", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateBuilding_InsuredValueWithTooManyDecimals_ReturnsBadRequest()
    {
        // Arrange
        var clientId = await SeedClientAsync();

        var request = CreateValidBuildingDto() with
        {
            InsuredValue = 1000.999m
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/brokers/clients/{clientId}/buildings", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Update Building Tests

    [Fact]
    public async Task UpdateBuilding_ValidRequest_ReturnsOk()
    {
        // Arrange
        var buildingId = await SeedBuildingAsync();

        var request = new UpdateBuildingDto(
            BuildingType.Office,
            "Republicii",
            "10B",
            1,
            2020,
            6,
            350.50m,
            1_250_000.00m,
            "Earthquake risk zone");

        // Act
        var response = await _client.PutAsJsonAsync($"/api/brokers/buildings/{buildingId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var building = await response.Content.ReadFromJsonAsync<BuildingDto>();

        Assert.NotNull(building);
        Assert.Equal(buildingId, building.BuildingId);
        Assert.Equal("Republicii", building.AddressStreet);
        Assert.Equal("10B", building.AddressStreetNumber);
        Assert.Equal(BuildingType.Office, building.BuildingType);
        Assert.Equal(350.50m, building.SurfaceArea);
        Assert.Equal(1_250_000.00m, building.InsuredValue);

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();

        var persistedBuilding = await dbContext.Buildings
            .AsNoTracking()
            .FirstAsync(x => x.BuildingId == buildingId);

        Assert.Equal("Republicii", persistedBuilding.AddressStreet);
        Assert.NotNull(persistedBuilding.ModifiedAt);
    }

    [Fact]
    public async Task UpdateBuilding_NonExistingBuilding_ReturnsNotFound()
    {
        // Arrange
        var request = CreateValidUpdateBuildingDto();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/brokers/buildings/{TestConstants.NonExistingId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBuilding_NonExistingCity_ReturnsNotFound()
    {
        // Arrange
        var buildingId = await SeedBuildingAsync();

        var request = CreateValidUpdateBuildingDto() with
        {
            CityId = TestConstants.NonExistingId
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/brokers/buildings/{buildingId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBuilding_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var buildingId = await SeedBuildingAsync();

        var request = CreateValidUpdateBuildingDto() with
        {
            AddressStreet = ""
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/brokers/buildings/{buildingId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Helpers

    private async Task<int> SeedClientAsync()
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();

        var client = new Client
        {
            ClientType = ClientType.Individual,
            Name = "Building Test Client",
            IdentificationNumber = $"TEST-{Guid.NewGuid():N}",
            Email = "building.test@test.com",
            Phone = "0712345678",
            Address = "Cluj-Napoca",
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Clients.Add(client);
        await dbContext.SaveChangesAsync();

        return client.ClientId;
    }

    private async Task<int> SeedBuildingAsync(int? clientId = null)
    {
        var actualClientId = clientId ?? await SeedClientAsync();

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();

        var building = new Building
        {
            ClientId = actualClientId,
            CityId = 1,
            AddressStreet = "Memorandumului",
            AddressStreetNumber = "25A",
            ConstructionYear = 2015,
            BuildingType = BuildingType.Residential,
            NumberOfFloors = 4,
            SurfaceArea = 185.50m,
            InsuredValue = 750_000.00m,
            RiskIndicators = "Flood zone",
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Buildings.Add(building);
        await dbContext.SaveChangesAsync();

        return building.BuildingId;
    }

    private static CreateBuildingDto CreateValidBuildingDto()
    {
        return new CreateBuildingDto(
            BuildingType.Residential,
            "Memorandumului",
            "25A",
            1,
            2015,
            4,
            185.50m,
            750_000.00m,
            "Flood zone");
    }

    private static UpdateBuildingDto CreateValidUpdateBuildingDto()
    {
        return new UpdateBuildingDto(
            BuildingType.Office,
            "Republicii",
            "10B",
            1,
            2020,
            6,
            350.50m,
            1_250_000.00m,
            "Earthquake risk zone");
    }

    #endregion
}
