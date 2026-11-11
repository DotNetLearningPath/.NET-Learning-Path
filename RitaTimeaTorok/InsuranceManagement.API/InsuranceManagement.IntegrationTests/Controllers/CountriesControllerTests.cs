using System.Net;
using System.Net.Http.Json;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.IntegrationTests.Helpers;

namespace InsuranceManagement.IntegrationTests.Controllers
{
    [TestClass]
    public class CountriesControllerTests : IntegrationTestBase
    {
        private static readonly Guid SeededCountryId =
            Guid.Parse("11111111-1111-4111-8111-111111111111");

        private const string BrokerRoute = "/api/brokers/countries";
        private const string AdminRoute = "/api/admin/countries";

        private const string SeededCountryName = "Romania";
        private const string TestCountryName = "Testland";

        [TestMethod]
        public async Task GetAll_ReturnsSeededCountries()
        {
            // Act
            using var response = await HttpClient.GetAsync(BrokerRoute);

            // Assert
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
            // Act
            using var response = await HttpClient.GetAsync(
                $"{BrokerRoute}/{SeededCountryId}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var country = await response.Content.ReadFromJsonAsync<CountryDto>();

            Assert.IsNotNull(country);
            Assert.AreEqual(SeededCountryId, country.Id);
            Assert.AreEqual(SeededCountryName, country.Name);
        }

        [TestMethod]
        public async Task GetById_WhenCountryDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            using var response = await HttpClient.GetAsync(
                $"{BrokerRoute}/{id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Create_WhenValid_ReturnsCreatedCountry()
        {
            // Act
            var country = await CreateCountryAsync(TestCountryName);

            using var response = await HttpClient.GetAsync(
                $"{BrokerRoute}/{country.Id}");

            // Assert
            Assert.AreNotEqual(Guid.Empty, country.Id);
            Assert.AreEqual(TestCountryName, country.Name);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task Update_WhenCountryExists_ReturnsUpdatedCountry()
        {
            // Arrange
            var country = await CreateCountryAsync(TestCountryName);
            var request = new UpdateCountryDto { Name = $"Updated {TestCountryName}" };

            // Act
            using var response = await HttpClient.PutAsJsonAsync(
                $"{AdminRoute}/{country.Id}", request);

            using var getResponse = await HttpClient.GetAsync(
                $"{BrokerRoute}/{country.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
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
            // Arrange
            var request = new UpdateCountryDto { Name = "Updated Country" };

            // Act
            using var response = await HttpClient.PutAsJsonAsync(
                $"{AdminRoute}/{Guid.NewGuid()}", request);

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenCountryExists_ReturnsNoContent()
        {
            // Arrange
            var country = await CreateCountryAsync("Country to delete");

            // Act
            using var response = await HttpClient.DeleteAsync(
                $"{AdminRoute}/{country.Id}");

            using var getResponse = await HttpClient.GetAsync(
                $"{BrokerRoute}/{country.Id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
            Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenCountryDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var id = Guid.NewGuid();

            // Act
            using var response = await HttpClient.DeleteAsync(
                $"{AdminRoute}/{id}");

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task Delete_WhenCountryHasCounties_ReturnsConflict()
        {
            // Act
            using var response = await HttpClient.DeleteAsync(
                $"{AdminRoute}/{SeededCountryId}");

            // Assert
            Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
        }

        private async Task<CountryDto> CreateCountryAsync(string name)
        {
            using var response = await HttpClient.PostAsJsonAsync(
                AdminRoute,
                TestDataFactory.CreateCountryRequest(name));

            Assert.AreEqual(
                HttpStatusCode.Created,
                response.StatusCode,
                await response.Content.ReadAsStringAsync());

            var country = await response.Content.ReadFromJsonAsync<CountryDto>();

            Assert.IsNotNull(country);
            return country;
        }
    }
}
