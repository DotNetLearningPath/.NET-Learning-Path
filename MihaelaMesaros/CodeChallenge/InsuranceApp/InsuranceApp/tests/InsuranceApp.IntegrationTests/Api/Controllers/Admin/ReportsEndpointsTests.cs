using InsuranceApp.Application.DTOs.Reports;
using InsuranceApp.Domain.Enums;
using InsuranceApp.IntegrationTests.Common;
using InsuranceApp.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Http.Json;

namespace InsuranceApp.IntegrationTests.Api.Controllers.Admin;

public sealed class ReportsEndpointsTests(WebApplicationFactorySqlServer factory)
    : IntegrationTestBaseSqlServer(factory), IClassFixture<WebApplicationFactorySqlServer>
{
    private const string BaseUrl = "/api/admin/reports";

    #region Report policies by group type

    [Theory]
    [InlineData("policies-by-country", PolicyReportGroupType.Country)]
    [InlineData("policies-by-county", PolicyReportGroupType.County)]
    [InlineData("policies-by-city", PolicyReportGroupType.City)]
    [InlineData("policies-by-broker", PolicyReportGroupType.Broker)]
    public async Task GetReportPolicies_ValidRequest_ReturnsOk(string route, PolicyReportGroupType groupType)
    {
        // Arrange
        await SeedPoliciesListAsync();

        // Act
        var response = await HttpClient.GetAsync($"{BaseUrl}/{route}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var reports = await response.Content.ReadFromJsonAsync<List<PolicyReportDto>>();

        Assert.NotNull(reports);
        Assert.NotEmpty(reports);
        Assert.All(reports, report => {
            Assert.Equal(groupType, report.GroupType);
            Assert.Equal(TestData.BaseCurrencyCode, report.BaseCurrencyCode);
        });

    }

    [Theory]
    [InlineData("policies-by-country", PolicyReportGroupType.Country, "from=2026-10-01&to=2027-10-01&status=Draft&currency=EUR&buildingType=Residential", 2)]
    [InlineData("policies-by-county", PolicyReportGroupType.County, "from=2026-10-01&to=2027-10-01&status=Draft&currency=EUR&buildingType=Residential", 2)]
    [InlineData("policies-by-city", PolicyReportGroupType.City, "from=2026-10-01&to=2027-10-01&status=Draft&currency=EUR&buildingType=Residential", 2)]
    [InlineData("policies-by-broker", PolicyReportGroupType.Broker, "from=2026-10-01&to=2027-10-01&status=Draft&currency=EUR&buildingType=Residential", 2)]
    [InlineData("policies-by-country", PolicyReportGroupType.Country, "from=2026-10-01&to=2027-10-01", 4)]
    [InlineData("policies-by-country", PolicyReportGroupType.Country, "from=2026-10-01", 4)]
    [InlineData("policies-by-country", PolicyReportGroupType.Country, "to=2027-10-01", 4)]
    [InlineData("policies-by-country", PolicyReportGroupType.Country, "status=Draft", 2)]
    [InlineData("policies-by-country", PolicyReportGroupType.Country, "currency=EUR", 2)]
    [InlineData("policies-by-country", PolicyReportGroupType.Country, "buildingType=Residential", 2)]
    public async Task GetReportPolicies_ValidFilters_ReturnsMatchingResults(string route, PolicyReportGroupType groupType, string queryString, int expectedCount)
    {
        // Arrange
        await SeedPoliciesListAsync();

        // Act
        var response = await HttpClient.GetAsync($"{BaseUrl}/{route}?{queryString}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var reports = await response.Content.ReadFromJsonAsync<List<PolicyReportDto>>();

        Assert.NotNull(reports);
        Assert.NotEmpty(reports);
        Assert.All(reports, report => {
            Assert.Equal(groupType, report.GroupType);
            Assert.Equal(TestData.BaseCurrencyCode, report.BaseCurrencyCode);
        });

        Assert.Equal(expectedCount, reports.Sum(x => x.PolicyCount));
    }

    [Theory]
    [InlineData("policies-by-country", "from=2099-10-01&to=2099-10-01")]
    [InlineData("policies-by-county", "currency=ABC")]
    [InlineData("policies-by-city", "currency=ABC")]
    [InlineData("policies-by-broker", "currency=ABC")]
    public async Task GetReportPolicies_NoMatchingPolicies_ReturnsEmptyList(string route, string queryString)
    {
        // Arrange
        await SeedPoliciesListAsync();

        // Act
        var response = await HttpClient.GetAsync($"{BaseUrl}/{route}?{queryString}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var reports = await response.Content.ReadFromJsonAsync<List<PolicyReportDto>>();

        Assert.NotNull(reports);
        Assert.Empty(reports);
    }

    [Theory]
    [InlineData("policies-by-country", "from=2027-10-01&to=2026-10-01")]
    [InlineData("policies-by-county", "status=Invalid")]
    [InlineData("policies-by-city", "status=999")]
    [InlineData("policies-by-broker", "currency=00000000-0000-0000-0000-000000000000")]
    [InlineData("policies-by-broker", "buildingType=00000000-0000-0000-0000-000000000000")]
    public async Task GetReportPolicies_InvalidFilters_ReturnsBadRequest(string route, string queryString)
    {
        // Act
        var response = await HttpClient.GetAsync($"{BaseUrl}/{route}?{queryString}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
    }

    [Theory]
    [InlineData("policies-by-country", PolicyReportGroupType.Country)]
    [InlineData("policies-by-county", PolicyReportGroupType.County)]
    [InlineData("policies-by-city", PolicyReportGroupType.City)]
    [InlineData("policies-by-broker", PolicyReportGroupType.Broker)]
    public async Task GetReportPolicies_ValidRequest_ReturnsCorrectTotals(string route, PolicyReportGroupType groupType)
    {
        // Arrange
        var policies = await SeedPoliciesListAsync();

        // Act
        var response = await HttpClient.GetAsync($"{BaseUrl}/{route}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var reports = await response.Content.ReadFromJsonAsync<List<PolicyReportDto>>();

        Assert.NotNull(reports);
        Assert.Equal(policies.Count, reports.Sum(x => x.PolicyCount));

        var eurReport = reports.Where(x => x.CurrencyCode == "EUR");
        Assert.Single(eurReport);
        var eurReportSingle = eurReport.First();
        Assert.Equal(2, eurReportSingle.PolicyCount);
        Assert.Equal(1600m, eurReportSingle.TotalFinalPremium);
        Assert.Equal(8352m, eurReportSingle.TotalFinalPremiumInBaseCurrency);

        var ronReport = reports.Where(x => x.CurrencyCode == "RON");
        Assert.Single(ronReport);
        var ronReportSingle = ronReport.First();
        Assert.Equal(2, ronReportSingle.PolicyCount);
        Assert.Equal(3850m, ronReportSingle.TotalFinalPremium);
        Assert.Equal(3850m, ronReportSingle.TotalFinalPremiumInBaseCurrency);

        Assert.All(reports, report => {
            Assert.Equal(groupType, report.GroupType);
            Assert.Equal(TestData.BaseCurrencyCode, report.BaseCurrencyCode);
        });

        if (groupType == PolicyReportGroupType.Country)
        {
            Assert.Equal(TestData.Countries[0].CountryId, eurReportSingle.GroupId);
            Assert.Equal("Romania", eurReportSingle.GroupName);

            Assert.Equal(TestData.Countries[0].CountryId, ronReportSingle.GroupId);
            Assert.Equal("Romania", ronReportSingle.GroupName);
        }
        else if (groupType == PolicyReportGroupType.County)
        {
            Assert.Equal(TestData.Counties[0].CountyId, eurReportSingle.GroupId);
            Assert.Equal("Cluj", eurReportSingle.GroupName);

            Assert.Equal(TestData.Counties[1].CountyId, ronReportSingle.GroupId);
            Assert.Equal("Brasov", ronReportSingle.GroupName);
        }
        else if (groupType == PolicyReportGroupType.City)
        {
            Assert.Equal(TestData.Cities[0].CityId, eurReportSingle.GroupId);
            Assert.Equal("Cluj-Napoca", eurReportSingle.GroupName);

            Assert.Equal(TestData.Cities[1].CityId, ronReportSingle.GroupId);
            Assert.Equal("Brasov", ronReportSingle.GroupName);
        }
        else if (groupType == PolicyReportGroupType.Broker)
        {
            Assert.Equal(TestData.BrokersList[0].BrokerId, eurReportSingle.GroupId);
            Assert.Equal("First Broker", eurReportSingle.GroupName);

            Assert.Equal(TestData.BrokersList[1].BrokerId, ronReportSingle.GroupId);
            Assert.Equal("Second Broker", ronReportSingle.GroupName);
        }
    }

    #endregion
}
