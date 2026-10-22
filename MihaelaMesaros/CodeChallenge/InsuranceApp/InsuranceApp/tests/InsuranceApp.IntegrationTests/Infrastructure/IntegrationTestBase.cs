using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.IntegrationTests.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceApp.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly IServiceScope _scope;

    protected HttpClient HttpClient { get; }

    protected InsuranceDbContext DbContext { get; }

    protected IntegrationTestBase(WebApplicationFactory<Program> factory)
    {
        HttpClient = factory.CreateClient();

        _scope = factory.Services.CreateScope();

        DbContext = _scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();
    }

    public abstract Task InitializeAsync();

    public Task DisposeAsync()
    {
        _scope.Dispose();
        HttpClient.Dispose();

        return Task.CompletedTask;
    }


    #region Reset database
    protected async Task ResetDatabaseAsync()
    {
        DbContext.Policies.RemoveRange(DbContext.Policies);
        DbContext.Buildings.RemoveRange(DbContext.Buildings);
        DbContext.RiskFactorConfigs.RemoveRange(DbContext.RiskFactorConfigs);
        DbContext.Clients.RemoveRange(DbContext.Clients);
        DbContext.Cities.RemoveRange(DbContext.Cities);
        DbContext.Counties.RemoveRange(DbContext.Counties);
        DbContext.Countries.RemoveRange(DbContext.Countries);
        DbContext.BuildingTypes.RemoveRange(DbContext.BuildingTypes);
        DbContext.Currencies.RemoveRange(DbContext.Currencies);
        DbContext.FeeConfigs.RemoveRange(DbContext.FeeConfigs);
        DbContext.Brokers.RemoveRange(DbContext.Brokers);

        await DbContext.SaveChangesAsync();
    }
    #endregion


    #region Seed general methods
    protected async Task SeedAsync<TEntity>(TEntity entity) where TEntity : class
    {
        DbContext.Set<TEntity>().Add(entity);

        await DbContext.SaveChangesAsync();
    }

    protected async Task SeedAsync<TEntity>(List<TEntity> entities) where TEntity : class
    {
        DbContext.Set<TEntity>().AddRange(entities);

        await DbContext.SaveChangesAsync();
    }
    #endregion


    #region Seed buildings
    protected async Task SeedBuildingDependenciesAsync()
    {
        await SeedAsync(TestData.Countries);
        await SeedAsync(TestData.Counties);
        await SeedAsync(TestData.Cities);
        await SeedAsync(TestData.BuildingTypes);
    }
    #endregion


    #region Seed policies
    protected async Task<Policy> SeedPolicyAsync(PolicyStatus status = PolicyStatus.Draft)
    {
        await SeedPolicyReferencesAsync();

        var policy = TestData.CreatePolicyForRead();
        policy.Status = status;

        await SeedAsync(policy);

        return policy;
    }

    protected async Task SeedPolicyReferencesAsync()
    {
        await SeedBuildingDependenciesAsync();

        await SeedAsync(TestData.BuildingClient);
        await SeedAsync(TestData.BuildingForRead);
        await SeedAsync(TestData.BrokerForCreate);
        await SeedAsync(TestData.CurrencyForCreate);
    }

    protected async Task<List<Policy>> SeedPoliciesListAsync()
    {
        await SeedBuildingDependenciesAsync();

        await SeedAsync(TestData.ClientsList);
        await SeedAsync(TestData.BuildingsList);
        await SeedAsync(TestData.BrokersList);
        await SeedAsync(TestData.CurrenciesList);

        var policies = TestData.PoliciesList;

        await SeedAsync(policies);

        return policies;
    }
    #endregion
}