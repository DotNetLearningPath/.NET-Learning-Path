using InsuranceApp.Application.Abstractions.Security;
using InsuranceApp.Application.DTOs.Auth;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.IntegrationTests.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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
        DbContext.UserRefreshTokens.RemoveRange(DbContext.UserRefreshTokens);
        DbContext.Users.RemoveRange(DbContext.Users);
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
        DbContext.ChangeTracker.Clear();
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
        if (!await DbContext.Brokers.AnyAsync(x => x.BrokerId == TestData.BrokerForCreate.BrokerId))
        {
            await SeedAsync(TestData.BrokerForCreate);
        }
        await SeedAsync(TestData.CurrencyForCreate);
    }

    protected async Task<List<Policy>> SeedPoliciesListAsync()
    {
        await SeedBuildingDependenciesAsync();

        await SeedAsync(TestData.ClientsList);
        await SeedAsync(TestData.BuildingsList);
        foreach (var broker in TestData.BrokersList)
        {
            if (!await DbContext.Brokers.AnyAsync(x => x.BrokerId == broker.BrokerId))
            {
                await SeedAsync(broker);
            }
        }
        await SeedAsync(TestData.CurrenciesList);

        var policies = TestData.PoliciesList;

        await SeedAsync(policies);

        return policies;
    }
    #endregion


    #region Seed users
    protected async Task<User> SeedUserAsync(User user)
    {
        var passwordService = _scope.ServiceProvider
            .GetRequiredService<IPasswordService>();

        user.PasswordHash = passwordService.HashPassword(
            user,
            TestData.TestPassword);

        await SeedAsync(user);

        return user;
    }
    #endregion


    #region Authentication

    protected async Task<LoginResponseDto> LoginAsync(string username)
    {
        var response = await HttpClient.PostAsJsonAsync("/api/auth/login", new LoginDto { Username = username, Password = TestData.TestPassword });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>();

        Assert.NotNull(result);

        return result;
    }

    protected async Task AuthenticateAsAdminAsync()
    {
        var admin = TestData.CreateAdminUser();
        await SeedUserAsync(admin);

        var login = await LoginAsync(admin.Username);

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
    }

    protected async Task AuthenticateAsBrokerAsync(Guid brokerId)
    {
        var brokerUser = await DbContext.Users.FirstOrDefaultAsync(x => x.Username == TestData.BrokerUsername);

        if (brokerUser is null)
        {
            brokerUser = TestData.CreateBrokerUser(brokerId);
            await SeedUserAsync(brokerUser);
        }
        else
        {
            brokerUser.BrokerId = brokerId;
            await DbContext.SaveChangesAsync();
        }

        var login = await LoginAsync(brokerUser.Username);

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login.AccessToken);
    }
    #endregion
}
