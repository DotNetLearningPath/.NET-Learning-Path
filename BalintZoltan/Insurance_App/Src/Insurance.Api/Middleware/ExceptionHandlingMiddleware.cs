using Insurance.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace Insurance.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
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
            NotFoundException =>
                (HttpStatusCode.NotFound, "The requested resource was not found."),
            InvalidOperationException =>
                (HttpStatusCode.Conflict, "The request conflicts with the current state."),
            _ => (HttpStatusCode.InternalServerError, "An unexpected error occurred.")
        };

        if ((int)statusCode >= 500)
        {
            logger.LogError(
                exception,
                "Unhandled exception while processing {Method} {Path}. CorrelationId: {CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.TraceIdentifier);
        }
        else
        {
            logger.LogError(
                "Request failed with status code {StatusCode} for {Method} {Path}. Error: {ErrorMessage}. CorrelationId: {CorrelationId}",
                (int)statusCode,
                context.Request.Method,
                context.Request.Path,
                exception.Message,
                context.TraceIdentifier);
        }

        if (context.Response.HasStarted)
        {
            logger.LogWarning(
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
