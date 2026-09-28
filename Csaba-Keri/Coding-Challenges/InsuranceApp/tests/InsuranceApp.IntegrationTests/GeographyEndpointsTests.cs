using InsuranceApp.IntegrationTests.Infrastructure;
using System.Net;

namespace InsuranceApp.IntegrationTests;

public sealed class GeographyEndpointsTests : IntegrationTestBase
{
    [Fact]
    public async Task GetCountries_ReturnsSeededCountriesInNameOrder()
    {
        // Act
        using var response = await HttpClient.GetAsync("/api/brokers/countries");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var page = await ReadJsonAsync(response);
        Assert.Equal(2, page["totalCount"]!.GetValue<int>());
        
        Assert.Equal(
            expected: ["Hungary", "Romania"],
            actual: page["items"]!.AsArray()
                .Select(item => item!["name"]!.GetValue<string>())
        );
    }

    [Fact]
    public async Task GetCountiesByCountryId_ReturnsOnlyCountiesOfRequestedCountry()
    {
        // Act
        using var response = await HttpClient.GetAsync($"/api/brokers/countries/{TestData.RomaniaId}/counties");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var page = await ReadJsonAsync(response);
        Assert.Equal(1, page["totalCount"]!.GetValue<int>());
        
        var county = Assert.Single(page["items"]!.AsArray());
        Assert.Equal(TestData.ClujId, county!["id"]!.GetValue<string>());
        Assert.Equal("Cluj", county["name"]!.GetValue<string>());
        Assert.Equal(TestData.RomaniaId, county["countryId"]!.GetValue<string>());
    }

    [Fact]
    public async Task GetCitiesByCountyId_PagedRequest_ReturnsOnlyCitiesOfRequestedCounty()
    {
        // Act
        using var response = await HttpClient.GetAsync(
            requestUri: $"/api/brokers/counties/{TestData.ClujId}/cities?pageNumber=2&pageSize=1"
        );

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var page = await ReadJsonAsync(response);
        Assert.Equal(2, page["totalCount"]!.GetValue<int>());
        Assert.Equal(2, page["pageNumber"]!.GetValue<int>());
        Assert.Equal(1, page["pageSize"]!.GetValue<int>());
        
        var city = Assert.Single(page["items"]!.AsArray());
        Assert.Equal("Turda", city!["name"]!.GetValue<string>());
        Assert.Equal(TestData.ClujId, city["countyId"]!.GetValue<string>());
    }
}
