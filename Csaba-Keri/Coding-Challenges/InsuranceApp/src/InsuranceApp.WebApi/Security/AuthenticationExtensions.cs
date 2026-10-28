using InsuranceApp.Application.Authentication;
using InsuranceApp.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace InsuranceApp.WebApi.Security;

internal static class AuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .Validate(options => !string.IsNullOrWhiteSpace(options.Issuer), "Jwt:Issuer is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.Audience), "Jwt:Audience is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.SigningKey)
                && Encoding.UTF8.GetByteCount(options.SigningKey) >= JwtOptions.MinimumSigningKeyBytes,
                $"Jwt:SigningKey must contain at least {JwtOptions.MinimumSigningKeyBytes} UTF-8 bytes.")
            .Validate(options => options.AccessTokenLifetimeMinutes is >= 1 and <= 15,
                "Jwt:AccessTokenLifetimeMinutes must be between 1 and 15.")
            .ValidateOnStart();

        services.AddScoped<IAccessTokenGenerator, JwtAccessTokenGenerator>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, configuredJwtOptions) =>
            {
                var jwtOptions = configuredJwtOptions.Value;
                
                options.MapInboundClaims = false;
                options.IncludeErrorDetails = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
                    RequireSignedTokens = true,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidAlgorithms = [SecurityAlgorithms.HmacSha256],
                    NameClaimType = JwtRegisteredClaimNames.Sub,
                    RoleClaimType = JwtClaimNames.Role
                };
            });

        return services;
    }
}
