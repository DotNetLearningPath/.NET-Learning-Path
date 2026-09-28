using Application.Abstractions;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("InsuranceDatabase")
            ?? throw new InvalidOperationException(
                "The InsuranceDatabase connection string is missing.");

        services.AddDbContext<InsuranceDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        var seedDataSection = configuration.GetSection(SeedDataOptions.SectionName);
        services.AddOptions<SeedDataOptions>()
            .Configure(options =>
            {
                options.BasePath = GetRequiredSeedSetting(seedDataSection, nameof(SeedDataOptions.BasePath));
                options.GeographyFile = GetRequiredSeedSetting(seedDataSection, nameof(SeedDataOptions.GeographyFile));
                options.ClientFile = GetRequiredSeedSetting(seedDataSection, nameof(SeedDataOptions.ClientFile));
                options.BuildingFile = GetRequiredSeedSetting(seedDataSection, nameof(SeedDataOptions.BuildingFile));
                options.CurrencyFile = GetRequiredSeedSetting(seedDataSection, nameof(SeedDataOptions.CurrencyFile));
                options.BrokerFile = GetRequiredSeedSetting(seedDataSection, nameof(SeedDataOptions.BrokerFile));
                options.FeeConfigurationFile = GetRequiredSeedSetting(seedDataSection, nameof(SeedDataOptions.FeeConfigurationFile));
                options.RiskFactorConfigurationFile = GetRequiredSeedSetting(seedDataSection, nameof(SeedDataOptions.RiskFactorConfigurationFile));
            });

        services.AddScoped<GeographySeeder>();
        services.AddScoped<CurrencySeeder>();
        services.AddScoped<FeeConfigurationSeeder>();
        services.AddScoped<RiskFactorConfigurationSeeder>();
        services.AddScoped<ClientSeeder>();
        services.AddScoped<BrokerSeeder>();
        services.AddScoped<BuildingSeeder>();

        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IBuildingRepository, BuildingRepository>();
        services.AddScoped<IGeographyRepository, GeographyRepository>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<IBrokerRepository, BrokerRepository>();
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<IFeeConfigurationRepository, FeeConfigurationRepository>();
        services.AddScoped<IRiskFactorRepository, RiskFactorRepository>();

        return services;
    }

    private static string GetRequiredSeedSetting(
        IConfigurationSection section,
        string key)
    {
        return section[key] is { Length: > 0 } value
            ? value
            : throw new InvalidOperationException(
                $"SeedData:{key} configuration is missing.");
    }
}
