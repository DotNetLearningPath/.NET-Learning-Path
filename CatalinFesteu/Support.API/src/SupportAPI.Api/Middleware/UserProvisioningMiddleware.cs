using Microsoft.IdentityModel.JsonWebTokens;
using SupportAPI.Application.Abstractions;
using System.Security.Claims;

namespace SupportAPI.Api.Middleware
{
    public class UserProvisioningMiddleware(
        RequestDelegate next)
    {
        public async Task InvokeAsync(
            HttpContext httpContext,
            IUserProvisioningService userProvisioningService,
            ICurrentUser currentUser)
        {
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                var issuer = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Iss);
                var subject = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

                var name = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Name);

                if (string.IsNullOrWhiteSpace(name))
                {
                    name = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.PreferredUsername);
                }
                if (string.IsNullOrWhiteSpace(issuer) ||
                      string.IsNullOrWhiteSpace(subject) ||
                      string.IsNullOrWhiteSpace(name))
                {
                    httpContext.Response.StatusCode =
                        StatusCodes.Status403Forbidden;
                    return;
                }

                var userId = await userProvisioningService.FindOrCreateUserAsync(name, issuer, subject, httpContext.RequestAborted);

                currentUser.SetUserId(userId);
            }

            await next(httpContext);
        }
    }
}
