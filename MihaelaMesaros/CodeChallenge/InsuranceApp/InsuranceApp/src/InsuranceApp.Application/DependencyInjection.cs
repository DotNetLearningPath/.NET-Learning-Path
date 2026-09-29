using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IGeographyService, GeographyService>();

        services.AddScoped<IClientService, ClientService>();

        services.AddScoped<IBuildingService, BuildingService>();

        return services;
    }
}