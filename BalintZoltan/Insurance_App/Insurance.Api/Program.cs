using Application.Abstractions;
using Application.Services;
using Infrastructure;
using Infrastructure.Persistence;
using Infrastructure.Seed;
using Microsoft.EntityFrameworkCore;
using InsuranceApp.Api.Middleware;
using InsuranceApp.Api.Logging;

var builder = WebApplication.CreateBuilder(args);

var logDirectory = builder.Configuration["Logging:File:Path"]
    ?? Path.Combine("..", "Log");
var resolvedLogDirectory = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, logDirectory));

builder.Logging.AddProvider(new DailyFileLoggerProvider(resolvedLogDirectory));

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IBuildingService, BuildingService>();
builder.Services.AddScoped<IGeographyService, GeographyService>();

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<InsuranceDbContext>();

    await dbContext.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
    await scope.ServiceProvider.GetRequiredService<GeographySeeder>().SeedAsync(
        dbContext,
        app.Environment.ContentRootPath,
        app.Lifetime.ApplicationStopping);
    await scope.ServiceProvider.GetRequiredService<CurrencySeeder>().SeedAsync(
        dbContext,
        app.Environment.ContentRootPath,
        app.Lifetime.ApplicationStopping);
    await scope.ServiceProvider.GetRequiredService<FeeConfigurationSeeder>().SeedAsync(
        dbContext,
        app.Environment.ContentRootPath,
        app.Lifetime.ApplicationStopping);
    await scope.ServiceProvider.GetRequiredService<RiskFactorConfigurationSeeder>().SeedAsync(
        dbContext,
        app.Environment.ContentRootPath,
        app.Lifetime.ApplicationStopping);
    await scope.ServiceProvider.GetRequiredService<ClientSeeder>().SeedAsync(
        dbContext,
        app.Environment.ContentRootPath,
        app.Lifetime.ApplicationStopping);
    await scope.ServiceProvider.GetRequiredService<BrokerSeeder>().SeedAsync(
        dbContext,
        app.Environment.ContentRootPath,
        app.Lifetime.ApplicationStopping);
    await scope.ServiceProvider.GetRequiredService<BuildingSeeder>().SeedAsync(
        dbContext,
        app.Environment.ContentRootPath,
        app.Lifetime.ApplicationStopping);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

await app.RunAsync(app.Lifetime.ApplicationStopping);
