namespace InsuranceApp.Api.Middleware;

public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(
            "X-Correlation-ID",
            out var requestCorrelationId)
            && !string.IsNullOrWhiteSpace(requestCorrelationId)
                ? requestCorrelationId.ToString()
                : context.TraceIdentifier;

        context.TraceIdentifier = correlationId;
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        try
        {
            await _next(context);
        }
        finally
        {
            if (context.Response.StatusCode is >= 200 and < 300
                && TryGetBusinessAction(context, out var action))
            {
                _logger.LogInformation(
                    "{Action}. CorrelationId: {CorrelationId}",
                    action,
                    correlationId);
            }
        }
    }

    private static bool TryGetBusinessAction(
        HttpContext context,
        out string action)
    {
        var segments = context.Request.Path.Value?
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            ?? Array.Empty<string>();

        var resource = segments.FirstOrDefault(segment =>
            segment.Equals("clients", StringComparison.OrdinalIgnoreCase)
            || segment.Equals("buildings", StringComparison.OrdinalIgnoreCase));

        var resourceName = resource?.ToLowerInvariant() switch
        {
            "clients" => "Client",
            "buildings" => "Building",
            _ => null
        };

        if (resourceName is null
            || !context.Request.Method.Equals(
                "POST",
                StringComparison.OrdinalIgnoreCase))
        {
            action = string.Empty;
            return false;
        }

        action = $"{resourceName} created";
        return true;
    }
}
