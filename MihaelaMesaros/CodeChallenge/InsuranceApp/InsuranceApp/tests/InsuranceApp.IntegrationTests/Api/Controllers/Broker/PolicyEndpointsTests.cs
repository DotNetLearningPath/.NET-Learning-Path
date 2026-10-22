using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Policy;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.IntegrationTests.Common;
using InsuranceApp.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace InsuranceApp.IntegrationTests.Api.Controllers.Broker;

public sealed class PolicyEndpointsTests(WebApplicationFactorySqlite factory)
    : IntegrationTestBaseSqlite(factory), IClassFixture<WebApplicationFactorySqlite>
{
    private const string BaseUrl = "/api/brokers/policies";

    #region Create Policy Tests

    [Theory]
    [InlineData(false, 1000)]
    [InlineData(true, 1087.50)]
    public async Task CreatePolicy_ValidRequest_ReturnsCreatedAndPersistsPolicy(bool withConfigurations, decimal expectedFinalPremium)
    {
        // Arrange
        await SeedPolicyReferencesAsync();

        if (withConfigurations)
        {
            await SeedAsync(TestData.FeeConfigsList);
            await SeedAsync(TestData.RiskFactorConfigsList);
        }

        var request = TestData.PolicyDtoForCreate;

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert - HTTP
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdPolicy = await response.Content.ReadFromJsonAsync<PolicyDto>();

        Assert.NotNull(createdPolicy);
        Assert.NotEqual(Guid.Empty, createdPolicy.PolicyId);
        Assert.Equal(PolicyStatus.Draft, createdPolicy.Status);
        Assert.Equal(request.ClientId, createdPolicy.ClientId);
        Assert.Equal(request.BuildingId, createdPolicy.BuildingId);
        Assert.Equal(request.BrokerId, createdPolicy.BrokerId);
        Assert.Equal(request.CurrencyId, createdPolicy.CurrencyId);
        Assert.Equal(request.BasePremium, createdPolicy.BasePremium);
        Assert.Equal(expectedFinalPremium, createdPolicy.FinalPremium);
        Assert.False(string.IsNullOrWhiteSpace(createdPolicy.PolicyNumber));
        Assert.Equal(request.StartDate, createdPolicy.StartDate);
        Assert.Equal(request.EndDate, createdPolicy.EndDate);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal($"{BaseUrl}/{createdPolicy.PolicyId}", response.Headers.Location.AbsolutePath);

        // Assert - persistence
        var persistedPolicy = await DbContext.Policies.AsNoTracking().FirstOrDefaultAsync(x => x.PolicyId == createdPolicy.PolicyId);

        Assert.NotNull(persistedPolicy);
        Assert.Equal(PolicyStatus.Draft, persistedPolicy.Status);
        Assert.Equal(createdPolicy.PolicyNumber, persistedPolicy.PolicyNumber);
        Assert.Equal(request.BasePremium, persistedPolicy.BasePremium);
        Assert.Equal(expectedFinalPremium, persistedPolicy.FinalPremium);
    }

    [Fact]
    public async Task CreatePolicy_InvalidBasePremium_ReturnsBadRequest()
    {
        // Arrange
        var request = TestData.PolicyDtoForCreate with { BasePremium = 0m };

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Read Policy Tests

    [Fact]
    public async Task GetPolicyById_ExistingPolicy_ReturnsOk()
    {
        // Arrange
        var policyToSeed = await SeedPolicyAsync();

        // Act
        var response = await HttpClient.GetAsync($"{BaseUrl}/{policyToSeed.PolicyId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var policy = await response.Content.ReadFromJsonAsync<PolicyDto>();

        Assert.NotNull(policy);
        Assert.Equal(policyToSeed.PolicyId, policy.PolicyId);
        Assert.Equal(policyToSeed.PolicyNumber, policy.PolicyNumber);
        Assert.Equal(policyToSeed.Status, policy.Status);
    }

    [Fact]
    public async Task GetPolicyById_NonExistingPolicy_ReturnsNotFound()
    {
        // Act
        var response = await HttpClient.GetAsync($"{BaseUrl}/{TestData.NonExistingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    #endregion

    #region Search Policies Tests

    [Fact]
    public async Task SearchPolicies_ValidSearch_ReturnsPagedResult()
    {
        // Arrange
        var policies = await SeedPoliciesListAsync();

        // Act
        var response = await HttpClient.GetAsync($"{BaseUrl}?pageNumber=1&pageSize=20");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<PolicyDto>>();

        Assert.NotNull(result);
        Assert.Equal(policies.Count, result.Items.Count);
        Assert.Equal(policies.Count, result.TotalCount);
        Assert.Equal(1, result.PageNumber);
        Assert.Equal(20, result.PageSize);
        Assert.All(policies, policy => Assert.Contains(result.Items, x => x.PolicyId == policy.PolicyId));
    }

    [Theory]
    [InlineData("clientId")]
    [InlineData("brokerId")]
    [InlineData("status")]
    public async Task SearchPolicies_ValidFilter_ReturnsMatchingPolicies(string filter)
    {
        // Arrange
        var policies = await SeedPoliciesListAsync();

        var expectedPolicy = policies[0];
        var expectedCount = 0;

        string queryString;

        if (filter == "clientId")
        {
            queryString = $"clientId={expectedPolicy.ClientId}";
            expectedCount = 2;
        }
        else if (filter == "brokerId")
        {
            queryString = $"brokerId={expectedPolicy.BrokerId}";
            expectedCount = 2;
        }
        else
        {
            queryString = $"status={expectedPolicy.Status}";
            expectedCount = 2;
        }

        // Act
        var response = await HttpClient.GetAsync($"{BaseUrl}?{queryString}&pageNumber=1&pageSize=20");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<PagedResult<PolicyDto>>();

        Assert.NotNull(result);
        Assert.Equal(expectedCount, result.Items.Count);
        Assert.Equal(expectedCount, result.TotalCount);

        foreach (var policy in policies.Take(expectedCount))
        {
            Assert.Contains(result.Items, x => x.PolicyId == policy.PolicyId);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SearchPolicies_InvalidPageNumber_ReturnsBadRequest(int pageNumber)
    {
        // Act
        var response = await HttpClient.GetAsync($"{BaseUrl}?pageNumber={pageNumber}&pageSize=50");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task SearchPolicies_InvalidPageSize_ReturnsBadRequest(int pageSize)
    {
        // Act
        var response = await HttpClient.GetAsync($"{BaseUrl}?pageNumber=1&pageSize={pageSize}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    #endregion


    #region Activate Policy Tests

    [Fact]
    public async Task ActivatePolicy_ValidDraft_ReturnsOkAndPersistsChanges()
    {
        // Arrange
        var policyToSeed = await SeedPolicyAsync();

        // Act
        var response = await HttpClient.PostAsync($"{BaseUrl}/{policyToSeed.PolicyId}/activate", null);

        // Assert - HTTP
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var policy = await response.Content.ReadFromJsonAsync<PolicyDto>();

        Assert.NotNull(policy);
        Assert.Equal(PolicyStatus.Active, policy.Status);
        Assert.Equal(policyToSeed.PolicyId, policy.PolicyId);

        // Assert - persistence
        var persistedPolicy = await DbContext.Policies.AsNoTracking().FirstAsync(x => x.PolicyId == policyToSeed.PolicyId);

        Assert.Equal(PolicyStatus.Active, persistedPolicy.Status);
        Assert.NotNull(persistedPolicy.ActivationDate);
    }

    [Fact]
    public async Task ActivatePolicy_NonExistingPolicy_ReturnsNotFound()
    {
        // Act
        var response = await HttpClient.PostAsync($"{BaseUrl}/{TestData.NonExistingId}/activate", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
    }

    [Fact]
    public async Task ActivatePolicy_AlreadyActive_ReturnsBadRequest()
    {
        // Arrange
        var policyToSeed = await SeedPolicyAsync(PolicyStatus.Active);

        // Act
        var response = await HttpClient.PostAsync($"{BaseUrl}/{policyToSeed.PolicyId}/activate", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ActivatePolicy_StartDateInPast_ReturnsBadRequest()
    {
        // Arrange
        var policyToSeed = await SeedPolicyAsync();

        policyToSeed.StartDate = DateTime.UtcNow.Date.AddDays(-1);

        await DbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.PostAsync($"{BaseUrl}/{policyToSeed.PolicyId}/activate", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var persistedPolicy = await DbContext.Policies.AsNoTracking().FirstAsync(x => x.PolicyId == policyToSeed.PolicyId);

        Assert.Equal(PolicyStatus.Draft, persistedPolicy.Status);
        Assert.Null(persistedPolicy.ActivationDate);
    }

    [Fact]
    public async Task ActivatePolicy_InactiveBroker_ReturnsBadRequest()
    {
        // Arrange
        var policyToSeed = await SeedPolicyAsync();

        await SeedAsync(TestData.BrokerInactive);

        policyToSeed.BrokerId = TestData.BrokerInactive.BrokerId;

        await DbContext.SaveChangesAsync();

        // Act
        var response = await HttpClient.PostAsync($"{BaseUrl}/{policyToSeed.PolicyId}/activate", null);

        // Assert
        Assert.False(TestData.BrokerInactive.IsActive);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var persistedPolicy = await DbContext.Policies.AsNoTracking().FirstAsync(x => x.PolicyId == policyToSeed.PolicyId);

        Assert.Equal(PolicyStatus.Draft, persistedPolicy.Status);
        Assert.Null(persistedPolicy.ActivationDate);
    }
    #endregion


    #region Cancel Policy Tests

    [Fact]
    public async Task CancelPolicy_ValidActivePolicy_ReturnsOkAndPersistsChanges()
    {
        // Arrange
        var policyToSeed = await SeedPolicyAsync(PolicyStatus.Active);

        var request = TestData.PolicyDtoForCancel;

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseUrl}/{policyToSeed.PolicyId}/cancel", request);

        // Assert - HTTP
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var policy = await response.Content.ReadFromJsonAsync<PolicyDto>();

        Assert.NotNull(policy);
        Assert.Equal(PolicyStatus.Cancelled, policy.Status);
        
        // Assert - persistence
        var persistedPolicy = await DbContext.Policies.AsNoTracking().FirstAsync(x => x.PolicyId == policyToSeed.PolicyId);

        Assert.Equal(PolicyStatus.Cancelled, persistedPolicy.Status);
        Assert.NotNull(persistedPolicy.CancellationDate);
        Assert.Equal(request.CancellationReason, persistedPolicy.CancellationReason);
    }

    [Fact]
    public async Task CancelPolicy_NonExistingPolicy_ReturnsNotFound()
    {
        // Arrange
        var request = TestData.PolicyDtoForCancel;

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseUrl}/{TestData.NonExistingId}/cancel", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
    }

    [Fact]
    public async Task CancelPolicy_DraftPolicy_ReturnsBadRequest()
    {
        // Arrange
        var policyToSeed = await SeedPolicyAsync(PolicyStatus.Draft);
        var request = TestData.PolicyDtoForCancel;

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseUrl}/{policyToSeed.PolicyId}/cancel", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var persistedPolicy = await DbContext.Policies.AsNoTracking().FirstAsync(x => x.PolicyId == policyToSeed.PolicyId);

        Assert.Equal(PolicyStatus.Draft, persistedPolicy.Status);
        Assert.Null(persistedPolicy.CancellationDate);
    }

    [Fact]
    public async Task CancelPolicy_ExpiredPolicy_ReturnsBadRequest()
    {
        // Arrange
        var policyToSeed = await SeedPolicyAsync(PolicyStatus.Active);
        policyToSeed.EndDate = DateTime.UtcNow.Date.AddDays(-1);

        await DbContext.SaveChangesAsync();

        var request = TestData.PolicyDtoForCancel;

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseUrl}/{policyToSeed.PolicyId}/cancel", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var persistedPolicy = await DbContext.Policies.AsNoTracking().FirstAsync(x => x.PolicyId == policyToSeed.PolicyId);

        Assert.Equal(PolicyStatus.Active, persistedPolicy.Status);
        Assert.Null(persistedPolicy.CancellationDate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("checkMaxLength")]
    public async Task CancelPolicy_InvalidCancellationReason_ReturnsBadRequest(string reason)
    {
        // Arrange
        var policyToSeed = await SeedPolicyAsync(PolicyStatus.Active);

        var cancellationReason = reason;

        if (reason == "checkMaxLength")
        {
            cancellationReason = new string('A', 501);
        }

        var request = TestData.PolicyDtoForCancel with
        {
            CancellationReason = cancellationReason
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync($"{BaseUrl}/{policyToSeed.PolicyId}/cancel", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var persistedPolicy = await DbContext.Policies.AsNoTracking().FirstAsync(x => x.PolicyId == policyToSeed.PolicyId);

        Assert.Equal(PolicyStatus.Active, persistedPolicy.Status);
        Assert.Null(persistedPolicy.CancellationDate);
    }
    #endregion
}
