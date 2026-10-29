using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Services;
using InsuranceApp.Application.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
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
        services.AddSingleton<IPolicyNumberGeneratorService, PolicyNumberGeneratorService>();
        services.AddScoped<IPolicyCalculationService, PolicyCalculationService>();

        services.AddScoped<IPolicyReportService, PolicyReportService>();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();

        services.AddOptions<ApplicationSettings>()
            .Bind(configuration.GetSection(ApplicationSettings.SectionName))
            .Validate(
                settings => !string.IsNullOrWhiteSpace(settings.BaseCurrencyCode),
                "BaseCurrencyCode must be configured."
            )
            .ValidateOnStart();

        return services;
    }
}