using InsuranceManagement.API.Middleware;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Application.Interfaces.Services;
using InsuranceManagement.Application.Mapping;
using InsuranceManagement.Application.Services;
using InsuranceManagement.Infrastructure.Persistence;
using InsuranceManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

var connectionString = builder.Configuration
    .GetConnectionString("InsuranceManagementConnection")
    ?? throw new InvalidOperationException(
        "Database connection string is missing.");

builder.Services.AddDbContext<InsuranceDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IBuildingRepository, BuildingRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<ICountyRepository, CountyRepository>();
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<IRiskFactorConfigurationRepository, RiskFactorConfigurationRepository>();

builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<ICountyService, CountyService>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IBuildingService, BuildingService>();
builder.Services.AddScoped<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IBrokerService, BrokerService>();
builder.Services.AddScoped<IFeeConfigurationService, FeeConfigurationService>();
builder.Services.AddScoped<IRiskFactorConfigurationService, RiskFactorConfigurationService>();

builder.Services.AddAutoMapper(
    config => { },
    typeof(GeographyMappingProfile),
    typeof(ClientMappingProfile),
    typeof(BuildingMappingProfile),
    typeof(CurrencyMappingProfile),
    typeof(BrokerMappingProfile),
    typeof(FeeConfigurationMappingProfile),
    typeof(RiskFactorConfigurationMappingProfile));

builder.Services.AddControllers();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program { }
