using InsuranceApp.Application.Brokers;
using InsuranceApp.Application.Buildings;
using InsuranceApp.Application.Clients;
using InsuranceApp.Application.Currencies;
using InsuranceApp.Application.Fees;
using InsuranceApp.Application.Geography;
using InsuranceApp.Application.Policies;
using InsuranceApp.Application.Reports;
using InsuranceApp.Application.RiskFactors;
using InsuranceApp.Infrastructure.Identity;
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

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.Password.RequiredLength = 12;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
        })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<InsuranceDbContext>();

        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IBuildingRepository, BuildingRepository>();
        services.AddScoped<IGeographyRepository, GeographyRepository>();
        services.AddScoped<IBrokerRepository, BrokerRepository>();
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<IFeeRepository, FeeRepository>();
        services.AddScoped<IRiskFactorRepository, RiskFactorRepository>();
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        services.AddScoped<IPolicyQueryRepository, PolicyQueryRepository>();
        services.AddScoped<IPolicyReportRepository, PolicyReportRepository>();

        return services;
    }
}
