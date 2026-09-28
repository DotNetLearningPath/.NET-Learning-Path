using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InsuranceApp.IntegrationTests.Infrastructure;

internal class InsuranceApiFactory(SqliteConnection dbConnection) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Program checks for a connection string. The provider is replaced below.
        builder.UseSetting("ConnectionStrings:InsuranceDatabase", "Host=unused;Database=unused");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<InsuranceDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<InsuranceDbContext>>();

            services.AddDbContext<InsuranceDbContext>(options => options.UseSqlite(dbConnection));
        });
    }
}
