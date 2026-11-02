using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SupportAPI.Application;
using SupportAPI.Application.Abstractions;
using SupportAPI.Infrastructure;

namespace SupportAPI.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddSupportApi(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("SupportDb");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:SupportDb is required.");
        }

        var keycloak = configuration.GetRequiredSection("Keycloak");
        var metadataAddress = RequiredUri(keycloak, "MetadataAddress", environment);
        var realmUrl = RequiredUri(keycloak, "RealmUrl", environment).TrimEnd('/');
        var issuer = RequiredUri(keycloak, "Issuer", environment);
        var audience = keycloak["Audience"];
        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException("Keycloak:Audience is required.");
        }

        services.AddApplication();
        services.AddInfrastructure(connectionString);
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddScoped<HttpCurrentUser>();
        services.AddScoped<ICurrentUser>(provider => provider.GetRequiredService<HttpCurrentUser>());
        services.AddAuthorization();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MetadataAddress = metadataAddress;
                options.RequireHttpsMetadata = !environment.IsDevelopment();
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true
                };
            });

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Keycloak", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{realmUrl}/protocol/openid-connect/auth"),
                        TokenUrl = new Uri($"{realmUrl}/protocol/openid-connect/token"),
                        Scopes = new Dictionary<string, string> { ["openid"] = "Authenticate the user" }
                    }
                }
            });
            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Keycloak", document)] = ["openid"]
            });
        });

        var frontendOrigins = configuration.GetSection("Frontend:Origins").Get<string[]>()
            ?? ["http://localhost:5173"];
        services.AddCors(options => options.AddPolicy("Frontend", policy => policy
            .WithOrigins(frontendOrigins)
            .WithMethods("GET", "POST")
            .WithHeaders("Authorization", "Content-Type", "Accept")));

        return services;
    }

    private static string RequiredUri(
        IConfigurationSection keycloak, string key, IHostEnvironment environment)
    {
        var value = keycloak[key];
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException($"Keycloak:{key} must be an absolute HTTP or HTTPS URL.");
        }

        if (!environment.IsDevelopment() && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException($"Keycloak:{key} must use HTTPS outside Development.");
        }

        return value!;
    }
}
