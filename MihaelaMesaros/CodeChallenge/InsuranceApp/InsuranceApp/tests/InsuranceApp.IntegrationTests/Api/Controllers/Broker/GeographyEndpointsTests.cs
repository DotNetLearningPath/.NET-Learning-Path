using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Geography;
using InsuranceApp.IntegrationTests.Common;
using InsuranceApp.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;

namespace InsuranceApp.IntegrationTests.Api.Controllers.Broker;

public sealed class GeographyEndpointsTests(InsuranceAppWebApplicationFactory factory)
    : IntegrationTestBase(factory), IClassFixture<InsuranceAppWebApplicationFactory>
{
    #region Countries

    [Fact]
    public async Task GetCountries_ReturnsOkWithCountries()
    {
        // Arrange
        await SeedAsync(TestData.Countries);

        // Act
        var response = await HttpClient.GetAsync("/api/brokers/countries");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var countries = await response.Content.ReadFromJsonAsync<List<CountryDto>>();

        Assert.NotNull(countries);
        Assert.Equal(TestData.Countries.Count, countries.Count);

        foreach (var expectedCountry in TestData.Countries)
        {
            Assert.Contains(countries, country => country.CountryId == expectedCountry.CountryId && country.Name == expectedCountry.Name);
        }
    }

    #endregion

    #region Counties

    [Fact]
    public async Task GetCounties_WhenCountryExists_ReturnsOkWithCounties()
    {
        // Arrange
        var country = TestData.Countries[0];
        var expectedCounties = TestData.Counties.Where(x => x.CountryId == country.CountryId).ToList();

        await SeedAsync(TestData.Countries);
        await SeedAsync(TestData.Counties);

        // Act
        var response = await HttpClient.GetAsync($"/api/brokers/countries/{country.CountryId}/counties");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var counties = await response.Content.ReadFromJsonAsync<List<CountyDto>>();

        Assert.NotNull(counties);
        Assert.Equal(expectedCounties.Count, counties.Count);

        foreach (var expectedCounty in expectedCounties)
        {
            Assert.Contains(counties, county => county.CountyId == expectedCounty.CountyId && county.Name == expectedCounty.Name);
        }
    }

    [Fact]
    public async Task GetCounties_WhenCountryDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/brokers/countries/{TestData.NonExistingId}/counties");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCounties_WhenCountryDoesNotExist_ReturnsNotFoundProblemDetails()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/brokers/countries/{TestData.NonExistingId}/counties");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
        Assert.NotNull(problem.Detail);
    }

    [Fact]
    public async Task GetCounties_EmptyCountryId_ReturnsBadRequest()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/brokers/countries/{Guid.Empty}/counties");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal(GeographyErrors.InvalidCountryId.Description, problem.Detail);
        Assert.Equal(GeographyErrors.InvalidCountryId.Code, problem.Extensions["code"]?.ToString());
    }

    #endregion

    #region Cities

    [Fact]
    public async Task GetCities_WhenCountyExists_ReturnsOkWithCities()
    {
        // Arrange
        var county = TestData.Counties[0];
        var expectedCities = TestData.Cities.Where(x => x.CountyId == county.CountyId).ToList();

        await SeedAsync(TestData.Countries);
        await SeedAsync(TestData.Counties);
        await SeedAsync(TestData.Cities);

        // Act
        var response = await HttpClient.GetAsync($"/api/brokers/counties/{county.CountyId}/cities");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var cities = await response.Content.ReadFromJsonAsync<List<CityDto>>();

        Assert.NotNull(cities);
        Assert.Equal(expectedCities.Count, cities.Count);

        foreach (var expectedCity in expectedCities)
        {
            Assert.Contains(cities, city => city.CityId == expectedCity.CityId && city.Name == expectedCity.Name);
        }
    }

    [Fact]
    public async Task GetCities_WhenCountyDoesNotExist_ReturnsNotFound()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/brokers/counties/{TestData.NonExistingId}/cities");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetCities_EmptyCountyId_ReturnsBadRequest()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/brokers/counties/{Guid.Empty}/cities");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal(GeographyErrors.InvalidCountyId.Description, problem.Detail);
        Assert.Equal(GeographyErrors.InvalidCountyId.Code, problem.Extensions["code"]?.ToString());
    }

    #endregion
}
