using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace InsuranceApp.Api.ExceptionHandling;

/// <summary>
/// A global exception handler that logs unhandled exceptions and returns a standardized error response.
/// </summary>
/// <param name="logger">The logger instance.</param>
public sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    /// <summary>
    /// Attempts to handle an unhandled exception by logging it and returning a standardized error response.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <param name="exception">The exception to handle.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A value indicating whether the exception was handled.</returns>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException && cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Request {Method} {Path} was cancelled.", httpContext.Request.Method, httpContext.Request.Path);
            return false;
        }

        var stackTrace = new StackTrace(exception, true);

        var frame = stackTrace.GetFrames()?.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.GetFileName()));

        var fileName = frame?.GetFileName() is { } filePath ? Path.GetFileName(filePath) : "Unknown";

        var lineNumber = frame?.GetFileLineNumber() ?? 0;

        logger.LogError(
            "Unhandled exception - Type: {ExceptionType} - Message: {ExceptionMessage} \n"
            + "Request: {Method} {Path} - Source: {Source} \n"
            + "InnerException: {InnerException} - File: {File} Line: {Line} \n",
            exception.GetType().Name,
            exception.Message,
            httpContext.Request.Method,
            httpContext.Request.Path,
            exception.Source ?? "Unknown",
            exception.InnerException?.Message ?? "None",
            fileName,
            lineNumber
        );

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "An unexpected error occurred.",
            Detail = "An unexpected error occurred while processing the request."
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}