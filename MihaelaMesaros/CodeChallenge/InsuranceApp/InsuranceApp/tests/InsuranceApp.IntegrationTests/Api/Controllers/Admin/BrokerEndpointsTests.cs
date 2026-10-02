using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Broker;
using InsuranceApp.Domain.Constants;
using InsuranceApp.IntegrationTests.Common;
using InsuranceApp.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace InsuranceApp.IntegrationTests.Api.Controllers.Admin;

public sealed class BrokerEndpointsTests(InsuranceAppWebApplicationFactory factory)
    : IntegrationTestBase(factory), IClassFixture<InsuranceAppWebApplicationFactory>
{
    private const string BaseUrl = "/api/admin/brokers";

    #region Create Broker

    [Fact]
    public async Task CreateBroker_ValidRequest_ReturnsCreatedAndPersistsBroker()
    {
        // Arrange
        var broker = TestData.BrokerForCreate;
        var request = new CreateBrokerDto(broker.BrokerCode, broker.Name, broker.Email, broker.Phone, broker.CommissionPercentage, broker.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert - HTTP
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdBroker = await response.Content.ReadFromJsonAsync<BrokerDto>();

        Assert.NotNull(createdBroker);
        Assert.NotEqual(Guid.Empty, createdBroker.BrokerId);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal($"{BaseUrl}/{createdBroker.BrokerId}", response.Headers.Location.AbsolutePath);
        Assert.Equal(request.BrokerCode, createdBroker.BrokerCode);
        Assert.Equal(request.Name, createdBroker.Name);
        Assert.Equal(request.Email, createdBroker.Email);
        Assert.Equal(request.Phone, createdBroker.Phone);
        Assert.Equal(request.CommissionPercentage, createdBroker.CommissionPercentage);
        Assert.Equal(request.IsActive, createdBroker.IsActive);

        // Assert - persistence
        var persistedBroker = await DbContext.Brokers.AsNoTracking().FirstOrDefaultAsync(x => x.BrokerId == createdBroker.BrokerId);

        Assert.NotNull(persistedBroker);
        Assert.Equal(request.BrokerCode, persistedBroker.BrokerCode);
        Assert.Equal(request.Name, persistedBroker.Name);
        Assert.Equal(request.Email, persistedBroker.Email);
        Assert.Equal(request.Phone, persistedBroker.Phone);
        Assert.Equal(request.CommissionPercentage, persistedBroker.CommissionPercentage);
        Assert.Equal(request.IsActive, persistedBroker.IsActive);
    }

    [Fact]
    public async Task CreateBroker_NormalizesBrokerDetails()
    {
        // Arrange
        var broker = TestData.BrokerForCreate;
        var request = new CreateBrokerDto(
            $" {broker.BrokerCode.ToLowerInvariant()} ",
            $" {broker.Name} ",
            broker.Email is null ? null : $" {broker.Email} ",
            broker.Phone is null ? null : $" {broker.Phone} ",
            broker.CommissionPercentage,
            broker.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdBroker = await response.Content.ReadFromJsonAsync<BrokerDto>();

        Assert.NotNull(createdBroker);
        Assert.Equal(request.BrokerCode.Trim().ToUpperInvariant(), createdBroker.BrokerCode);
        Assert.Equal(request.Name.Trim(), createdBroker.Name);
        Assert.Equal(request.Email?.Trim(), createdBroker.Email);
        Assert.Equal(request.Phone?.Trim(), createdBroker.Phone);
    }

    [Fact]
    public async Task CreateBroker_MissingBrokerCode_ReturnsBadRequest()
    {
        // Arrange
        var broker = TestData.BrokerForCreate;
        var request = new CreateBrokerDto("", broker.Name, broker.Email, broker.Phone, broker.CommissionPercentage, broker.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal(BrokerErrors.BrokerCodeRequired.Code, problem.Extensions["code"]?.ToString());
    }

    [Fact]
    public async Task CreateBroker_BrokerCodeBelowMinimumLength_ReturnsBadRequest()
    {
        // Arrange
        var broker = TestData.BrokerForCreate;
        var brokerCode = new string('B', BrokerConstraints.BrokerCodeMinLength - 1);
        var request = new CreateBrokerDto(brokerCode, broker.Name, broker.Email, broker.Phone, broker.CommissionPercentage, broker.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateBroker_BrokerCodeAboveMaximumLength_ReturnsBadRequest()
    {
        // Arrange
        var broker = TestData.BrokerForCreate;
        var brokerCode = new string('B', BrokerConstraints.BrokerCodeMaxLength + 1);
        var request = new CreateBrokerDto(brokerCode, broker.Name, broker.Email, broker.Phone, broker.CommissionPercentage, broker.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateBroker_MissingName_ReturnsBadRequest()
    {
        // Arrange
        var broker = TestData.BrokerForCreate;
        var request = new CreateBrokerDto(broker.BrokerCode, "", broker.Email, broker.Phone, broker.CommissionPercentage, broker.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(BrokerErrors.NameRequired.Code, problem.Extensions["code"]?.ToString());
    }

    [Fact]
    public async Task CreateBroker_NameBelowMinimumLength_ReturnsBadRequest()
    {
        // Arrange
        var broker = TestData.BrokerForCreate;
        var name = new string('A', BrokerConstraints.NameMinLength - 1);
        var request = new CreateBrokerDto(broker.BrokerCode, name, broker.Email, broker.Phone, broker.CommissionPercentage, broker.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateBroker_NameAboveMaximumLength_ReturnsBadRequest()
    {
        // Arrange
        var broker = TestData.BrokerForCreate;
        var name = new string('A', BrokerConstraints.NameMaxLength + 1);
        var request = new CreateBrokerDto(broker.BrokerCode, name, broker.Email, broker.Phone, broker.CommissionPercentage, broker.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData("@test.com")]
    public async Task CreateBroker_InvalidEmail_ReturnsBadRequest(string email)
    {
        // Arrange
        var broker = TestData.BrokerForCreate;
        var request = new CreateBrokerDto(broker.BrokerCode, broker.Name, email, broker.Phone, broker.CommissionPercentage, broker.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateBroker_EmailAboveMaximumLength_ReturnsBadRequest()
    {
        // Arrange
        var broker = TestData.BrokerForCreate;
        var email = $"{new string('a', BrokerConstraints.EmailMaxLength - "@test.com".Length + 1)}@test.com";
        var request = new CreateBrokerDto(broker.BrokerCode, broker.Name, email, broker.Phone, broker.CommissionPercentage, broker.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateBroker_PhoneAboveMaximumLength_ReturnsBadRequest()
    {
        // Arrange
        var broker = TestData.BrokerForCreate;
        var phone = new string('1', BrokerConstraints.PhoneMaxLength + 1);
        var request = new CreateBrokerDto(broker.BrokerCode, broker.Name, broker.Email, phone, broker.CommissionPercentage, broker.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateBroker_CommissionPercentageAboveMaximum_ReturnsBadRequest()
    {
        // Arrange
        var broker = TestData.BrokerForCreate;
        var commissionPercentage = BrokerConstraints.MaxCommissionPercentage + 0.01m;
        var request = new CreateBrokerDto(broker.BrokerCode, broker.Name, broker.Email, broker.Phone, commissionPercentage, broker.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateBroker_CommissionPercentageBelowMinimum_ReturnsBadRequest()
    {
        // Arrange
        var broker = TestData.BrokerForCreate;
        var commissionPercentage = BrokerConstraints.MinCommissionPercentage - 0.01m;
        var request = new CreateBrokerDto(broker.BrokerCode, broker.Name, broker.Email, broker.Phone, commissionPercentage, broker.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateBroker_CommissionPercentageWithInvalidScale_ReturnsBadRequest()
    {
        // Arrange
        var broker = TestData.BrokerForCreate;
        var commissionPercentage = 5.123m;
        var request = new CreateBrokerDto(broker.BrokerCode, broker.Name, broker.Email, broker.Phone, commissionPercentage, broker.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(null, "0712345678")]
    [InlineData("john.broker@test.com", null)]
    [InlineData(null, null)]
    public async Task CreateBroker_OptionalContactInfo_ReturnsCreated(string? email, string? phone)
    {
        // Arrange
        var broker = TestData.BrokerForCreate;
        var request = new CreateBrokerDto(broker.BrokerCode, broker.Name, email, phone, broker.CommissionPercentage, broker.IsActive);

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdBroker = await response.Content.ReadFromJsonAsync<BrokerDto>();

        Assert.NotNull(createdBroker);
        Assert.Equal(email, createdBroker.Email);
        Assert.Equal(phone, createdBroker.Phone);
    }

    [Fact]
    public async Task CreateBroker_DuplicateBrokerCode_ReturnsConflict()
    {
        // Arrange
        var broker = TestData.BrokerForCreate;
        var request = new CreateBrokerDto(broker.BrokerCode, broker.Name, broker.Email, broker.Phone, broker.CommissionPercentage, broker.IsActive);

        var firstResponse = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act
        var secondResponse = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

        var problem = await secondResponse.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status409Conflict, problem.Status);
        Assert.Equal(BrokerErrors.DuplicateBrokerCode(request.BrokerCode).Code, problem.Extensions["code"]?.ToString());

        var count = await DbContext.Brokers.CountAsync(x => x.BrokerCode == request.BrokerCode);

        Assert.Equal(1, count);
    }

    #endregion

    #region Update Broker

    [Fact]
    public async Task UpdateBroker_ValidRequest_ReturnsOkAndPersistsChanges()
    {
        // Arrange
        var brokerToUpdate = TestData.BrokerForUpdate;
        await SeedAsync(brokerToUpdate);

        var request = new UpdateBrokerDto($"{brokerToUpdate.BrokerCode}U", $"{brokerToUpdate.Name} Updated", "updated.broker@test.com", "0733333333", 6.25m);

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{BaseUrl}/{brokerToUpdate.BrokerId}", request);

        // Assert - HTTP
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedBroker = await response.Content.ReadFromJsonAsync<BrokerDto>();

        Assert.NotNull(updatedBroker);
        Assert.Equal(brokerToUpdate.BrokerId, updatedBroker.BrokerId);
        Assert.Equal(request.BrokerCode, updatedBroker.BrokerCode);
        Assert.Equal(request.Name, updatedBroker.Name);
        Assert.Equal(request.Email, updatedBroker.Email);
        Assert.Equal(request.Phone, updatedBroker.Phone);
        Assert.Equal(request.CommissionPercentage, updatedBroker.CommissionPercentage);
        Assert.Equal(brokerToUpdate.IsActive, updatedBroker.IsActive);

        // Assert - persistence
        var persistedBroker = await DbContext.Brokers.AsNoTracking().FirstAsync(x => x.BrokerId == brokerToUpdate.BrokerId);

        Assert.Equal(request.BrokerCode, persistedBroker.BrokerCode);
        Assert.Equal(request.Name, persistedBroker.Name);
        Assert.Equal(request.Email, persistedBroker.Email);
        Assert.Equal(request.Phone, persistedBroker.Phone);
        Assert.Equal(request.CommissionPercentage, persistedBroker.CommissionPercentage);
        Assert.Equal(brokerToUpdate.IsActive, persistedBroker.IsActive);
        Assert.NotNull(persistedBroker.ModifiedAt);
    }

    [Fact]
    public async Task UpdateBroker_NonExistingBroker_ReturnsNotFound()
    {
        // Arrange
        var broker = TestData.BrokerForUpdate;
        var request = new UpdateBrokerDto(broker.BrokerCode, broker.Name, broker.Email, broker.Phone, broker.CommissionPercentage);

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{BaseUrl}/{TestData.NonExistingId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBroker_EmptyBrokerId_ReturnsBadRequest()
    {
        // Arrange
        var broker = TestData.BrokerForUpdate;
        var request = new UpdateBrokerDto(broker.BrokerCode, broker.Name, broker.Email, broker.Phone, broker.CommissionPercentage);

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{BaseUrl}/{Guid.Empty}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateBroker_DuplicateBrokerCode_ReturnsConflict()
    {
        // Arrange
        await SeedAsync(TestData.BrokersList);

        var brokerToUpdate = TestData.BrokersList[0];
        var brokerWithDuplicateCode = TestData.BrokersList[1];

        var request = new UpdateBrokerDto(brokerWithDuplicateCode.BrokerCode, brokerToUpdate.Name, brokerToUpdate.Email, brokerToUpdate.Phone, brokerToUpdate.CommissionPercentage);

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{BaseUrl}/{brokerToUpdate.BrokerId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status409Conflict, problem.Status);
        Assert.Equal(BrokerErrors.DuplicateBrokerCode(request.BrokerCode).Code, problem.Extensions["code"]?.ToString());
    }

    [Fact]
    public async Task UpdateBroker_SameBrokerCode_ReturnsOk()
    {
        // Arrange
        var broker = TestData.BrokerForUpdate;
        await SeedAsync(broker);

        var request = new UpdateBrokerDto(broker.BrokerCode, $"{broker.Name} Updated", broker.Email, broker.Phone, broker.CommissionPercentage);

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{BaseUrl}/{broker.BrokerId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData(null, "0712345678")]
    [InlineData("john.updated@test.com", null)]
    [InlineData(null, null)]
    public async Task UpdateBroker_OptionalContactInfo_ReturnsOk(string? email, string? phone)
    {
        // Arrange
        var broker = TestData.BrokerForUpdate;
        await SeedAsync(broker);

        var request = new UpdateBrokerDto(broker.BrokerCode, broker.Name, email, phone, broker.CommissionPercentage);

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{BaseUrl}/{broker.BrokerId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedBroker = await response.Content.ReadFromJsonAsync<BrokerDto>();

        Assert.NotNull(updatedBroker);
        Assert.Equal(email, updatedBroker.Email);
        Assert.Equal(phone, updatedBroker.Phone);
    }

    #endregion

    #region Read Broker

    [Fact]
    public async Task GetBrokers_ReturnsOkWithBrokers()
    {
        // Arrange
        await SeedAsync(TestData.BrokersList);

        // Act
        var response = await HttpClient.GetAsync(BaseUrl);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var brokers = await response.Content.ReadFromJsonAsync<List<BrokerDto>>();

        Assert.NotNull(brokers);
        Assert.Equal(TestData.BrokersList.Count, brokers.Count);

        foreach (var expectedBroker in TestData.BrokersList)
        {
            Assert.Contains(
                brokers,
                broker =>
                    broker.BrokerId == expectedBroker.BrokerId &&
                    broker.BrokerCode == expectedBroker.BrokerCode &&
                    broker.Name == expectedBroker.Name &&
                    broker.Email == expectedBroker.Email &&
                    broker.Phone == expectedBroker.Phone &&
                    broker.CommissionPercentage == expectedBroker.CommissionPercentage &&
                    broker.IsActive == expectedBroker.IsActive);
        }
    }

    [Fact]
    public async Task GetBrokerById_ExistingBroker_ReturnsOk()
    {
        // Arrange
        var brokerToSeed = TestData.BrokersList[0];
        await SeedAsync(brokerToSeed);

        // Act
        var response = await HttpClient.GetAsync($"{BaseUrl}/{brokerToSeed.BrokerId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var broker = await response.Content.ReadFromJsonAsync<BrokerDto>();

        Assert.NotNull(broker);
        Assert.Equal(brokerToSeed.BrokerId, broker.BrokerId);
        Assert.Equal(brokerToSeed.BrokerCode, broker.BrokerCode);
        Assert.Equal(brokerToSeed.Name, broker.Name);
        Assert.Equal(brokerToSeed.Email, broker.Email);
        Assert.Equal(brokerToSeed.Phone, broker.Phone);
        Assert.Equal(brokerToSeed.CommissionPercentage, broker.CommissionPercentage);
        Assert.Equal(brokerToSeed.IsActive, broker.IsActive);
    }

    [Fact]
    public async Task GetBrokerById_NonExistingBroker_ReturnsNotFound()
    {
        // Act
        var response = await HttpClient.GetAsync($"{BaseUrl}/{TestData.NonExistingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status404NotFound, problem.Status);
    }

    [Fact]
    public async Task GetBrokerById_EmptyBrokerId_ReturnsBadRequest()
    {
        // Act
        var response = await HttpClient.GetAsync($"{BaseUrl}/{Guid.Empty}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Activate Broker

    [Fact]
    public async Task ActivateBroker_InactiveBroker_ReturnsOkAndPersistsChanges()
    {
        // Arrange
        var broker = TestData.BrokersList[1];
        await SeedAsync(broker);

        // Act
        var response = await HttpClient.PostAsync($"{BaseUrl}/{broker.BrokerId}/activate", null);

        // Assert - HTTP
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var activatedBroker = await response.Content.ReadFromJsonAsync<BrokerDto>();

        Assert.NotNull(activatedBroker);
        Assert.Equal(broker.BrokerId, activatedBroker.BrokerId);
        Assert.True(activatedBroker.IsActive);

        // Assert - persistence
        var persistedBroker = await DbContext.Brokers.AsNoTracking().FirstAsync(x => x.BrokerId == broker.BrokerId);

        Assert.True(persistedBroker.IsActive);
        Assert.NotNull(persistedBroker.ModifiedAt);
    }

    [Fact]
    public async Task ActivateBroker_NonExistingBroker_ReturnsNotFound()
    {
        // Act
        var response = await HttpClient.PostAsync($"{BaseUrl}/{TestData.NonExistingId}/activate", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ActivateBroker_EmptyBrokerId_ReturnsBadRequest()
    {
        // Act
        var response = await HttpClient.PostAsync($"{BaseUrl}/{Guid.Empty}/activate", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Deactivate Broker

    [Fact]
    public async Task DeactivateBroker_ActiveBroker_ReturnsOkAndPersistsChanges()
    {
        // Arrange
        var broker = TestData.BrokersList[0];
        await SeedAsync(broker);

        // Act
        var response = await HttpClient.PostAsync($"{BaseUrl}/{broker.BrokerId}/deactivate", null);

        // Assert - HTTP
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var deactivatedBroker = await response.Content.ReadFromJsonAsync<BrokerDto>();

        Assert.NotNull(deactivatedBroker);
        Assert.Equal(broker.BrokerId, deactivatedBroker.BrokerId);
        Assert.False(deactivatedBroker.IsActive);

        // Assert - persistence
        var persistedBroker = await DbContext.Brokers.AsNoTracking().FirstAsync(x => x.BrokerId == broker.BrokerId);

        Assert.False(persistedBroker.IsActive);
        Assert.NotNull(persistedBroker.ModifiedAt);
    }

    [Fact]
    public async Task DeactivateBroker_NonExistingBroker_ReturnsNotFound()
    {
        // Act
        var response = await HttpClient.PostAsync($"{BaseUrl}/{TestData.NonExistingId}/deactivate", null);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeactivateBroker_EmptyBrokerId_ReturnsBadRequest()
    {
        // Act
        var response = await HttpClient.PostAsync($"{BaseUrl}/{Guid.Empty}/deactivate", null);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion
}
