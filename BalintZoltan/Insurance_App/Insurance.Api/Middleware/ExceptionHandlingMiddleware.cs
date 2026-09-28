using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            ArgumentException =>
                (HttpStatusCode.BadRequest, "The request is invalid."),
            InvalidOperationException invalidOperationException when
                invalidOperationException.Message.Contains(
                    "not found", StringComparison.OrdinalIgnoreCase) =>
                (HttpStatusCode.NotFound, "The requested resource was not found."),
            InvalidOperationException =>
                (HttpStatusCode.Conflict, "The request conflicts with the current state."),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        if ((int)statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}. CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);
        }
        else
        {
            _logger.LogWarning(
                "Request failed with status code {StatusCode} for {Method} {Path}. Error: {ErrorMessage}. CorrelationId: {CorrelationId}",
                (int)statusCode,
                context.Request.Method,
                context.Request.Path,
                exception.Message,
                context.TraceIdentifier);
        }

        if (context.Response.HasStarted)
        {
            _logger.LogWarning(
                "The response had already started; the exception response could not be written. CorrelationId: {CorrelationId}",
                context.TraceIdentifier);
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/problem+json";

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = (int)statusCode >= 500
                ? "Please contact support with the correlation ID."
                : exception.Message,
            Instance = context.Request.Path
        };
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problemDetails),
            context.RequestAborted);
    }
}
