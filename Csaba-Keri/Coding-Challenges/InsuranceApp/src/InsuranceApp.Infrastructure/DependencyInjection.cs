using InsuranceApp.Application.Buildings;
using InsuranceApp.Application.Clients;
using InsuranceApp.Application.Geography;
using InsuranceApp.Infrastructure.Persistence;
using InsuranceApp.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<InsuranceDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IBuildingRepository, BuildingRepository>();
        services.AddScoped<IGeographyRepository, GeographyRepository>();

        return services;
    }
}
