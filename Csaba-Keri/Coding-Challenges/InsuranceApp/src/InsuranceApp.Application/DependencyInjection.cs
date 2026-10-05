using FluentValidation;
using InsuranceApp.Application.Brokers;
using InsuranceApp.Application.Brokers.Commands;
using InsuranceApp.Application.Brokers.Validation;
using InsuranceApp.Application.Buildings;
using InsuranceApp.Application.Buildings.Commands;
using InsuranceApp.Application.Buildings.Validation;
using InsuranceApp.Application.Clients;
using InsuranceApp.Application.Clients.Commands;
using InsuranceApp.Application.Clients.Queries;
using InsuranceApp.Application.Clients.Validation;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Currencies;
using InsuranceApp.Application.Currencies.Commands;
using InsuranceApp.Application.Currencies.Validation;
using InsuranceApp.Application.Fees;
using InsuranceApp.Application.Fees.Commands;
using InsuranceApp.Application.Fees.Validation;
using InsuranceApp.Application.Geography;
using Microsoft.Extensions.DependencyInjection;

namespace InsuranceApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<TimeProvider>(TimeProvider.System);

        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IGeographyService, GeographyService>();
        services.AddScoped<IBuildingService, BuildingService>();
        services.AddScoped<IBrokerService, BrokerService>();
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<IFeeService, FeeService>();

        services.AddScoped<IValidator<CreateClientCommand>, CreateClientCommandValidator>();
        services.AddScoped<IValidator<UpdateClientCommand>, UpdateClientCommandValidator>();
        services.AddScoped<IValidator<SearchClientsQuery>, SearchClientsQueryValidator>();
        services.AddScoped<IValidator<PageQuery>, PageQueryValidator>();
        services.AddScoped<IValidator<BuildingAddressCommand>, BuildingAddressCommandValidator>();
        services.AddScoped<IValidator<CreateBuildingCommand>, CreateBuildingCommandValidator>();
        services.AddScoped<IValidator<UpdateBuildingCommand>, UpdateBuildingCommandValidator>();
        services.AddScoped<IValidator<CreateBrokerCommand>, CreateBrokerCommandValidator>();
        services.AddScoped<IValidator<UpdateBrokerCommand>, UpdateBrokerCommandValidator>();
        services.AddScoped<IValidator<CreateCurrencyCommand>, CreateCurrencyCommandValidator>();
        services.AddScoped<IValidator<UpdateCurrencyCommand>, UpdateCurrencyCommandValidator>();
        services.AddScoped<IValidator<CreateFeeCommand>, CreateFeeCommandValidator>();
        services.AddScoped<IValidator<UpdateFeeCommand>, UpdateFeeCommandValidator>();

        return services;
    }
}
