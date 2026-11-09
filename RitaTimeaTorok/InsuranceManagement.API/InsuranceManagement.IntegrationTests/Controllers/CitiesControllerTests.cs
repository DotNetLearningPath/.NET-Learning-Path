using System.Net;
using System.Net.Http.Json;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.DTOs.Pagination;

namespace InsuranceManagement.IntegrationTests.Controllers
{
    [TestClass]
    public class CitiesControllerTests
    {
        private static readonly Guid SeededCountyId =
            Guid.Parse("22222222-2222-4222-8222-222222222222");
        private static readonly Guid SeededCityId =
            Guid.Parse("44444444-4444-4444-8444-444444444444");

        private const string BrokerRoute = "/api/brokers/cities";
        private const string AdminRoute = "/api/admin/cities";

        private const string ClujNapocaCityName = "Cluj-Napoca";
        private const string BucurestiCityName = "Bucuresti";
        private const string TestCityName = "Test City";

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
        public async Task GetAll_ReturnsSeededCities()
        {
            using var response = await _client.GetAsync(BrokerRoute);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<PagedResult<CityDto>>();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.TotalCount);
            CollectionAssert.AreEquivalent(
                new[] { ClujNapocaCityName, BucurestiCityName },
                result.Items.Select(city => city.Name).ToArray());
        }

        [TestMethod]
        public async Task GetById_WhenCityExists_ReturnsCity()
        {
            using var response = await _client.GetAsync($"{BrokerRoute}/{SeededCityId}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var city = await response.Content.ReadFromJsonAsync<CityDto>();

            Assert.IsNotNull(city);
            Assert.AreEqual(SeededCityId, city.Id);
            Assert.AreEqual(ClujNapocaCityName, city.Name);
            Assert.AreEqual(SeededCountyId, city.CountyId);
        }

        [TestMethod]
        public async Task GetById_WhenCityDoesNotExist_ReturnsNotFound()
        {
            using var response = await _client.GetAsync($"{BrokerRoute}/{Guid.NewGuid()}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_WhenValid_ReturnsCreatedCity()
        {
            var city = await CreateCityAsync(TestCityName);

            Assert.AreNotEqual(Guid.Empty, city.Id);
            Assert.AreEqual(TestCityName, city.Name);
            Assert.AreEqual(SeededCountyId, city.CountyId);

            using var response = await _client.GetAsync($"{BrokerRoute}/{city.Id}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task Update_WhenCityExists_ReturnsUpdatedCity()
        {
            var city = await CreateCityAsync(TestCityName);
            var request = new UpdateCityDto
            {
                Name = "Updated City",
                CountyId = SeededCountyId
            };

            using var response = await _client.PutAsJsonAsync(
                $"{AdminRoute}/{city.Id}", request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            using var getResponse = await _client.GetAsync($"{BrokerRoute}/{city.Id}");

            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);

            var updatedCity = await getResponse.Content.ReadFromJsonAsync<CityDto>();

            Assert.IsNotNull(updatedCity);
            Assert.AreEqual(city.Id, updatedCity.Id);
            Assert.AreEqual(request.Name, updatedCity.Name);
            Assert.AreEqual(request.CountyId, updatedCity.CountyId);
        }

        [TestMethod]
        public async Task Update_WhenCityDoesNotExist_ReturnsNotFound()
        {
            var request = new UpdateCityDto
            {
                Name = "Updated City",
                CountyId = SeededCountyId
            };

            using var response = await _client.PutAsJsonAsync(
                $"{AdminRoute}/{Guid.NewGuid()}", request);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenCityExists_ReturnsNoContent()
        {
            var city = await CreateCityAsync(TestCityName);

            using var response = await _client.DeleteAsync($"{AdminRoute}/{city.Id}");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            using var getResponse = await _client.GetAsync($"{BrokerRoute}/{city.Id}");

            Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenCityDoesNotExist_ReturnsNotFound()
        {
            using var response = await _client.DeleteAsync($"{AdminRoute}/{Guid.NewGuid()}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task<CityDto> CreateCityAsync(string name)
        {
            var request = new CreateCityDto
            {
                Name = name,
                CountyId = SeededCountyId
            };

            using var response = await _client.PostAsJsonAsync(AdminRoute, request);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var city = await response.Content.ReadFromJsonAsync<CityDto>();

            Assert.IsNotNull(city);
            return city;
        }
    }
}
