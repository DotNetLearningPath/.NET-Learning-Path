using InsuranceApp.Application.Common.Security;
using Microsoft.AspNetCore.Authorization;

namespace InsuranceApp.WebApi.Security;

internal static class AuthorizationExtensions
{
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build())
            .AddPolicy(AuthorizationPolicies.AdminOnly, policy => policy.RequireAuthenticatedUser()
                .RequireRole(ApplicationRoles.Admin))
            .AddPolicy(AuthorizationPolicies.BrokerOnly, policy => policy.RequireAuthenticatedUser()
                .RequireRole(ApplicationRoles.Broker));

        return services;
    }
}
