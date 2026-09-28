using System.Net;
using System.Net.Http.Json;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.RiskFactorConfig;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.IntegrationTests.Common;
using InsuranceApp.IntegrationTests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceApp.IntegrationTests.Api.Controllers.Admin;

public sealed class RiskFactorConfigEndpointsTests(
    InsuranceAppWebApplicationFactory factory)
    : IClassFixture<InsuranceAppWebApplicationFactory>, IAsyncLifetime
{
    private const string BaseUrl = "/api/admin/risk-factors";

    private readonly InsuranceAppWebApplicationFactory _factory = factory;
    private readonly HttpClient _client = factory.CreateClient();

    public async Task InitializeAsync()
    {
        await _factory.ResetDatabaseAsync();
        await _factory.SeedGeographyAsync();
        await _factory.SeedBuildingTypesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    #region Create Tests

    [Fact]
    public async Task CreateRiskFactorConfig_ValidRequest_ReturnsCreatedAndPersistsConfig()
    {
        // Arrange
        var countryId = await GetCountryIdAsync("Romania");

        var request = new CreateRiskFactorConfigDto(
            RiskFactorLevel.Country,
            countryId,
            5.25m,
            true);

        // Act
        var response = await _client.PostAsJsonAsync(
            BaseUrl,
            request);

        // Assert - HTTP
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var createdConfig =
            await response.Content
                .ReadFromJsonAsync<RiskFactorConfigDto>();

        Assert.NotNull(createdConfig);

        Assert.NotEqual(
            Guid.Empty,
            createdConfig.RiskFactorConfigId);

        Assert.Equal(
            RiskFactorLevel.Country,
            createdConfig.Level);

        Assert.Equal(
            countryId,
            createdConfig.ReferenceId);

        Assert.Equal(
            5.25m,
            createdConfig.AdjustmentPercentage);

        Assert.True(createdConfig.IsActive);

        Assert.NotNull(response.Headers.Location);

        Assert.Equal(
            $"{BaseUrl}/{createdConfig.RiskFactorConfigId}",
            response.Headers.Location.AbsolutePath);

        // Assert - persistence
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<InsuranceDbContext>();

        var persistedConfig =
            await dbContext.RiskFactorConfigs
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.RiskFactorConfigId ==
                         createdConfig.RiskFactorConfigId);

        Assert.NotNull(persistedConfig);

        Assert.Equal(
            countryId,
            persistedConfig.ReferenceId);

        Assert.Equal(
            5.25m,
            persistedConfig.AdjustmentPercentage);
    }

    [Fact]
    public async Task CreateRiskFactorConfig_NegativePercentage_ReturnsCreated()
    {
        // Arrange
        var countryId = await GetCountryIdAsync("Romania");

        var request = new CreateRiskFactorConfigDto(
            RiskFactorLevel.Country,
            countryId,
            -5.25m,
            true);

        // Act
        var response = await _client.PostAsJsonAsync(
            BaseUrl,
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var createdConfig =
            await response.Content
                .ReadFromJsonAsync<RiskFactorConfigDto>();

        Assert.NotNull(createdConfig);

        Assert.Equal(
            -5.25m,
            createdConfig.AdjustmentPercentage);
    }

    [Fact]
    public async Task CreateRiskFactorConfig_InvalidLevel_ReturnsBadRequest()
    {
        // Arrange
        var countryId = await GetCountryIdAsync("Romania");

        var request = new CreateRiskFactorConfigDto(
            (RiskFactorLevel)999,
            countryId,
            5.25m,
            true);

        // Act
        var response = await _client.PostAsJsonAsync(
            BaseUrl,
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateRiskFactorConfig_EmptyReferenceId_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateRiskFactorConfigDto(
            RiskFactorLevel.Country,
            Guid.Empty,
            5.25m,
            true);

        // Act
        var response = await _client.PostAsJsonAsync(
            BaseUrl,
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problem =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);

        Assert.Equal(
            RiskFactorConfigErrors.InvalidReferenceId.Code,
            problem.Extensions["code"]?.ToString());
    }

    [Fact]
    public async Task CreateRiskFactorConfig_NonExistingReference_ReturnsNotFound()
    {
        // Arrange
        var request = new CreateRiskFactorConfigDto(
            RiskFactorLevel.Country,
            TestConstants.NonExistingId,
            5.25m,
            true);

        // Act
        var response = await _client.PostAsJsonAsync(
            BaseUrl,
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        var problem =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);

        Assert.Equal(
            RiskFactorConfigErrors.ReferenceNotFound.Code,
            problem.Extensions["code"]?.ToString());
    }

    [Fact]
    public async Task CreateRiskFactorConfig_PercentageAboveMaximum_ReturnsBadRequest()
    {
        // Arrange
        var countryId = await GetCountryIdAsync("Romania");

        var request = new CreateRiskFactorConfigDto(
            RiskFactorLevel.Country,
            countryId,
            100.01m,
            true);

        // Act
        var response = await _client.PostAsJsonAsync(
            BaseUrl,
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateRiskFactorConfig_PercentageBelowMinimum_ReturnsBadRequest()
    {
        // Arrange
        var countryId = await GetCountryIdAsync("Romania");

        var request = new CreateRiskFactorConfigDto(
            RiskFactorLevel.Country,
            countryId,
            -100.01m,
            true);

        // Act
        var response = await _client.PostAsJsonAsync(
            BaseUrl,
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateRiskFactorConfig_PercentageWithTooManyDecimals_ReturnsBadRequest()
    {
        // Arrange
        var countryId = await GetCountryIdAsync("Romania");

        var request = new CreateRiskFactorConfigDto(
            RiskFactorLevel.Country,
            countryId,
            5.123m,
            true);

        // Act
        var response = await _client.PostAsJsonAsync(
            BaseUrl,
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);

        var problem =
            await response.Content
                .ReadFromJsonAsync<ProblemDetails>();

        Assert.NotNull(problem);

        Assert.Equal(
            RiskFactorConfigErrors.InvalidAdjustmentPercentageScale.Code,
            problem.Extensions["code"]?.ToString());
    }

    [Fact]
    public async Task CreateRiskFactorConfig_DuplicateLevelAndReference_ReturnsConflict()
    {
        // Arrange
        var countryId = await GetCountryIdAsync("Romania");

        var request = new CreateRiskFactorConfigDto(
            RiskFactorLevel.Country,
            countryId,
            5.25m,
            true);

        var firstResponse = await _client.PostAsJsonAsync(
            BaseUrl,
            request);

        Assert.Equal(
            HttpStatusCode.Created,
            firstResponse.StatusCode);

        // Act
        var secondResponse = await _client.PostAsJsonAsync(
            BaseUrl,
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Conflict,
            secondResponse.StatusCode);

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<InsuranceDbContext>();

        var count = await dbContext.RiskFactorConfigs
            .CountAsync(x =>
                x.Level == RiskFactorLevel.Country &&
                x.ReferenceId == countryId);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task CreateRiskFactorConfig_ConcurrentDuplicate_OnlyOneIsCreated()
    {
        // Arrange
        var countryId = await GetCountryIdAsync("Romania");

        var request = new CreateRiskFactorConfigDto(
            RiskFactorLevel.Country,
            countryId,
            5.25m,
            true);

        // Act
        var task1 = _client.PostAsJsonAsync(
            BaseUrl,
            request);

        var task2 = _client.PostAsJsonAsync(
            BaseUrl,
            request);

        var responses = await Task.WhenAll(
            task1,
            task2);

        // Assert
        Assert.Contains(
            responses,
            x => x.StatusCode == HttpStatusCode.Created);

        Assert.Contains(
            responses,
            x => x.StatusCode == HttpStatusCode.Conflict);

        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<InsuranceDbContext>();

        var count = await dbContext.RiskFactorConfigs
            .CountAsync(x =>
                x.Level == RiskFactorLevel.Country &&
                x.ReferenceId == countryId);

        Assert.Equal(1, count);
    }
    #endregion

    #region Read Tests

    [Fact]
    public async Task GetRiskFactorConfigs_ReturnsOkWithConfigs()
    {
        // Arrange
        var romaniaId = await GetCountryIdAsync("Romania");
        var hungaryId = await GetCountryIdAsync("Hungary");

        await SeedRiskFactorConfigAsync(
            RiskFactorLevel.Country,
            romaniaId,
            5.25m);

        await SeedRiskFactorConfigAsync(
            RiskFactorLevel.Country,
            hungaryId,
            -2.50m);

        // Act
        var response = await _client.GetAsync(BaseUrl);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var configs =
            await response.Content
                .ReadFromJsonAsync<List<RiskFactorConfigDto>>();

        Assert.NotNull(configs);
        Assert.Equal(2, configs.Count);

        Assert.Contains(
            configs,
            x => x.ReferenceId == romaniaId);

        Assert.Contains(
            configs,
            x => x.ReferenceId == hungaryId);
    }

    [Fact]
    public async Task GetRiskFactorConfigById_ExistingConfig_ReturnsOk()
    {
        // Arrange
        var countryId = await GetCountryIdAsync("Romania");

        var configId = await SeedRiskFactorConfigAsync(
            RiskFactorLevel.Country,
            countryId,
            5.25m);

        // Act
        var response = await _client.GetAsync(
            $"{BaseUrl}/{configId}");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var config =
            await response.Content
                .ReadFromJsonAsync<RiskFactorConfigDto>();

        Assert.NotNull(config);
        Assert.Equal(configId, config.RiskFactorConfigId);
        Assert.Equal(countryId, config.ReferenceId);
    }

    [Fact]
    public async Task GetRiskFactorConfigById_NonExistingConfig_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync(
            $"{BaseUrl}/{TestConstants.NonExistingId}");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task GetRiskFactorConfigById_EmptyId_ReturnsBadRequest()
    {
        // Act
        var response = await _client.GetAsync(
            $"{BaseUrl}/{Guid.Empty}");

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    #endregion

    #region Update Tests

    [Fact]
    public async Task UpdateRiskFactorConfig_ValidRequest_ReturnsOkAndPersistsChanges()
    {
        // Arrange
        var romaniaId = await GetCountryIdAsync("Romania");
        var hungaryId = await GetCountryIdAsync("Hungary");

        var configId = await SeedRiskFactorConfigAsync(
            RiskFactorLevel.Country,
            romaniaId,
            5.25m);

        var request = new UpdateRiskFactorConfigDto(
            RiskFactorLevel.Country,
            hungaryId,
            -3.50m,
            false);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"{BaseUrl}/{configId}",
            request);

        // Assert - HTTP
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var updatedConfig =
            await response.Content
                .ReadFromJsonAsync<RiskFactorConfigDto>();

        Assert.NotNull(updatedConfig);

        Assert.Equal(
            configId,
            updatedConfig.RiskFactorConfigId);

        Assert.Equal(
            hungaryId,
            updatedConfig.ReferenceId);

        Assert.Equal(
            -3.50m,
            updatedConfig.AdjustmentPercentage);

        Assert.False(updatedConfig.IsActive);

        // Assert - persistence
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<InsuranceDbContext>();

        var persistedConfig =
            await dbContext.RiskFactorConfigs
                .AsNoTracking()
                .FirstAsync(
                    x => x.RiskFactorConfigId == configId);

        Assert.Equal(
            hungaryId,
            persistedConfig.ReferenceId);

        Assert.Equal(
            -3.50m,
            persistedConfig.AdjustmentPercentage);

        Assert.False(persistedConfig.IsActive);
        Assert.NotNull(persistedConfig.ModifiedAt);
    }

    [Fact]
    public async Task UpdateRiskFactorConfig_NonExistingConfig_ReturnsNotFound()
    {
        // Arrange
        var countryId = await GetCountryIdAsync("Romania");

        var request = new UpdateRiskFactorConfigDto(
            RiskFactorLevel.Country,
            countryId,
            5.25m,
            true);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"{BaseUrl}/{TestConstants.NonExistingId}",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task UpdateRiskFactorConfig_EmptyId_ReturnsBadRequest()
    {
        // Arrange
        var countryId = await GetCountryIdAsync("Romania");

        var request = new UpdateRiskFactorConfigDto(
            RiskFactorLevel.Country,
            countryId,
            5.25m,
            true);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"{BaseUrl}/{Guid.Empty}",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task UpdateRiskFactorConfig_NonExistingReference_ReturnsNotFound()
    {
        // Arrange
        var countryId = await GetCountryIdAsync("Romania");

        var configId = await SeedRiskFactorConfigAsync(
            RiskFactorLevel.Country,
            countryId,
            5.25m);

        var request = new UpdateRiskFactorConfigDto(
            RiskFactorLevel.Country,
            TestConstants.NonExistingId,
            5.25m,
            true);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"{BaseUrl}/{configId}",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task UpdateRiskFactorConfig_DuplicateLevelAndReference_ReturnsConflict()
    {
        // Arrange
        var romaniaId = await GetCountryIdAsync("Romania");
        var hungaryId = await GetCountryIdAsync("Hungary");

        await SeedRiskFactorConfigAsync(
            RiskFactorLevel.Country,
            romaniaId,
            5.25m);

        var secondConfigId = await SeedRiskFactorConfigAsync(
            RiskFactorLevel.Country,
            hungaryId,
            2.50m);

        var request = new UpdateRiskFactorConfigDto(
            RiskFactorLevel.Country,
            romaniaId,
            3.50m,
            true);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"{BaseUrl}/{secondConfigId}",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Conflict,
            response.StatusCode);
    }

    [Fact]
    public async Task UpdateRiskFactorConfig_InvalidLevel_ReturnsBadRequest()
    {
        // Arrange
        var countryId = await GetCountryIdAsync("Romania");

        var configId = await SeedRiskFactorConfigAsync(
            RiskFactorLevel.Country,
            countryId,
            5.25m);

        var request = new UpdateRiskFactorConfigDto(
            (RiskFactorLevel)999,
            countryId,
            5.25m,
            true);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"{BaseUrl}/{configId}",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }

    [Fact]
    public async Task UpdateRiskFactorConfig_PercentageWithTooManyDecimals_ReturnsBadRequest()
    {
        // Arrange
        var countryId = await GetCountryIdAsync("Romania");

        var configId = await SeedRiskFactorConfigAsync(
            RiskFactorLevel.Country,
            countryId,
            5.25m);

        var request = new UpdateRiskFactorConfigDto(
            RiskFactorLevel.Country,
            countryId,
            5.123m,
            true);

        // Act
        var response = await _client.PutAsJsonAsync(
            $"{BaseUrl}/{configId}",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.BadRequest,
            response.StatusCode);
    }
    #endregion

    #region Reference Level Tests

    [Fact]
    public async Task CreateRiskFactorConfig_ExistingCountyReference_ReturnsCreated()
    {
        // Arrange
        var countyId = await GetCountyIdAsync("Cluj");

        var request = new CreateRiskFactorConfigDto(
            RiskFactorLevel.County,
            countyId,
            3.25m,
            true);

        // Act
        var response = await _client.PostAsJsonAsync(
            BaseUrl,
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateRiskFactorConfig_ExistingCityReference_ReturnsCreated()
    {
        // Arrange
        var cityId = await GetCityIdAsync("Cluj-Napoca");

        var request = new CreateRiskFactorConfigDto(
            RiskFactorLevel.City,
            cityId,
            4.50m,
            true);

        // Act
        var response = await _client.PostAsJsonAsync(
            BaseUrl,
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateRiskFactorConfig_ExistingBuildingTypeReference_ReturnsCreated()
    {
        // Arrange
        var buildingTypeId =
            await GetBuildingTypeIdAsync("Residential");

        var request = new CreateRiskFactorConfigDto(
            RiskFactorLevel.BuildingType,
            buildingTypeId,
            7.50m,
            true);

        // Act
        var response = await _client.PostAsJsonAsync(
            BaseUrl,
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);
    }

    #endregion

    #region Helpers

    private async Task<Guid> SeedRiskFactorConfigAsync(
        RiskFactorLevel level,
        Guid referenceId,
        decimal adjustmentPercentage)
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<InsuranceDbContext>();

        var config = new RiskFactorConfig
        {
            RiskFactorConfigId = Guid.NewGuid(),
            Level = level,
            ReferenceId = referenceId,
            AdjustmentPercentage = adjustmentPercentage,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.RiskFactorConfigs.Add(config);

        await dbContext.SaveChangesAsync();

        return config.RiskFactorConfigId;
    }

    private async Task<Guid> GetCountryIdAsync(string name)
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<InsuranceDbContext>();

        return await dbContext.Countries
            .AsNoTracking()
            .Where(x => x.Name == name)
            .Select(x => x.CountryId)
            .SingleAsync();
    }

    private async Task<Guid> GetCountyIdAsync(string name)
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<InsuranceDbContext>();

        return await dbContext.Counties
            .AsNoTracking()
            .Where(x => x.Name == name)
            .Select(x => x.CountyId)
            .SingleAsync();
    }

    private async Task<Guid> GetCityIdAsync(string name)
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<InsuranceDbContext>();

        return await dbContext.Cities
            .AsNoTracking()
            .Where(x => x.Name == name)
            .Select(x => x.CityId)
            .SingleAsync();
    }

    private async Task<Guid> GetBuildingTypeIdAsync(string name)
    {
        using var scope = _factory.Services.CreateScope();

        var dbContext = scope.ServiceProvider
            .GetRequiredService<InsuranceDbContext>();

        return await dbContext.BuildingTypes
            .AsNoTracking()
            .Where(x => x.Name == name)
            .Select(x => x.BuildingTypeId)
            .SingleAsync();
    }

    #endregion
}
