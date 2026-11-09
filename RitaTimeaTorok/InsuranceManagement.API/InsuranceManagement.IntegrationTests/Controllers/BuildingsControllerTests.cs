using System.Net;
using System.Net.Http.Json;
using InsuranceManagement.Application.DTOs.Building;
using InsuranceManagement.Application.DTOs.Client;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Domain.Enums;
using InsuranceManagement.IntegrationTests.Helpers;

namespace InsuranceManagement.IntegrationTests.Controllers
{
    [TestClass]
    public class BuildingsControllerTests : IntegrationTestBase
    {
        private static readonly Guid SeededCityId =
            Guid.Parse("44444444-4444-4444-8444-444444444444");

        private const string BuildingRoute = "/api/brokers/buildings";
        private const string ClientsRoute = "/api/brokers/clients";

        private const string OwnerName = "Building Owner";
        private const string OwnerEmail = "owner@example.com";
        private const string UpdatedStreet = "Updated Street";
        private const string UpdatedStreetNumber = "20A";
        private const string SeededCityName = "Cluj-Napoca";
        private const string SeededCountyName = "Cluj";
        private const string SeededCountryName = "Romania";

        [TestMethod]
        public async Task GetAll_ReturnsCreatedBuilding()
        {
            var building = await CreateBuildingAsync();

            using var response = await HttpClient.GetAsync(BuildingRoute);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<PagedResult<BuildingDto>>();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.TotalCount);
            Assert.AreEqual(building.Id, result.Items.Single().Id);
            Assert.AreEqual(TestDataFactory.MainStreet, result.Items.Single().Street);
        }

        [TestMethod]
        public async Task GetById_WhenBuildingExists_ReturnsBuilding()
        {
            var building = await CreateBuildingAsync();

            using var response = await HttpClient.GetAsync($"{BuildingRoute}/{building.Id}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var details = await response.Content.ReadFromJsonAsync<BuildingDetailsDto>();

            Assert.IsNotNull(details);
            Assert.AreEqual(building.Id, details.Id);
            Assert.AreEqual(TestDataFactory.MainStreet, details.Street);
            Assert.AreEqual(TestDataFactory.MainStreetNumber, details.StreetNumber);
            Assert.AreEqual(SeededCityName, details.City.Name);
            Assert.AreEqual(SeededCountyName, details.County.Name);
            Assert.AreEqual(SeededCountryName, details.Country.Name);
        }

        [TestMethod]
        public async Task GetById_WhenBuildingDoesNotExist_ReturnsNotFound()
        {
            using var response = await HttpClient.GetAsync($"{BuildingRoute}/{Guid.NewGuid()}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_WhenValid_ReturnsCreatedBuilding()
        {
            var building = await CreateBuildingAsync();

            Assert.AreNotEqual(Guid.Empty, building.Id);
            Assert.AreEqual(TestDataFactory.MainStreet, building.Street);
            Assert.AreEqual(TestDataFactory.MainStreetNumber, building.StreetNumber);
            Assert.AreEqual(SeededCityId, building.CityId);

            using var response = await HttpClient.GetAsync($"{BuildingRoute}/{building.Id}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_WhenStreetIsMissing_ReturnsBadRequest()
        {
            var client = await CreateClientAsync();
            var request = TestDataFactory.CreateBuildingRequest(client.Id, SeededCityId);
            request.Street = string.Empty;

            using var response = await HttpClient.PostAsJsonAsync(BuildingRoute, request);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Update_WhenBuildingExists_ReturnsUpdatedBuilding()
        {
            var building = await CreateBuildingAsync();
            var request = CreateUpdateBuildingRequest();

            using var response = await HttpClient.PutAsJsonAsync(
                $"{BuildingRoute}/{building.Id}", request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            using var getResponse = await HttpClient.GetAsync($"{BuildingRoute}/{building.Id}");
            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);

            var updatedBuilding = await getResponse.Content
                .ReadFromJsonAsync<BuildingDetailsDto>();

            Assert.IsNotNull(updatedBuilding);
            Assert.AreEqual(building.Id, updatedBuilding.Id);
            Assert.AreEqual(UpdatedStreet, updatedBuilding.Street);
            Assert.AreEqual(UpdatedStreetNumber, updatedBuilding.StreetNumber);
            Assert.AreEqual(request.ConstructionYear, updatedBuilding.ConstructionYear);
            Assert.AreEqual(request.Type, updatedBuilding.Type);
            Assert.AreEqual(request.NumberOfFloors, updatedBuilding.NumberOfFloors);
            Assert.AreEqual(request.SurfaceArea, updatedBuilding.SurfaceArea);
            Assert.AreEqual(request.InsuredValue, updatedBuilding.InsuredValue);
        }

        [TestMethod]
        public async Task Update_WhenBuildingDoesNotExist_ReturnsNotFound()
        {
            var request = CreateUpdateBuildingRequest();

            using var response = await HttpClient.PutAsJsonAsync(
                $"{BuildingRoute}/{Guid.NewGuid()}", request);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenBuildingExists_ReturnsNoContent()
        {
            var building = await CreateBuildingAsync();

            using var response = await HttpClient.DeleteAsync($"{BuildingRoute}/{building.Id}");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            using var getResponse = await HttpClient.GetAsync($"{BuildingRoute}/{building.Id}");
            Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenBuildingDoesNotExist_ReturnsNotFound()
        {
            using var response = await HttpClient.DeleteAsync($"{BuildingRoute}/{Guid.NewGuid()}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task<BuildingDto> CreateBuildingAsync()
        {
            var client = await CreateClientAsync();
            using var response = await HttpClient.PostAsJsonAsync(
                BuildingRoute,
                TestDataFactory.CreateBuildingRequest(client.Id, SeededCityId));

            Assert.AreEqual(
                HttpStatusCode.Created,
                response.StatusCode,
                await response.Content.ReadAsStringAsync());

            var building = await response.Content.ReadFromJsonAsync<BuildingDto>();

            Assert.IsNotNull(building);
            return building;
        }

        private async Task<ClientDto> CreateClientAsync()
        {
            using var response = await HttpClient.PostAsJsonAsync(
                ClientsRoute,
                TestDataFactory.CreateClientRequest(
                    OwnerName,
                    email: OwnerEmail));

            Assert.AreEqual(
                HttpStatusCode.Created,
                response.StatusCode,
                await response.Content.ReadAsStringAsync());

            var client = await response.Content.ReadFromJsonAsync<ClientDto>();

            Assert.IsNotNull(client);
            return client;
        }

        private static UpdateBuildingDto CreateUpdateBuildingRequest() => new()
        {
            Street = UpdatedStreet,
            StreetNumber = UpdatedStreetNumber,
            CityId = SeededCityId,
            ConstructionYear = 2001,
            Type = BuildingType.Office,
            NumberOfFloors = 3,
            SurfaceArea = 150,
            InsuredValue = 150000
        };
    }

}
