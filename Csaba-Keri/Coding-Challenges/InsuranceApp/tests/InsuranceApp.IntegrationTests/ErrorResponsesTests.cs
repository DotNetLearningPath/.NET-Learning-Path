using InsuranceApp.IntegrationTests.Infrastructure;
using System.Net;
using System.Net.Http.Json;

namespace InsuranceApp.IntegrationTests;

public sealed class ErrorResponsesTests : IntegrationTestBase
{
    [Theory]
    [InlineData("buildings/{id}")]
    [InlineData("clients/{id}")]
    [InlineData("clients/{id}/buildings")]
    [InlineData("countries/{id}/counties")]
    [InlineData("counties/{id}/cities")]
    public async Task GetResourceById_UnknownId_ReturnsNotFoundProlem(string route)
    {
        // Arrange
        var url = "/api/brokers/" + route.Replace("{id}", Guid.NewGuid().ToString());

        // Act
        using var response = await HttpClient.GetAsync(url);

        // Assert
        var problem = await AssertProblemAsync(response, HttpStatusCode.NotFound);

        Assert.Equal(url, problem["instance"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("buildings/not-a-guid")]
    [InlineData("clients/not-a-guid")]
    [InlineData("clients/not-a-guid/buildings")]
    [InlineData("countries/not-a-guid/counties")]
    [InlineData("counties/not-a-guid/cities")]
    [InlineData("clients?pageNumber=0")]
    [InlineData("countries?pageSize=101")]
    public async Task GetResource_InvalidInput_ReturnsValidationProblem(string route)
    {
        // Arrange
        var url = "/api/brokers/" + route;

        // Act
        using var response = await HttpClient.GetAsync(url);

        // Assert
        var problem = await AssertProblemAsync(response, HttpStatusCode.BadRequest);

        Assert.NotEmpty(problem["errors"]!.AsObject());
    }

    [Theory]
    [InlineData("clients")]
    [InlineData("buildings")]
    public async Task UpdateResourceById_UnknownId_ReturnsNotFoundProblem(string resource)
    {
        // Arrange
        var url = $"/api/brokers/{resource}/{Guid.NewGuid()}";
        var request = resource == "clients" ? TestData.Client() : TestData.Building();

        // Act
        using var response = await HttpClient.PutAsJsonAsync(url, request);

        // Assert
        await AssertProblemAsync(response, HttpStatusCode.NotFound);
    }
}
