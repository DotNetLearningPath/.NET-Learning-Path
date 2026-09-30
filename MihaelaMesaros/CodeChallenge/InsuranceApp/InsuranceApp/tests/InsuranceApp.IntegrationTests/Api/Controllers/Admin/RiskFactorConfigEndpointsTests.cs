using System.Net;
using System.Net.Http.Json;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.RiskFactorConfig;
using InsuranceApp.Domain.Enums;
using InsuranceApp.IntegrationTests.Common;
using InsuranceApp.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.IntegrationTests.Api.Controllers.Admin;

public sealed class RiskFactorConfigEndpointsTests(InsuranceAppWebApplicationFactory factory)
    : IntegrationTestBase(factory), IClassFixture<InsuranceAppWebApplicationFactory>
{
    private const string BaseUrl = "/api/admin/risk-factors";

    #region Create tests

    [Fact]
    public async Task CreateRiskFactorConfig_ValidRequest_ReturnsCreatedAndPersistsConfig()
    {
        // Arrange
        await SeedAsync(TestData.Countries);

        var request = TestData.RiskFactorConfigDtoForCreate;

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert - HTTP
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdConfig = await response.Content.ReadFromJsonAsync<RiskFactorConfigDto>();

        Assert.NotNull(createdConfig);
        Assert.NotEqual(Guid.Empty, createdConfig.RiskFactorConfigId);
        Assert.Equal(request.Level, createdConfig.Level);
        Assert.Equal(request.ReferenceId, createdConfig.ReferenceId);
        Assert.Equal(request.AdjustmentPercentage, createdConfig.AdjustmentPercentage);
        Assert.Equal(request.IsActive, createdConfig.IsActive);
        Assert.NotNull(response.Headers.Location);
        Assert.Equal($"{BaseUrl}/{createdConfig.RiskFactorConfigId}", response.Headers.Location.AbsolutePath);

        // Assert - persistence
        var persistedConfig = await DbContext.RiskFactorConfigs.AsNoTracking().FirstOrDefaultAsync(x => x.RiskFactorConfigId == createdConfig.RiskFactorConfigId);

        Assert.NotNull(persistedConfig);
        Assert.Equal(request.Level, persistedConfig.Level);
        Assert.Equal(request.ReferenceId, persistedConfig.ReferenceId);
        Assert.Equal(request.AdjustmentPercentage, persistedConfig.AdjustmentPercentage);
        Assert.Equal(request.IsActive, persistedConfig.IsActive);
    }

    [Fact]
    public async Task CreateRiskFactorConfig_NegativePercentage_ReturnsCreated()
    {
        // Arrange
        await SeedAsync(TestData.Countries);

        var request = TestData.RiskFactorConfigDtoForCreate with
        {
            AdjustmentPercentage = -5.25m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdConfig = await response.Content.ReadFromJsonAsync<RiskFactorConfigDto>();

        Assert.NotNull(createdConfig);
        Assert.Equal(request.AdjustmentPercentage, createdConfig.AdjustmentPercentage);
    }

    [Fact]
    public async Task CreateRiskFactorConfig_InvalidLevel_ReturnsBadRequest()
    {
        // Arrange
        var request = TestData.RiskFactorConfigDtoForCreate with
        {
            Level = (RiskFactorLevel)999
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateRiskFactorConfig_EmptyReferenceId_ReturnsBadRequest()
    {
        // Arrange
        var request = TestData.RiskFactorConfigDtoForCreate with
        {
            ReferenceId = Guid.Empty
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(RiskFactorConfigErrors.InvalidReferenceId.Code, problem.Extensions["code"]?.ToString());
    }

    [Theory]
    [InlineData(RiskFactorLevel.Country)]
    [InlineData(RiskFactorLevel.County)]
    [InlineData(RiskFactorLevel.City)]
    [InlineData(RiskFactorLevel.BuildingType)]
    public async Task CreateRiskFactorConfig_NonExistingReference_ReturnsNotFound(RiskFactorLevel level)
    {
        // Arrange
        var request = TestData.RiskFactorConfigDtoForCreate with
        {
            Level = level,
            ReferenceId = TestData.NonExistingId
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(RiskFactorConfigErrors.ReferenceNotFound.Code, problem.Extensions["code"]?.ToString());
    }

    [Fact]
    public async Task CreateRiskFactorConfig_PercentageAboveMaximum_ReturnsBadRequest()
    {
        // Arrange
        var request = TestData.RiskFactorConfigDtoForCreate with
        {
            AdjustmentPercentage = 100.01m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateRiskFactorConfig_PercentageBelowMinimum_ReturnsBadRequest()
    {
        // Arrange
        var request = TestData.RiskFactorConfigDtoForCreate with
        {
            AdjustmentPercentage = -100.01m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateRiskFactorConfig_PercentageWithTooManyDecimals_ReturnsBadRequest()
    {
        // Arrange
        var request = TestData.RiskFactorConfigDtoForCreate with
        {
            AdjustmentPercentage = 5.123m
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);
        Assert.Equal(RiskFactorConfigErrors.InvalidAdjustmentPercentageScale.Code, problem.Extensions["code"]?.ToString());
    }

    [Fact]
    public async Task CreateRiskFactorConfig_DuplicateLevelAndReference_ReturnsConflict()
    {
        // Arrange
        await SeedAsync(TestData.Countries);

        var request = TestData.RiskFactorConfigDtoForCreate;

        var firstResponse = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        // Act
        var secondResponse = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

        var count = await DbContext.RiskFactorConfigs.CountAsync(x => x.Level == request.Level && x.ReferenceId == request.ReferenceId);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task CreateRiskFactorConfig_ConcurrentDuplicate_OnlyOneIsCreated()
    {
        // Arrange
        await SeedAsync(TestData.Countries);

        var request = TestData.RiskFactorConfigDtoForCreate;

        // Act
        var task1 = HttpClient.PostAsJsonAsync(BaseUrl, request);
        var task2 = HttpClient.PostAsJsonAsync(BaseUrl, request);

        var responses = await Task.WhenAll(task1, task2);

        // Assert
        Assert.Contains(responses, x => x.StatusCode == HttpStatusCode.Created);
        Assert.Contains(responses, x => x.StatusCode == HttpStatusCode.Conflict);

        var count = await DbContext.RiskFactorConfigs.CountAsync(x => x.Level == request.Level && x.ReferenceId == request.ReferenceId);

        Assert.Equal(1, count);
    }

    #endregion

    #region Read tests

    [Fact]
    public async Task GetRiskFactorConfigs_ReturnsOkWithConfigs()
    {
        // Arrange
        await SeedAsync(TestData.RiskFactorConfigsList);

        // Act
        var response = await HttpClient.GetAsync(BaseUrl);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var configs = await response.Content.ReadFromJsonAsync<List<RiskFactorConfigDto>>();

        Assert.NotNull(configs);
        Assert.Equal(TestData.RiskFactorConfigsList.Count, configs.Count);

        foreach (var expectedConfig in TestData.RiskFactorConfigsList)
        {
            Assert.Contains(configs, config =>
                config.RiskFactorConfigId == expectedConfig.RiskFactorConfigId &&
                config.Level == expectedConfig.Level &&
                config.ReferenceId == expectedConfig.ReferenceId &&
                config.AdjustmentPercentage == expectedConfig.AdjustmentPercentage &&
                config.IsActive == expectedConfig.IsActive);
        }
    }

    [Fact]
    public async Task GetRiskFactorConfigById_ExistingConfig_ReturnsOk()
    {
        // Arrange
        var riskFactorConfig = TestData.RiskFactorConfigsList[0];
        await SeedAsync(riskFactorConfig);

        // Act
        var response = await HttpClient.GetAsync($"{BaseUrl}/{riskFactorConfig.RiskFactorConfigId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var config = await response.Content.ReadFromJsonAsync<RiskFactorConfigDto>();

        Assert.NotNull(config);
        Assert.Equal(riskFactorConfig.RiskFactorConfigId, config.RiskFactorConfigId);
        Assert.Equal(riskFactorConfig.Level, config.Level);
        Assert.Equal(riskFactorConfig.ReferenceId, config.ReferenceId);
        Assert.Equal(riskFactorConfig.AdjustmentPercentage, config.AdjustmentPercentage);
        Assert.Equal(riskFactorConfig.IsActive, config.IsActive);
    }

    [Fact]
    public async Task GetRiskFactorConfigById_NonExistingConfig_ReturnsNotFound()
    {
        // Act
        var response = await HttpClient.GetAsync($"{BaseUrl}/{TestData.NonExistingId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetRiskFactorConfigById_EmptyId_ReturnsBadRequest()
    {
        // Act
        var response = await HttpClient.GetAsync($"{BaseUrl}/{Guid.Empty}");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion

    #region Update tests

    [Fact]
    public async Task UpdateRiskFactorConfig_ValidRequest_ReturnsOkAndPersistsChanges()
    {
        // Arrange
        await SeedAsync(TestData.Countries);

        var riskFactorConfig = TestData.RiskFactorConfigsList[0];
        await SeedAsync(riskFactorConfig);

        var request = TestData.RiskFactorConfigDtoForUpdate;

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{BaseUrl}/{riskFactorConfig.RiskFactorConfigId}", request);

        // Assert - HTTP
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedConfig = await response.Content.ReadFromJsonAsync<RiskFactorConfigDto>();

        Assert.NotNull(updatedConfig);
        Assert.Equal(riskFactorConfig.RiskFactorConfigId, updatedConfig.RiskFactorConfigId);
        Assert.Equal(request.Level, updatedConfig.Level);
        Assert.Equal(request.ReferenceId, updatedConfig.ReferenceId);
        Assert.Equal(request.AdjustmentPercentage, updatedConfig.AdjustmentPercentage);
        Assert.Equal(request.IsActive, updatedConfig.IsActive);

        // Assert - persistence
        var persistedConfig = await DbContext.RiskFactorConfigs.AsNoTracking().FirstAsync(x => x.RiskFactorConfigId == riskFactorConfig.RiskFactorConfigId);

        Assert.Equal(request.Level, persistedConfig.Level);
        Assert.Equal(request.ReferenceId, persistedConfig.ReferenceId);
        Assert.Equal(request.AdjustmentPercentage, persistedConfig.AdjustmentPercentage);
        Assert.Equal(request.IsActive, persistedConfig.IsActive);
        Assert.NotNull(persistedConfig.ModifiedAt);
    }

    [Fact]
    public async Task UpdateRiskFactorConfig_NonExistingConfig_ReturnsNotFound()
    {
        // Arrange
        var request = TestData.RiskFactorConfigDtoForUpdate;

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{BaseUrl}/{TestData.NonExistingId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRiskFactorConfig_EmptyId_ReturnsBadRequest()
    {
        // Arrange
        var request = TestData.RiskFactorConfigDtoForUpdate;

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{BaseUrl}/{Guid.Empty}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRiskFactorConfig_NonExistingReference_ReturnsNotFound()
    {
        // Arrange
        var riskFactorConfig = TestData.RiskFactorConfigsList[0];
        await SeedAsync(riskFactorConfig);

        var request = TestData.RiskFactorConfigDtoForUpdate with
        {
            ReferenceId = TestData.NonExistingId
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{BaseUrl}/{riskFactorConfig.RiskFactorConfigId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRiskFactorConfig_DuplicateLevelAndReference_ReturnsConflict()
    {
        // Arrange
        await SeedAsync(TestData.Countries);
        await SeedAsync(TestData.RiskFactorConfigsList);

        var firstConfig = TestData.RiskFactorConfigsList[0];
        var secondConfig = TestData.RiskFactorConfigsList[1];

        var request = TestData.RiskFactorConfigDtoForUpdate with
        {
            Level = firstConfig.Level,
            ReferenceId = firstConfig.ReferenceId
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{BaseUrl}/{secondConfig.RiskFactorConfigId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRiskFactorConfig_InvalidLevel_ReturnsBadRequest()
    {
        // Arrange
        var riskFactorConfig = TestData.RiskFactorConfigsList[0];
        await SeedAsync(riskFactorConfig);

        var request = TestData.RiskFactorConfigDtoForUpdate with
        {
            Level = (RiskFactorLevel)999
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{BaseUrl}/{riskFactorConfig.RiskFactorConfigId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateRiskFactorConfig_PercentageWithTooManyDecimals_ReturnsBadRequest()
    {
        // Arrange
        var riskFactorConfig = TestData.RiskFactorConfigsList[0];
        await SeedAsync(riskFactorConfig);

        var request = TestData.RiskFactorConfigDtoForUpdate with
        {
            AdjustmentPercentage = 5.123m
        };

        // Act
        var response = await HttpClient.PutAsJsonAsync($"{BaseUrl}/{riskFactorConfig.RiskFactorConfigId}", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion


    #region Reference level tests

    [Fact]
    public async Task CreateRiskFactorConfig_ExistingCountryReference_ReturnsCreated()
    {
        // Arrange
        await SeedAsync(TestData.Countries);

        var request = TestData.RiskFactorConfigDtoForCreate with
        {
            Level = RiskFactorLevel.Country,
            ReferenceId = TestData.Countries[0].CountryId
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateRiskFactorConfig_ExistingCountyReference_ReturnsCreated()
    {
        // Arrange
        await SeedAsync(TestData.Countries);
        await SeedAsync(TestData.Counties);

        var request = TestData.RiskFactorConfigDtoForCreate with
        {
            Level = RiskFactorLevel.County,
            ReferenceId = TestData.Counties[0].CountyId
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateRiskFactorConfig_ExistingCityReference_ReturnsCreated()
    {
        // Arrange
        await SeedAsync(TestData.Countries);
        await SeedAsync(TestData.Counties);
        await SeedAsync(TestData.Cities);

        var request = TestData.RiskFactorConfigDtoForCreate with
        {
            Level = RiskFactorLevel.City,
            ReferenceId = TestData.Cities[0].CityId
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateRiskFactorConfig_ExistingBuildingTypeReference_ReturnsCreated()
    {
        // Arrange
        await SeedAsync(TestData.BuildingTypes);

        var request = TestData.RiskFactorConfigDtoForCreate with
        {
            Level = RiskFactorLevel.BuildingType,
            ReferenceId = TestData.BuildingTypes[0].BuildingTypeId
        };

        // Act
        var response = await HttpClient.PostAsJsonAsync(BaseUrl, request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    #endregion
}
