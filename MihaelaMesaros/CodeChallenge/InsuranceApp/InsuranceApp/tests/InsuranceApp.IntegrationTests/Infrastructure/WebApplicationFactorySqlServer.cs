using InsuranceApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InsuranceApp.IntegrationTests.Infrastructure;

public sealed class WebApplicationFactorySqlServer : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"Insurance_Test_{Guid.NewGuid():N}";

    private string ConnectionString => $@"Server=(localdb)\MSSQLLocalDB;Database={_databaseName};Trusted_Connection=True;TrustServerCertificate=True;";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<InsuranceDbContext>();
            services.RemoveAll<DbContextOptions<InsuranceDbContext>>();

            services.AddDbContext<InsuranceDbContext>(options => options.UseSqlServer(ConnectionString));
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();

        await dbContext.Database.EnsureCreatedAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        await DeleteDatabaseAsync();
        await base.DisposeAsync();
    }

    public async Task DeleteDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();

        await dbContext.Database.EnsureDeletedAsync();
    }
}