using System.Net;
using System.Net.Http.Json;
using InsuranceManagement.Application.DTOs.Client;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Domain.Enums;
using InsuranceManagement.IntegrationTests.Helpers;

namespace InsuranceManagement.IntegrationTests.Controllers
{
    [TestClass]
    public class ClientsControllerTests : IntegrationTestBase
    {
        private static readonly Guid SeededCityId =
            Guid.Parse("44444444-4444-4444-8444-444444444444");

        private const string ClientRoute = "/api/brokers/clients";
        private const string BuildingRoute = "/api/brokers/buildings";

        private const string TestClientName = "Test Client";
        private const string TestClientIdentifier = "CLIENT-001";
        private const string FirstClientName = "First Client";
        private const string SecondClientName = "Second Client";
        private const string UpdatedClientName = "Updated Client";
        private const string ClientEmail = "client@example.com";
        private const string UpdatedClientEmail = "updated@example.com";
        private const string UpdatedClientPhoneNumber = "555-0101";
        private const string UpdatedClientAddress = "2 Main Street";

        [TestMethod]
        public async Task GetAll_ReturnsCreatedClient()
        {
            var createdClient = await CreateClientAsync(TestClientName, TestClientIdentifier);

            using var response = await HttpClient.GetAsync(ClientRoute);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<PagedResult<ClientDto>>();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.TotalCount);
            Assert.AreEqual(createdClient.Id, result.Items.Single().Id);
            Assert.AreEqual(TestClientName, result.Items.Single().Name);
        }

        [TestMethod]
        public async Task GetById_WhenClientExists_ReturnsClient()
        {
            var createdClient = await CreateClientAsync(TestClientName, TestClientIdentifier);

            using var response = await HttpClient.GetAsync($"{ClientRoute}/{createdClient.Id}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var client = await response.Content.ReadFromJsonAsync<ClientDetailsDto>();

            Assert.IsNotNull(client);
            Assert.AreEqual(createdClient.Id, client.Id);
            Assert.AreEqual(TestClientName, client.Name);
            Assert.AreEqual(TestClientIdentifier, client.IdentificationNumber);
            Assert.AreEqual(0, client.Buildings.Count);
        }

        [TestMethod]
        public async Task GetById_WhenClientDoesNotExist_ReturnsNotFound()
        {
            using var response = await HttpClient.GetAsync($"{ClientRoute}/{Guid.NewGuid()}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_WhenValid_ReturnsCreatedClient()
        {
            var client = await CreateClientAsync(TestClientName, TestClientIdentifier);

            Assert.AreNotEqual(Guid.Empty, client.Id);
            Assert.AreEqual(TestClientName, client.Name);
            Assert.AreEqual(TestClientIdentifier, client.IdentificationNumber);

            using var response = await HttpClient.GetAsync($"{ClientRoute}/{client.Id}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_WhenIdentifierAlreadyExists_ReturnsConflict()
        {
            await CreateClientAsync(FirstClientName, TestClientIdentifier);
            var request = TestDataFactory.CreateClientRequest(
                SecondClientName, TestClientIdentifier, ClientEmail);

            using var response = await HttpClient.PostAsJsonAsync(ClientRoute, request);

            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_WhenNameIsMissing_ReturnsBadRequest()
        {
            var request = TestDataFactory.CreateClientRequest(
                string.Empty, TestClientIdentifier, ClientEmail);

            using var response = await HttpClient.PostAsJsonAsync(ClientRoute, request);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task Update_WhenClientExists_ReturnsUpdatedClient()
        {
            var client = await CreateClientAsync(TestClientName, TestClientIdentifier);
            var request = new UpdateClientDto
            {
                Type = ClientType.Company,
                Name = UpdatedClientName,
                Email = UpdatedClientEmail,
                PhoneNumber = UpdatedClientPhoneNumber,
                PrimaryAddress = UpdatedClientAddress
            };

            using var response = await HttpClient.PutAsJsonAsync($"{ClientRoute}/{client.Id}", request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            using var getResponse = await HttpClient.GetAsync($"{ClientRoute}/{client.Id}");
            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);

            var updatedClient = await getResponse.Content.ReadFromJsonAsync<ClientDetailsDto>();

            Assert.IsNotNull(updatedClient);
            Assert.AreEqual(client.Id, updatedClient.Id);
            Assert.AreEqual(request.Type, updatedClient.Type);
            Assert.AreEqual(request.Name, updatedClient.Name);
            Assert.AreEqual(TestClientIdentifier, updatedClient.IdentificationNumber);
            Assert.AreEqual(request.Email, updatedClient.Email);
            Assert.AreEqual(request.PhoneNumber, updatedClient.PhoneNumber);
            Assert.AreEqual(request.PrimaryAddress, updatedClient.PrimaryAddress);
        }

        [TestMethod]
        public async Task Update_WhenClientDoesNotExist_ReturnsNotFound()
        {
            var request = new UpdateClientDto
            {
                Type = ClientType.Individual,
                Name = UpdatedClientName,
                Email = UpdatedClientEmail
            };

            using var response = await HttpClient.PutAsJsonAsync(
                $"{ClientRoute}/{Guid.NewGuid()}", request);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenClientExists_ReturnsNoContent()
        {
            var client = await CreateClientAsync(TestClientName, TestClientIdentifier);

            using var response = await HttpClient.DeleteAsync($"{ClientRoute}/{client.Id}");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            using var getResponse = await HttpClient.GetAsync($"{ClientRoute}/{client.Id}");
            Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenClientDoesNotExist_ReturnsNotFound()
        {
            using var response = await HttpClient.DeleteAsync($"{ClientRoute}/{Guid.NewGuid()}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenClientOwnsBuilding_ReturnsConflict()
        {
            var client = await CreateClientAsync(TestClientName, TestClientIdentifier);
            var buildingRequest = TestDataFactory.CreateBuildingRequest(
                client.Id,
                SeededCityId);

            using var buildingResponse = await HttpClient.PostAsJsonAsync(
                BuildingRoute, buildingRequest);

            Assert.AreEqual(
                HttpStatusCode.Created,
                buildingResponse.StatusCode,
                await buildingResponse.Content.ReadAsStringAsync());

            using var deleteResponse = await HttpClient.DeleteAsync(
                $"{ClientRoute}/{client.Id}");

            Assert.AreEqual(HttpStatusCode.Conflict, deleteResponse.StatusCode);
        }

        private async Task<ClientDto> CreateClientAsync(string name, string identifier)
        {
            using var response = await HttpClient.PostAsJsonAsync(
                ClientRoute,
                TestDataFactory.CreateClientRequest(name, identifier, ClientEmail));

            Assert.AreEqual(
                HttpStatusCode.Created,
                response.StatusCode,
                await response.Content.ReadAsStringAsync());

            var client = await response.Content.ReadFromJsonAsync<ClientDto>();

            Assert.IsNotNull(client);
            return client;
        }
    }

}
