using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.IntegrationTests.Authentication;
using InsuranceApp.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InsuranceApp.IntegrationTests.Infrastructure;

internal class InsuranceApiFactory(string connectionString) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:InsuranceDatabase", connectionString);
        builder.UseSetting("Jwt:Issuer", AuthenticationTestData.Issuer);
        builder.UseSetting("Jwt:Audience", AuthenticationTestData.Audience);
        builder.UseSetting("Jwt:SigningKey", AuthenticationTestData.SigningKey);
        builder.UseSetting("Jwt:AccessTokenLifetimeMinutes", "15");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<InsuranceDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<InsuranceDbContext>>();

            services.AddDbContext<InsuranceDbContext>(options => options.UseNpgsql(connectionString));
        });
    }
}
