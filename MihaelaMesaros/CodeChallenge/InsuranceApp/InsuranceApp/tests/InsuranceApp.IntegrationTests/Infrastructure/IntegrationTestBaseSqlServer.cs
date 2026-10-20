namespace InsuranceApp.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBaseSqlServer(WebApplicationFactorySqlServer factory) : IntegrationTestBase(factory)
{
    public override async Task InitializeAsync()
    {
        await factory.InitializeDatabaseAsync();
        await ResetDatabaseAsync();
    }
}
