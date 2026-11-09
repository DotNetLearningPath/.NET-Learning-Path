using System.Net;
using System.Net.Http.Json;
using InsuranceManagement.Application.DTOs.Building;
using InsuranceManagement.Application.DTOs.Client;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.IntegrationTests.Controllers
{
    [TestClass]
    public class BuildingsControllerTests
    {
        private static readonly Guid SeededCityId =
            Guid.Parse("44444444-4444-4444-8444-444444444444");

        private const string BuildingRoute = "/api/brokers/buildings";
        private const string ClientsRoute = "/api/brokers/clients";

        private const string OwnerName = "Building Owner";
        private const string OwnerEmail = "owner@example.com";
        private const string MainStreet = "Main Street";
        private const string MainStreetNumber = "10";
        private const string UpdatedStreet = "Updated Street";
        private const string UpdatedStreetNumber = "20A";
        private const string SeededCityName = "Cluj-Napoca";
        private const string SeededCountyName = "Cluj";
        private const string SeededCountryName = "Romania";
        private const string GuidCompactFormat = "N";

        private CustomWebApplicationFactory _factory = null!;
        private HttpClient _client = null!;

        [TestInitialize]
        public void SetUp()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
        }

        [TestCleanup]
        public async Task CleanUp()
        {
            _client.Dispose();
            await _factory.DisposeAsync();
        }

        [TestMethod]
        public async Task GetAll_ReturnsCreatedBuilding()
        {
            var building = await CreateBuildingAsync();

            using var response = await _client.GetAsync(BuildingRoute);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<PagedResult<BuildingDto>>();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.TotalCount);
            Assert.AreEqual(building.Id, result.Items.Single().Id);
            Assert.AreEqual(MainStreet, result.Items.Single().Street);
        }

        [TestMethod]
        public async Task GetById_WhenBuildingExists_ReturnsBuilding()
        {
            var building = await CreateBuildingAsync();

            using var response = await _client.GetAsync($"{BuildingRoute}/{building.Id}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var details = await response.Content.ReadFromJsonAsync<BuildingDetailsDto>();

            Assert.IsNotNull(details);
            Assert.AreEqual(building.Id, details.Id);
            Assert.AreEqual(MainStreet, details.Street);
            Assert.AreEqual(MainStreetNumber, details.StreetNumber);
            Assert.AreEqual(SeededCityName, details.City.Name);
            Assert.AreEqual(SeededCountyName, details.County.Name);
            Assert.AreEqual(SeededCountryName, details.Country.Name);
        }

        [TestMethod]
        public async Task GetById_WhenBuildingDoesNotExist_ReturnsNotFound()
        {
            using var response = await _client.GetAsync($"{BuildingRoute}/{Guid.NewGuid()}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_WhenValid_ReturnsCreatedBuilding()
        {
            var building = await CreateBuildingAsync();

            Assert.AreNotEqual(Guid.Empty, building.Id);
            Assert.AreEqual(MainStreet, building.Street);
            Assert.AreEqual(MainStreetNumber, building.StreetNumber);
            Assert.AreEqual(SeededCityId, building.CityId);

            using var response = await _client.GetAsync($"{BuildingRoute}/{building.Id}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_WhenStreetIsMissing_ReturnsBadRequest()
        {
            var client = await CreateClientAsync();
            var request = CreateBuildingRequest(client.Id);
            request.Street = string.Empty;

            using var response = await _client.PostAsJsonAsync(BuildingRoute, request);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Update_WhenBuildingExists_ReturnsUpdatedBuilding()
        {
            var building = await CreateBuildingAsync();
            var request = new UpdateBuildingDto
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

            using var response = await _client.PutAsJsonAsync(
                $"{BuildingRoute}/{building.Id}", request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            using var getResponse = await _client.GetAsync($"{BuildingRoute}/{building.Id}");
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

            using var response = await _client.PutAsJsonAsync(
                $"{BuildingRoute}/{Guid.NewGuid()}", request);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenBuildingExists_ReturnsNoContent()
        {
            var building = await CreateBuildingAsync();

            using var response = await _client.DeleteAsync($"{BuildingRoute}/{building.Id}");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            using var getResponse = await _client.GetAsync($"{BuildingRoute}/{building.Id}");
            Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenBuildingDoesNotExist_ReturnsNotFound()
        {
            using var response = await _client.DeleteAsync($"{BuildingRoute}/{Guid.NewGuid()}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task<BuildingDto> CreateBuildingAsync()
        {
            var client = await CreateClientAsync();

            using var response = await _client.PostAsJsonAsync(
                BuildingRoute, CreateBuildingRequest(client.Id));

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
            using var response = await _client.PostAsJsonAsync(
                ClientsRoute,
                new CreateClientDto
                {
                    Type = ClientType.Individual,
                    Name = OwnerName,
                    IdentificationNumber = Guid.NewGuid().ToString(GuidCompactFormat)[..20],
                    Email = OwnerEmail
                });

            Assert.AreEqual(
                HttpStatusCode.Created,
                response.StatusCode,
                await response.Content.ReadAsStringAsync());

            var client = await response.Content.ReadFromJsonAsync<ClientDto>();

            Assert.IsNotNull(client);
            return client;
        }

        private static CreateBuildingDto CreateBuildingRequest(Guid clientId) => new()
        {
            ClientId = clientId,
            Street = MainStreet,
            StreetNumber = MainStreetNumber,
            CityId = SeededCityId,
            ConstructionYear = 2000,
            Type = BuildingType.Residential,
            NumberOfFloors = 2,
            SurfaceArea = 120,
            InsuredValue = 100000
        };

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