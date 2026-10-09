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

        services.AddScoped<ICurrencyService, CurrencyService>();

        services.AddScoped<IFeeConfigService, FeeConfigService>();

        services.AddScoped<IRiskFactorConfigService, RiskFactorConfigService>();

        services.AddScoped<IBrokerService, BrokerService>();

        services.AddScoped<IPolicyService, PolicyService>();
        services.AddScoped<IPolicyReferenceService, PolicyReferenceService>();
        services.AddScoped<IPolicyNumberGenerator, PolicyNumberGenerator>();
        services.AddScoped<IPolicyCalculationService, PolicyCalculationService>();

        return services;
    }
}