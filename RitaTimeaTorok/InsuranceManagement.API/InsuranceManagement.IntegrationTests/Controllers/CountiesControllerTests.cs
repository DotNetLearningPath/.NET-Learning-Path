using System.Net;
using System.Net.Http.Json;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.DTOs.Pagination;

namespace InsuranceManagement.IntegrationTests.Controllers
{
    [TestClass]
    public class CountiesControllerTests
    {
        private static readonly Guid SeededCountryId =
            Guid.Parse("11111111-1111-4111-8111-111111111111");
        private static readonly Guid SeededCountyId =
            Guid.Parse("22222222-2222-4222-8222-222222222222");

        private const string BrokerRoute = "/api/brokers/counties";
        private const string AdminRoute = "/api/admin/counties";

        private const string ClujCountyName = "Cluj";
        private const string BucurestiCountyName = "Bucuresti";
        private const string TestCountyName = "Test County";

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
        public async Task GetAll_ReturnsSeededCounties()
        {
            using var response = await _client.GetAsync(BrokerRoute);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var result = await response.Content.ReadFromJsonAsync<PagedResult<CountyDto>>();

            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.TotalCount);
            CollectionAssert.AreEquivalent(
                new[] { ClujCountyName, BucurestiCountyName },
                result.Items.Select(county => county.Name).ToArray());
        }

        [TestMethod]
        public async Task GetById_WhenCountyExists_ReturnsCounty()
        {
            using var response = await _client.GetAsync($"{BrokerRoute}/{SeededCountyId}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var county = await response.Content.ReadFromJsonAsync<CountyDto>();

            Assert.IsNotNull(county);
            Assert.AreEqual(SeededCountyId, county.Id);
            Assert.AreEqual(ClujCountyName, county.Name);
            Assert.AreEqual(SeededCountryId, county.CountryId);
        }

        [TestMethod]
        public async Task GetById_WhenCountyDoesNotExist_ReturnsNotFound()
        {
            using var response = await _client.GetAsync($"{BrokerRoute}/{Guid.NewGuid()}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_WhenValid_ReturnsCreatedCounty()
        {
            var county = await CreateCountyAsync(TestCountyName);

            Assert.AreNotEqual(Guid.Empty, county.Id);
            Assert.AreEqual(TestCountyName, county.Name);
            Assert.AreEqual(SeededCountryId, county.CountryId);

            using var response = await _client.GetAsync($"{BrokerRoute}/{county.Id}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task Update_WhenCountyExists_ReturnsUpdatedCounty()
        {
            var county = await CreateCountyAsync(TestCountyName);
            var request = new UpdateCountyDto
            {
                Name = "Updated County",
                CountryId = SeededCountryId
            };

            using var response = await _client.PutAsJsonAsync(
                $"{AdminRoute}/{county.Id}", request);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            using var getResponse = await _client.GetAsync($"{BrokerRoute}/{county.Id}");

            Assert.AreEqual(HttpStatusCode.OK, getResponse.StatusCode);

            var updatedCounty = await getResponse.Content.ReadFromJsonAsync<CountyDto>();

            Assert.IsNotNull(updatedCounty);
            Assert.AreEqual(county.Id, updatedCounty.Id);
            Assert.AreEqual(request.Name, updatedCounty.Name);
            Assert.AreEqual(request.CountryId, updatedCounty.CountryId);
        }

        [TestMethod]
        public async Task Update_WhenCountyDoesNotExist_ReturnsNotFound()
        {
            var request = new UpdateCountyDto
            {
                Name = "Updated County",
                CountryId = SeededCountryId
            };

            using var response = await _client.PutAsJsonAsync(
                $"{AdminRoute}/{Guid.NewGuid()}", request);

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenCountyExists_ReturnsNoContent()
        {
            var county = await CreateCountyAsync(TestCountyName);

            using var response = await _client.DeleteAsync($"{AdminRoute}/{county.Id}");

            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);

            using var getResponse = await _client.GetAsync($"{BrokerRoute}/{county.Id}");

            Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenCountyDoesNotExist_ReturnsNotFound()
        {
            using var response = await _client.DeleteAsync($"{AdminRoute}/{Guid.NewGuid()}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenCountyHasCities_ReturnsConflict()
        {
            using var response = await _client.DeleteAsync($"{AdminRoute}/{SeededCountyId}");

            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        }

        private async Task<CountyDto> CreateCountyAsync(string name)
        {
            var request = new CreateCountyDto
            {
                Name = name,
                CountryId = SeededCountryId
            };

            using var response = await _client.PostAsJsonAsync(AdminRoute, request);

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var county = await response.Content.ReadFromJsonAsync<CountyDto>();

            Assert.IsNotNull(county);
            return county;
        }
    }

}