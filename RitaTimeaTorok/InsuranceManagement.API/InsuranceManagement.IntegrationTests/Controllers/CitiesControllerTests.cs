using System.Net;
using System.Net.Http.Json;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.IntegrationTests.Helpers;

namespace InsuranceManagement.IntegrationTests.Controllers
{
    [TestClass]
    public class CitiesControllerTests : IntegrationTestBase
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

        [TestMethod]
        public async Task GetAll_ReturnsSeededCities()
        {
            // Act
            using var response = await HttpClient.GetAsync(BrokerRoute);

            // Assert
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
            // Act
            using var response = await HttpClient.GetAsync($"{BrokerRoute}/{SeededCityId}");

            // Assert
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
            // Arrange
            var id = Guid.NewGuid();

            // Act
            using var response = await HttpClient.GetAsync($"{BrokerRoute}/{id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_WhenValid_ReturnsCreatedCity()
        {
            // Act
            var city = await CreateCityAsync(TestCityName);
            using var response = await HttpClient.GetAsync($"{BrokerRoute}/{city.Id}");

            // Assert
            Assert.AreNotEqual(Guid.Empty, city.Id);
            Assert.AreEqual(TestCityName, city.Name);
            Assert.AreEqual(SeededCountyId, city.CountyId);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task Update_WhenCityExists_ReturnsUpdatedCity()
        {
            // Arrange
            var city = await CreateCityAsync(TestCityName);
            var request = new UpdateCityDto
            {
                Name = "Updated City",
                CountyId = SeededCountyId
            };

            // Act
            using var response = await HttpClient.PutAsJsonAsync(
                $"{AdminRoute}/{city.Id}", request);

            using var getResponse = await HttpClient.GetAsync($"{BrokerRoute}/{city.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
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
            // Arrange
            var request = new UpdateCityDto
            {
                Name = "Updated City",
                CountyId = SeededCountyId
            };

            // Act
            using var response = await HttpClient.PutAsJsonAsync(
                $"{AdminRoute}/{Guid.NewGuid()}", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenCityExists_ReturnsNoContent()
        {
            // Arrange
            var city = await CreateCityAsync(TestCityName);

            // Act
            using var response = await HttpClient.DeleteAsync($"{AdminRoute}/{city.Id}");

            using var getResponse = await HttpClient.GetAsync($"{BrokerRoute}/{city.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenCityDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            using var response = await HttpClient.DeleteAsync($"{AdminRoute}/{id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        private async Task<CityDto> CreateCityAsync(string name)
        {
            using var response = await HttpClient.PostAsJsonAsync(
                AdminRoute,
                TestDataFactory.CreateCityRequest(SeededCountyId, name));

            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var city = await response.Content.ReadFromJsonAsync<CityDto>();

            Assert.IsNotNull(city);
            return city;
        }
    }
}
