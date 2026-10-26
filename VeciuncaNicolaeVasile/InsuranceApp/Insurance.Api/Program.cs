using Insurance.Application.Interfaces;
using Insurance.Application.Services;
using Insurance.Application.Validators;
using Insurance.Infrastructure.Data;
using Insurance.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database configuration
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

//var configurationDebugView = builder.Configuration.GetDebugView();


builder.Services.AddDbContext<InsuranceDbContext>(options =>
    options.UseSqlite(connectionString));


builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IClientValidator, ClientValidator>();
builder.Services.AddScoped<IClientService, ClientService>();

// API configuration
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/openapi/v1.json",
            "Insurance App API v1");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();