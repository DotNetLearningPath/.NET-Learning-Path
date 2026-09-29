using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.FeeConfig;
using InsuranceApp.Domain.Enums;
using InsuranceApp.IntegrationTests.Common;
using InsuranceApp.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Http.Json;

namespace InsuranceApp.IntegrationTests.Api.Controllers.Admin;

public sealed class FeeConfigEndpointsTests(InsuranceAppWebApplicationFactory factory)
    : IntegrationTestBase(factory), IClassFixture<InsuranceAppWebApplicationFactory>
{
    #region Create Fee Config Tests

    [Fact]
    public async Task CreateFeeConfig_ValidRequest_ReturnsCreatedAndPersistsFeeConfig()
    {
        // Arrange
        var request = TestData.FeeConfigDtoForCreate;

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/admin/fees", request);

        // Assert - HTTP
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdFee = await response.Content.ReadFromJsonAsync<FeeConfigDto>();

        Assert.NotNull(createdFee);
        Assert.NotEqual(Guid.Empty, createdFee.FeeConfigId);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal($"/api/admin/fees/{createdFee.FeeConfigId}", response.Headers.Location.AbsolutePath);
        Assert.Equal(request.Name, createdFee.Name);
        Assert.Equal(request.FeeType, createdFee.FeeType);
        Assert.Equal(request.Percentage, createdFee.Percentage);
        Assert.Equal(request.EffectiveFrom, createdFee.EffectiveFrom);
        Assert.Equal(request.EffectiveTo, createdFee.EffectiveTo);
        Assert.Equal(request.IsActive, createdFee.IsActive);

        // Assert - persistence
        var persistedFee = await DbContext.FeeConfigs.AsNoTracking().FirstOrDefaultAsync(x => x.FeeConfigId == createdFee.FeeConfigId);

        Assert.NotNull(persistedFee);
        Assert.Equal(request.Name, persistedFee.Name);
        Assert.Equal(request.FeeType, persistedFee.FeeType);
        Assert.Equal(request.Percentage, persistedFee.Percentage);
        Assert.Equal(request.EffectiveFrom, persistedFee.EffectiveFrom);
        Assert.Equal(request.EffectiveTo, persistedFee.EffectiveTo);
        Assert.Equal(request.IsActive, persistedFee.IsActive);
    }

    [Fact]
    public async Task CreateFeeConfig_NormalizesName()
    {
        // Arrange
        var request = TestData.FeeConfigDtoForCreate with
        {
            Name = $"  {TestData.FeeConfigDtoForCreate.Name}  "
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/admin/fees", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdFee = await response.Content.ReadFromJsonAsync<FeeConfigDto>();

        Assert.NotNull(createdFee);
        Assert.Equal(request.Name.Trim(), createdFee.Name);
    }

    [Fact]
    public async Task CreateFeeConfig_MissingName_ReturnsBadRequest()
    {
        // Arrange
        var request = TestData.FeeConfigDtoForCreate with
        {
            Name = ""
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/admin/fees", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(StatusCodes.Status400BadRequest, problem.Status);
        Assert.Equal(FeeConfigErrors.NameRequired.Code, problem.Extensions["code"]?.ToString());
    }

    [Fact]
    public async Task CreateFeeConfig_InvalidFeeType_ReturnsBadRequest()
    {
        // Arrange
        var request = TestData.FeeConfigDtoForCreate with
        {
            FeeType = (FeeType)999
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/admin/fees", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateFeeConfig_InvalidPercentage_ReturnsBadRequest()
    {
        // Arrange
        var request = TestData.FeeConfigDtoForCreate with
        {
            Percentage = 101m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/admin/fees", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateFeeConfig_PercentageWithTooManyDecimals_ReturnsBadRequest()
    {
        // Arrange
        var request = TestData.FeeConfigDtoForCreate with
        {
            Percentage = 2.12345m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/admin/fees", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateFeeConfig_InvalidEffectivePeriod_ReturnsBadRequest()
    {
        // Arrange
        var request = TestData.FeeConfigDtoForCreate with
        {
            EffectiveFrom = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            EffectiveTo = new DateTime(2026, 5, 31, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync("/api/admin/fees", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(FeeConfigErrors.InvalidEffectivePeriod.Code, problem.Extensions["code"]?.ToString());
    }

    #endregion

    #region Read Fee Config Tests

    [Fact]
    public async Task GetFees_ReturnsOkWithFeeConfigs()
    {
        // Arrange
        await SeedAsync(TestData.FeeConfigsList);

        // Act
        var response = await HttpClient.GetAsync("/api/admin/fees");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var fees = await response.Content.ReadFromJsonAsync<List<FeeConfigDto>>();

        Assert.NotNull(fees);
        Assert.Equal(TestData.FeeConfigsList.Count, fees.Count);

        foreach (var expectedFee in TestData.FeeConfigsList)
        {
            Assert.Contains(
                fees,
                fee =>
                    fee.FeeConfigId == expectedFee.FeeConfigId &&
                    fee.Name == expectedFee.Name &&
                    fee.FeeType == expectedFee.FeeType &&
                    fee.Percentage == expectedFee.Percentage &&
                    fee.EffectiveFrom == expectedFee.EffectiveFrom &&
                    fee.EffectiveTo == expectedFee.EffectiveTo &&
                    fee.IsActive == expectedFee.IsActive);
        }
    }

    [Fact]
    public async Task GetFeeById_ExistingFeeConfig_ReturnsOk()
    {
        // Arrange
        var feeConfig = TestData.FeeConfigsList[0];
        await SeedAsync(feeConfig);

        // Act
        var response = await HttpClient.GetAsync($"/api/admin/fees/{feeConfig.FeeConfigId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var fee = await response.Content.ReadFromJsonAsync<FeeConfigDto>();

        Assert.NotNull(fee);
        Assert.Equal(feeConfig.FeeConfigId, fee.FeeConfigId);
        Assert.Equal(feeConfig.Name, fee.Name);
        Assert.Equal(feeConfig.FeeType, fee.FeeType);
        Assert.Equal(feeConfig.Percentage, fee.Percentage);
        Assert.Equal(feeConfig.EffectiveFrom, fee.EffectiveFrom);
        Assert.Equal(feeConfig.EffectiveTo, fee.EffectiveTo);
        Assert.Equal(feeConfig.IsActive, fee.IsActive);
    }

    [Fact]
    public async Task GetFeeById_NonExistingFeeConfig_ReturnsNotFound()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/admin/fees/{TestData.NonExistingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetFeeById_EmptyFeeConfigId_ReturnsBadRequest()
    {
        // Act
        var response = await HttpClient.GetAsync($"/api/admin/fees/{Guid.Empty}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Update Fee Config Tests

    [Fact]
    public async Task UpdateFeeConfig_ValidRequest_ReturnsOkAndPersistsChanges()
    {
        // Arrange
        var feeConfig = TestData.FeeConfigsList[0];
        await SeedAsync(feeConfig);

        var request = TestData.FeeConfigDtoForUpdate with
        {
            Name = "Updated broker fee",
            Percentage = 5.5000m,
            IsActive = false
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/admin/fees/{feeConfig.FeeConfigId}", request);

        // Assert - HTTP
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedFee = await response.Content.ReadFromJsonAsync<FeeConfigDto>();

        Assert.NotNull(updatedFee);
        Assert.Equal(feeConfig.FeeConfigId, updatedFee.FeeConfigId);
        Assert.Equal(request.Name, updatedFee.Name);
        Assert.Equal(request.FeeType, updatedFee.FeeType);
        Assert.Equal(request.Percentage, updatedFee.Percentage);
        Assert.Equal(request.EffectiveFrom, updatedFee.EffectiveFrom);
        Assert.Equal(request.EffectiveTo, updatedFee.EffectiveTo);
        Assert.Equal(request.IsActive, updatedFee.IsActive);

        // Assert - persistence
        var persistedFee = await DbContext.FeeConfigs.AsNoTracking().FirstAsync(x => x.FeeConfigId == feeConfig.FeeConfigId);

        Assert.Equal(request.Name, persistedFee.Name);
        Assert.Equal(request.FeeType, persistedFee.FeeType);
        Assert.Equal(request.Percentage, persistedFee.Percentage);
        Assert.Equal(request.EffectiveFrom, persistedFee.EffectiveFrom);
        Assert.Equal(request.EffectiveTo, persistedFee.EffectiveTo);
        Assert.Equal(request.IsActive, persistedFee.IsActive);
        Assert.NotNull(persistedFee.ModifiedAt);
    }

    [Fact]
    public async Task UpdateFeeConfig_NonExistingFeeConfig_ReturnsNotFound()
    {
        // Arrange
        var request = TestData.FeeConfigDtoForUpdate;

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/admin/fees/{TestData.NonExistingId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateFeeConfig_EmptyFeeConfigId_ReturnsBadRequest()
    {
        // Arrange
        var request = TestData.FeeConfigDtoForUpdate;

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/admin/fees/{Guid.Empty}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateFeeConfig_InvalidEffectivePeriod_ReturnsBadRequest()
    {
        // Arrange
        var feeConfig = TestData.FeeConfigsList[0];
        await SeedAsync(feeConfig);

        var request = TestData.FeeConfigDtoForUpdate with
        {
            EffectiveFrom = new DateTime(2026, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            EffectiveTo = new DateTime(2026, 5, 31, 0, 0, 0, DateTimeKind.Utc)
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"/api/admin/fees/{feeConfig.FeeConfigId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion
}
