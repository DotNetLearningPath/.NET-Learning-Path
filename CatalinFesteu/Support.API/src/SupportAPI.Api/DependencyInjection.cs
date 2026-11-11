using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using SupportAPI.Api.ExceptionHandlers;
using SupportAPI.Application.Abstractions;
using SupportAPI.Application.Services;

namespace SupportAPI.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddIdentityAndAccessManagement(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var keycloakConfiguration = configuration.GetKeycloakConfiguration();
            services.AddAuthorization();
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                 {
                     options.MetadataAddress = keycloakConfiguration.MetadataAddress;
                     options.RequireHttpsMetadata = true;
                     options.MapInboundClaims = false;
                     options.TokenValidationParameters = new TokenValidationParameters
                     {
                         ValidateIssuer = true,
                         ValidIssuer = keycloakConfiguration.Issuer,

                         ValidateAudience = true,
                         ValidAudience = keycloakConfiguration.Audience,

                         ValidateLifetime = true,
                         RequireExpirationTime = true,
                         ValidateIssuerSigningKey = true
                     };
                 });

            services.AddScoped<ICurrentUser, HttpCurrentUser>();
            services.AddScoped<IUserProvisioningService, UserProvisioningService>();

            return services;
        }

        public static IServiceCollection AddSwaggerSetup(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var keycloakConfiguration = configuration.GetKeycloakConfiguration();
            services.AddEndpointsApiExplorer();

            services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Keycloak", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri(keycloakConfiguration.AuthorizationUrl),
                            TokenUrl = new Uri(keycloakConfiguration.TokenUrl),
                            Scopes = new Dictionary<string, string> { ["openid"] = "Authenticate the user" }
                        }
                    }
                });
                options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference("Keycloak", document)] = ["openid"]
                });
            });

            return services;
        }

        public static KeycloakConfiguration GetKeycloakConfiguration(this IConfiguration configuration)
        {
            return configuration.GetSection(KeycloakConfiguration.SectionName).Get<KeycloakConfiguration>()
                ?? throw new InvalidOperationException("Keycloak settings are missing");
        }

        public static IServiceCollection AddErrorHandling(this IServiceCollection services)
        {
            services.AddExceptionHandler<ValidationExceptionHandler>();
            services.AddExceptionHandler<GlobalExceptionHandler>();

            return services;
        }
    }
}
