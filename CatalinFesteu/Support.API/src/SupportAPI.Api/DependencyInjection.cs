using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace SupportAPI.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfigurationSection keycloakSettings)
        { 
            
            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MetadataAddress = keycloakSettings["MetadataAddress"]!;
                    options.RequireHttpsMetadata = false;

                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = keycloakSettings["Issuer"],
                        
                        ValidateAudience = true,
                        ValidAudience = keycloakSettings["Audience"],
                        
                        ValidateLifetime = true,

                        RequireExpirationTime = true,

                        ValidateIssuerSigningKey = true,
                    };
                });

            return services;
        }
    }
}
