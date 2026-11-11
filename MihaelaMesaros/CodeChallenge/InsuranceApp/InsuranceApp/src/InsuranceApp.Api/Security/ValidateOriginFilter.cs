using InsuranceApp.Api.Configurations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace InsuranceApp.Api.Security;

/// <summary>
/// Validates the request origin against the configured allowed origins to help prevent CSRF attacks.
/// </summary>
internal sealed class ValidateOriginFilter(IOptions<CorsSettings> corsOptions) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var origin = context.HttpContext.Request.Headers.Origin.ToString();

        if (string.IsNullOrWhiteSpace(origin) || !corsOptions.Value.AllowedOrigins.Contains(origin, StringComparer.Ordinal))
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            return;
        }

        await next();
    }
}