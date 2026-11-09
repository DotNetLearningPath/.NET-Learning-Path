using System.Net;
using System.Net.Http.Json;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.DTOs.Pagination;

namespace InsuranceManagement.IntegrationTests.Controllers
{
    [TestClass]
    public class CountriesControllerTests
    {
        private static readonly Guid SeededCountryId =
            Guid.Parse("11111111-1111-4111-8111-111111111111");

        private const string BrokerRoute = "/api/brokers/countries";
        private const string AdminRoute = "/api/admin/countries";

        private const string SeededCountryName = "Romania";
        private const string TestCountryName = "Testland";

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
        public async Task GetAll_ReturnsSeededCountries()
        {
            using var response = await _client.GetAsync(BrokerRoute);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content
                .ReadFromJsonAsync<PagedResult<CountryDto>>();

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.TotalCount);
            Assert.AreEqual(SeededCountryName, result.Items.Single().Name);
        }

        [TestMethod]
        public async Task GetById_WhenCountryExists_ReturnsCountry()
        {
            using var response = await _client.GetAsync(
                $"{BrokerRoute}/{SeededCountryId}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var country = await response.Content.ReadFromJsonAsync<CountryDto>();

            Assert.IsNotNull(country);
            Assert.AreEqual(SeededCountryId, country.Id);
            Assert.AreEqual(SeededCountryName, country.Name);
        }

        [TestMethod]
        public async Task GetById_WhenCountryDoesNotExist_ReturnsNotFound()
        {
            using var response = await _client.GetAsync(
                $"{BrokerRoute}/{Guid.NewGuid()}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_WhenValid_ReturnsCreatedCountry()
        {
            var country = await CreateCountryAsync(TestCountryName);

            Assert.AreNotEqual(Guid.Empty, country.Id);
            Assert.AreEqual(TestCountryName, country.Name);

            using var response = await _client.GetAsync(
                $"{BrokerRoute}/{country.Id}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task Update_WhenCountryExists_ReturnsUpdatedCountry()
        {
            var country = await CreateCountryAsync(TestCountryName);
            var request = new UpdateCountryDto { Name = $"Updated {TestCountryName}" };

            using var response = await _client.PutAsJsonAsync(
                $"{AdminRoute}/{country.Id}", request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            using var getResponse = await _client.GetAsync(
                $"{BrokerRoute}/{country.Id}");

            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);

            var updatedCountry = await getResponse.Content
                .ReadFromJsonAsync<CountryDto>();

            Assert.IsNotNull(updatedCountry);
            Assert.AreEqual(country.Id, updatedCountry.Id);
            Assert.AreEqual(request.Name, updatedCountry.Name);
        }

        [TestMethod]
        public async Task Update_WhenCountryDoesNotExist_ReturnsNotFound()
        {
            var request = new UpdateCountryDto { Name = "Updated Country" };

            using var response = await _client.PutAsJsonAsync(
                $"{AdminRoute}/{Guid.NewGuid()}", request);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenCountryExists_ReturnsNoContent()
        {
            var country = await CreateCountryAsync("Country to delete");

            using var response = await _client.DeleteAsync(
                $"{AdminRoute}/{country.Id}");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            using var getResponse = await _client.GetAsync(
                $"{BrokerRoute}/{country.Id}");

            Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenCountryDoesNotExist_ReturnsNotFound()
        {
            using var response = await _client.DeleteAsync(
                $"{AdminRoute}/{Guid.NewGuid()}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenCountryHasCounties_ReturnsConflict()
        {
            using var response = await _client.DeleteAsync(
                $"{AdminRoute}/{SeededCountryId}");

            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        }

        private async Task<CountryDto> CreateCountryAsync(string name)
        {
            var request = new CreateCountryDto { Name = name };

            using var response = await _client.PostAsJsonAsync(
                AdminRoute, request);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var country = await response.Content.ReadFromJsonAsync<CountryDto>();

            Assert.IsNotNull(country);
            return country;
        }
    }
}
