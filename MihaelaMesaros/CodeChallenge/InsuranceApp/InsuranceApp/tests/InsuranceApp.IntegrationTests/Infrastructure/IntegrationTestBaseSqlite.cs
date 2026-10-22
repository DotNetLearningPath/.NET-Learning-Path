namespace InsuranceApp.IntegrationTests.Infrastructure;

public abstract class IntegrationTestBaseSqlite(WebApplicationFactorySqlite factory) : IntegrationTestBase(factory)
{
    public override async Task InitializeAsync()
    {
        await ResetDatabaseAsync();
    }
}
