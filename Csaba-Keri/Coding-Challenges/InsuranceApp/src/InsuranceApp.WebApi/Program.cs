using InsuranceApp.Application;
using InsuranceApp.Infrastructure;
using InsuranceApp.WebApi.Errors;
using InsuranceApp.WebApi.Security;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

namespace InsuranceApp.WebApi;

public sealed class Program
{
    private Program()
    {
    }

    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var connectionString = builder.Configuration.GetConnectionString("InsuranceDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'InsuranceDatabase' is missing.");
        }

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(allowIntegerValues: false)
                );
            });

        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddProblemDetails();

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(options =>
            {
                options.UseOneOfForPolymorphism();

                options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                options.OperationFilter<BearerSecurityOperationFilter>();
            });
        }

        builder.Services.AddApplication();

        builder.Services.AddInfrastructure(connectionString);

        builder.Services.AddJwtAuthentication();
        builder.Services.AddAuthorizationPolicies();

        var app = builder.Build();

        app.UseExceptionHandler();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
