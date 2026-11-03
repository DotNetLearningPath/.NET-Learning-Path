using InsuranceManagement.Infrastructure.Persistence;
using InsuranceManagement.Application.Interfaces;
using InsuranceManagement.Application.Services;
using InsuranceManagement.Application.Mapping;
using InsuranceManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration
    .GetConnectionString("InsuranceManagementConnection")
    ?? throw new InvalidOperationException(
        "Database connection string is missing.");

builder.Services.AddDbContext<InsuranceDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<ICountyService, CountyService>();
builder.Services.AddScoped<ICityService, CityService>();

builder.Services.AddAutoMapper(
    config => { },
    typeof(GeographyMappingProfile));

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
