using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var environment = configuration["ASPNETCORE_ENVIRONMENT"] ?? "Production";

        if (environment.Equals("Testing", StringComparison.OrdinalIgnoreCase))
        {
            // For testing, the database provider is configured in WebApplicationFactory.
            services.AddDbContext<InsuranceDbContext>();
        }
        else
        {
            // Only register SQL Server for non-testing environments.
            var dbConnectionString = configuration.GetConnectionString("InsuranceAppDBConnection") ?? throw new InvalidOperationException("Connection string 'InsuranceAppDBConnection' was not found.");
            services.AddDbContext<InsuranceDbContext>(options => options.UseSqlServer(dbConnectionString));
        }

        services.AddScoped<IGeographyRepository, GeographyRepository>();

        services.AddScoped<IClientRepository, ClientRepository>();

        services.AddScoped<IBuildingRepository, BuildingRepository>();

        services.AddScoped<ICurrencyRepository, CurrencyRepository>();

        services.AddScoped<IFeeConfigRepository, FeeConfigRepository>();

        services.AddScoped<IRiskFactorConfigRepository, RiskFactorConfigRepository>();

        return services;
    }
}