using InsuranceApp.Domain.Entities;
using InsuranceApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace InsuranceApp.IntegrationTests.Infrastructure;

public sealed class InsuranceAppWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection;

    public InsuranceAppWebApplicationFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        Environment.SetEnvironmentVariable(
            "ASPNETCORE_ENVIRONMENT",
            "Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<InsuranceDbContext>();
            services.RemoveAll<DbContextOptions<InsuranceDbContext>>();

            services.AddDbContext<InsuranceDbContext>(options =>
                options.UseSqlite(_connection));

            using var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();

            var dbContext = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();

            dbContext.Database.EnsureCreated();
        });
    }

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();

        // Delete children before parents to respect foreign keys.
        dbContext.Buildings.RemoveRange(dbContext.Buildings);

        dbContext.Clients.RemoveRange(dbContext.Clients);

        dbContext.Cities.RemoveRange(dbContext.Cities);
        dbContext.Counties.RemoveRange(dbContext.Counties);
        dbContext.Countries.RemoveRange(dbContext.Countries);

        await dbContext.SaveChangesAsync();
    }

    public async Task SeedGeographyAsync()
    {
        using var scope = Services.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();

        var romania = new Country
        {
            CountryId = 1,
            Name = "Romania"
        };

        var hungary = new Country
        {
            CountryId = 2,
            Name = "Hungary"
        };

        var cluj = new County
        {
            CountyId = 1,
            CountryId = 1,
            Name = "Cluj"
        };

        var brasov = new County
        {
            CountyId = 2,
            CountryId = 1,
            Name = "Brasov"
        };

        var clujNapoca = new City
        {
            CityId = 1,
            CountyId = 1,
            Name = "Cluj-Napoca"
        };

        var turda = new City
        {
            CityId = 2,
            CountyId = 1,
            Name = "Turda"
        };

        dbContext.Countries.AddRange(
            romania,
            hungary);

        dbContext.Counties.AddRange(
            cluj,
            brasov);

        dbContext.Cities.AddRange(
            clujNapoca,
            turda);

        await dbContext.SaveChangesAsync();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection.Dispose();
        }

        base.Dispose(disposing);
    }
}