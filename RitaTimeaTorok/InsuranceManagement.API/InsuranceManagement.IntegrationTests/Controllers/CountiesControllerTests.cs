using System.Net;
using System.Net.Http.Json;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.IntegrationTests.Helpers;

namespace InsuranceManagement.IntegrationTests.Controllers
{
    [TestClass]
    public class CountiesControllerTests : IntegrationTestBase
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

        [TestMethod]
        public async Task GetAll_ReturnsSeededCounties()
        {
            // Act
            using var response = await HttpClient.GetAsync(BrokerRoute);

            // Assert
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
            // Act
            using var response = await HttpClient.GetAsync($"{BrokerRoute}/{SeededCountyId}");

            // Assert
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
            // Arrange
            var id = Guid.NewGuid();

            // Act
            using var response = await HttpClient.GetAsync($"{BrokerRoute}/{id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_WhenValid_ReturnsCreatedCounty()
        {
            // Act
            var county = await CreateCountyAsync(TestCountyName);

            using var response = await HttpClient.GetAsync($"{BrokerRoute}/{county.Id}");

            // Assert
            Assert.AreNotEqual(Guid.Empty, county.Id);
            Assert.AreEqual(TestCountyName, county.Name);
            Assert.AreEqual(SeededCountryId, county.CountryId);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task Update_WhenCountyExists_ReturnsUpdatedCounty()
        {
            // Arrange
            var county = await CreateCountyAsync(TestCountyName);
            var request = new UpdateCountyDto
            {
                Name = "Updated County",
                CountryId = SeededCountryId
            };

            // Act
            using var response = await HttpClient.PutAsJsonAsync(
                $"{AdminRoute}/{county.Id}", request);

            using var getResponse = await HttpClient.GetAsync($"{BrokerRoute}/{county.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
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
            // Arrange
            var request = new UpdateCountyDto
            {
                Name = "Updated County",
                CountryId = SeededCountryId
            };

            // Act
            using var response = await HttpClient.PutAsJsonAsync(
                $"{AdminRoute}/{Guid.NewGuid()}", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenCountyExists_ReturnsNoContent()
        {
            // Arrange
            var county = await CreateCountyAsync(TestCountyName);

            // Act
            using var response = await HttpClient.DeleteAsync($"{AdminRoute}/{county.Id}");

            using var getResponse = await HttpClient.GetAsync($"{BrokerRoute}/{county.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenCountyDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            using var response = await HttpClient.DeleteAsync($"{AdminRoute}/{id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenCountyHasCities_ReturnsConflict()
        {
            // Act
            using var response = await HttpClient.DeleteAsync($"{AdminRoute}/{SeededCountyId}");

            // Assert
            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        }

        private async Task<CountyDto> CreateCountyAsync(string name)
        {
            using var response = await HttpClient.PostAsJsonAsync(
                AdminRoute,
                TestDataFactory.CreateCountyRequest(SeededCountryId, name));

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var county = await response.Content.ReadFromJsonAsync<CountyDto>();

            Assert.IsNotNull(county);
            return county;
        }
    }

}
