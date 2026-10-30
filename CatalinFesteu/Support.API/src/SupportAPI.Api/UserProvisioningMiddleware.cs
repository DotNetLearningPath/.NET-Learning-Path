using SupportAPI.Application.Abstractions;
using System.Security.Claims;

namespace SupportAPI.Api
{
    public class UserProvisioningMiddleware(
        RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext httpContext,
            IUserProvisioningRepository userRepository,
            ICurrentUser currentUser)
        {
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                var issuer = httpContext.User.FindFirstValue("iss");
                var subject = httpContext.User.FindFirstValue("sub");

                var name = httpContext.User.FindFirstValue("name");

                if (string.IsNullOrWhiteSpace(name))
                {
                    name = httpContext.User.FindFirstValue("preferred_username");
                }
                if (string.IsNullOrWhiteSpace(issuer) ||
                      string.IsNullOrWhiteSpace(subject) ||
                      string.IsNullOrWhiteSpace(name))
                {
                    httpContext.Response.StatusCode =
                        StatusCodes.Status403Forbidden;
                    return;
                }

                var userId = await userRepository.FindOrCreateUserAsync(name, issuer, subject, httpContext.RequestAborted);

                currentUser.SetUserId(userId);
            }

            await next(httpContext);
        }
    }
}




