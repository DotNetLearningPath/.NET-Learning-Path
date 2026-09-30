using FluentValidation;
using InsuranceApp.Application.Brokers.Exceptions;
using InsuranceApp.Application.Clients.Exceptions;
using InsuranceApp.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace InsuranceApp.WebApi.Errors;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken
    )
    {
        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(error => error.ErrorMessage)
                        .Distinct()
                        .ToArray()
                );

            if (errors.Count == 0)
            {
                errors[string.Empty] = [validationException.Message];
            }

            await Results.ValidationProblem(
                errors: errors,
                title: "One or more validation errors occurred.",
                instance: httpContext.Request.Path
            ).ExecuteAsync(httpContext);

            return true;
        }

        var (statusCode, title) = exception switch
        {
            EntityNotFoundException =>
                (StatusCodes.Status404NotFound, "Resource not found."),

            DuplicateClientIdentificationException =>
                (StatusCodes.Status409Conflict, "Client already exists."),

            DuplicateBrokerCodeException =>
                (StatusCodes.Status409Conflict, "Broker already exists."),

            _ =>
                (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        Dictionary<string, object?>? extensions = null;

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            var traceId = httpContext.TraceIdentifier;

            _logger.LogError(
                exception,
                "Unexpected error while processing {Path}. TraceId: {TraceId}.",
                httpContext.Request.Path,
                traceId
            );

            extensions = new Dictionary<string, object?>
            {
                ["traceId"] = traceId
            };
        }

        await Results.Problem(
            statusCode: statusCode,
            title: title,
            detail: statusCode == StatusCodes.Status500InternalServerError
                ? "Please contact support and include the traceId."
                : exception.Message,
            instance: httpContext.Request.Path,
            extensions: extensions
        ).ExecuteAsync(httpContext);

        return true;
    }
}
