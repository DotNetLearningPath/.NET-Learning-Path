using InsuranceApp.Application.DTOs.Building;
using InsuranceApp.IntegrationTests.Common;
using InsuranceApp.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace InsuranceApp.IntegrationTests.Api.Controllers.Broker;

public sealed class BuildingEndpointsTests(InsuranceAppWebApplicationFactory factory)
    : IntegrationTestBase(factory), IClassFixture<InsuranceAppWebApplicationFactory>
{
    #region Get Building By Id Tests

    [Fact]
    public async Task GetBuildingById_ExistingBuilding_ReturnsOk()
    {
        // Arrange
        await SeedBuildingDependenciesAsync();
        await SeedAsync(TestData.BuildingClient);
        await SeedAsync(TestData.BuildingForRead);

        // Act
        var response = await HttpClient.GetAsync($"/api/brokers/buildings/{TestData.BuildingForRead.BuildingId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var building = await response.Content.ReadFromJsonAsync<BuildingDto>();

        Assert.NotNull(building);
        Assert.Equal(TestData.BuildingForRead.BuildingId, building.BuildingId);
        Assert.Equal(TestData.BuildingForRead.ClientId, building.ClientId);
        Assert.Equal(TestData.BuildingForRead.CityId, building.CityId);
        Assert.Equal(TestData.BuildingForRead.BuildingTypeId, building.BuildingTypeId);
        Assert.Equal(TestData.BuildingForRead.AddressStreet, building.AddressStreet);
        Assert.Equal(TestData.BuildingForRead.AddressStreetNumber, building.AddressStreetNumber);
        Assert.Equal(TestData.BuildingForRead.SurfaceArea, building.SurfaceArea);
        Assert.Equal(TestData.BuildingForRead.InsuredValue, building.InsuredValue);
    }

    [Fact]
    public async Task GetBuildingById_NonExistingBuilding_ReturnsNotFound()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/brokers/buildings/{TestData.NonExistingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Get Buildings By Client Tests

    [Fact]
    public async Task GetBuildingsByClient_ExistingClient_ReturnsOkWithBuildings()
    {
        // Arrange
        await SeedBuildingDependenciesAsync();
        await SeedAsync(TestData.BuildingClient);
        await SeedAsync(TestData.BuildingForRead);

        // Act
        var response = await HttpClient.GetAsync($"/api/brokers/clients/{TestData.BuildingClient.ClientId}/buildings");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var buildings = await response.Content.ReadFromJsonAsync<List<BuildingDto>>();

        Assert.NotNull(buildings);
        Assert.NotEmpty(buildings);
        Assert.All(buildings, building => Assert.Equal(TestData.BuildingClient.ClientId, building.ClientId));
    }

    [Fact]
    public async Task GetBuildingsByClient_ClientWithoutBuildings_ReturnsEmptyList()
    {
        // Arrange
        await SeedAsync(TestData.BuildingClient);

        // Act
        var response = await HttpClient.GetAsync($"/api/brokers/clients/{TestData.BuildingClient.ClientId}/buildings");

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
        var response = await HttpClient.GetAsync($"/api/brokers/clients/{TestData.NonExistingId}/buildings");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    #endregion

    #region Create Building Tests

    [Fact]
    public async Task CreateBuilding_ValidRequest_ReturnsCreated()
    {
        // Arrange
        await SeedBuildingDependenciesAsync();
        await SeedAsync(TestData.BuildingClient);

        var request = TestData.BuildingDtoForCreate;

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/brokers/clients/{TestData.BuildingClient.ClientId}/buildings", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var building = await response.Content.ReadFromJsonAsync<BuildingDto>();

        Assert.NotNull(building);
        Assert.NotEqual(Guid.Empty, building.BuildingId);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal($"/api/brokers/buildings/{building.BuildingId}", response.Headers.Location.AbsolutePath);
        Assert.Equal(TestData.BuildingClient.ClientId, building.ClientId);
        Assert.Equal(request.CityId, building.CityId);
        Assert.Equal(request.BuildingTypeId, building.BuildingTypeId);
        Assert.Equal(request.AddressStreet, building.AddressStreet);
        Assert.Equal(request.AddressStreetNumber, building.AddressStreetNumber);
        Assert.Equal(request.SurfaceArea, building.SurfaceArea);
        Assert.Equal(request.InsuredValue, building.InsuredValue);

        // Assert - persistence
        var persistedBuilding = await DbContext.Buildings.AsNoTracking().FirstOrDefaultAsync(x => x.BuildingId == building.BuildingId);

        Assert.NotNull(persistedBuilding);
        Assert.Equal(TestData.BuildingClient.ClientId, persistedBuilding.ClientId);
        Assert.Equal(request.CityId, persistedBuilding.CityId);
        Assert.Equal(request.BuildingTypeId, persistedBuilding.BuildingTypeId);
        Assert.Equal(request.AddressStreet, persistedBuilding.AddressStreet);
        Assert.Equal(request.AddressStreetNumber, persistedBuilding.AddressStreetNumber);
        Assert.Equal(request.SurfaceArea, persistedBuilding.SurfaceArea);
        Assert.Equal(request.InsuredValue, persistedBuilding.InsuredValue);
    }

    [Fact]
    public async Task CreateBuilding_NonExistingClient_ReturnsNotFound()
    {
        // Arrange
        await SeedBuildingDependenciesAsync();

        var request = TestData.BuildingDtoForCreate;

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/brokers/clients/{TestData.NonExistingId}/buildings", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateBuilding_NonExistingCity_ReturnsNotFound()
    {
        // Arrange
        await SeedBuildingDependenciesAsync();
        await SeedAsync(TestData.BuildingClient);

        var request = TestData.BuildingDtoForCreate with
        {
            CityId = TestData.NonExistingId
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/brokers/clients/{TestData.BuildingClient.ClientId}/buildings", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateBuilding_NonExistingBuildingType_ReturnsNotFound()
    {
        // Arrange
        await SeedBuildingDependenciesAsync();
        await SeedAsync(TestData.BuildingClient);

        var request = TestData.BuildingDtoForCreate with
        {
            BuildingTypeId = TestData.NonExistingId
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/brokers/clients/{TestData.BuildingClient.ClientId}/buildings", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateBuilding_InvalidSurfaceArea_ReturnsBadRequest()
    {
        // Arrange
        await SeedBuildingDependenciesAsync();
        await SeedAsync(TestData.BuildingClient);

        var request = TestData.BuildingDtoForCreate with
        {
            SurfaceArea = 0
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/brokers/clients/{TestData.BuildingClient.ClientId}/buildings", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateBuilding_SurfaceAreaWithTooManyDecimals_ReturnsBadRequest()
    {
        // Arrange
        await SeedBuildingDependenciesAsync();
        await SeedAsync(TestData.BuildingClient);

        var request = TestData.BuildingDtoForCreate with
        {
            SurfaceArea = 123.456m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/brokers/clients/{TestData.BuildingClient.ClientId}/buildings", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateBuilding_InsuredValueWithTooManyDecimals_ReturnsBadRequest()
    {
        // Arrange
        await SeedBuildingDependenciesAsync();
        await SeedAsync(TestData.BuildingClient);

        var request = TestData.BuildingDtoForCreate with
        {
            InsuredValue = 1000.999m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"/api/brokers/clients/{TestData.BuildingClient.ClientId}/buildings", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Update Building Tests

    [Fact]
    public async Task UpdateBuilding_ValidRequest_ReturnsOk()
    {
        // Arrange
        await SeedBuildingDependenciesAsync();
        await SeedAsync(TestData.BuildingClient);
        await SeedAsync(TestData.BuildingForRead);

        var request = TestData.BuildingDtoForUpdate;

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/brokers/buildings/{TestData.BuildingForRead.BuildingId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var building = await response.Content.ReadFromJsonAsync<BuildingDto>();

        Assert.NotNull(building);
        Assert.Equal(TestData.BuildingForRead.BuildingId, building.BuildingId);
        Assert.Equal(request.CityId, building.CityId);
        Assert.Equal(request.BuildingTypeId, building.BuildingTypeId);
        Assert.Equal(request.AddressStreet, building.AddressStreet);
        Assert.Equal(request.AddressStreetNumber, building.AddressStreetNumber);
        Assert.Equal(request.SurfaceArea, building.SurfaceArea);
        Assert.Equal(request.InsuredValue, building.InsuredValue);

        // Assert - persistence
        var persistedBuilding = await DbContext.Buildings.AsNoTracking().FirstAsync(x => x.BuildingId == TestData.BuildingForRead.BuildingId);

        Assert.Equal(request.CityId, persistedBuilding.CityId);
        Assert.Equal(request.BuildingTypeId, persistedBuilding.BuildingTypeId);
        Assert.Equal(request.AddressStreet, persistedBuilding.AddressStreet);
        Assert.Equal(request.AddressStreetNumber, persistedBuilding.AddressStreetNumber);
        Assert.Equal(request.SurfaceArea, persistedBuilding.SurfaceArea);
        Assert.Equal(request.InsuredValue, persistedBuilding.InsuredValue);
        Assert.NotNull(persistedBuilding.ModifiedAt);
    }

    [Fact]
    public async Task UpdateBuilding_NonExistingBuilding_ReturnsNotFound()
    {
        // Arrange
        await SeedBuildingDependenciesAsync();

        var request = TestData.BuildingDtoForUpdate;

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/brokers/buildings/{TestData.NonExistingId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBuilding_NonExistingCity_ReturnsNotFound()
    {
        // Arrange
        await SeedBuildingDependenciesAsync();
        await SeedAsync(TestData.BuildingClient);
        await SeedAsync(TestData.BuildingForRead);

        var request = TestData.BuildingDtoForUpdate with
        {
            CityId = TestData.NonExistingId
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/brokers/buildings/{TestData.BuildingForRead.BuildingId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBuilding_NonExistingBuildingType_ReturnsNotFound()
    {
        // Arrange
        await SeedBuildingDependenciesAsync();
        await SeedAsync(TestData.BuildingClient);
        await SeedAsync(TestData.BuildingForRead);

        var request = TestData.BuildingDtoForUpdate with
        {
            BuildingTypeId = TestData.NonExistingId
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/brokers/buildings/{TestData.BuildingForRead.BuildingId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBuilding_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        await SeedBuildingDependenciesAsync();
        await SeedAsync(TestData.BuildingClient);
        await SeedAsync(TestData.BuildingForRead);

        var request = TestData.BuildingDtoForUpdate with
        {
            AddressStreet = ""
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/brokers/buildings/{TestData.BuildingForRead.BuildingId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion
}
