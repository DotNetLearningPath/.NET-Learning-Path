using InsuranceApp.Application.Common;
using Microsoft.AspNetCore.Mvc;
using Serilog.Core;

namespace InsuranceApp.Api.Common;

/// <summary>
/// Provides extension methods to map application errors to appropriate HTTP responses.
/// </summary>
public static class ErrorHttpMapper
{
    /// <summary>
    /// Maps an application error to an appropriate HTTP response based on the error type.
    /// </summary>
    /// <param name="error">The application error to be mapped.</param>
    /// <param name="logger">The logger to log the error.</param>
    /// <returns>An <see cref="ObjectResult"/> representing the HTTP response.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the error type is not supported.</exception>
    public static ObjectResult ToProblemResult(this Error error, ILogger? logger = null)
    {
        if (logger is not null)
        {
            LogExpectedError(error, logger);
        }


        return error.Type switch
        {
            ErrorType.Validation => new BadRequestObjectResult(CreateProblemDetails(StatusCodes.Status400BadRequest, "Validation failed", error)),

            ErrorType.NotFound => new NotFoundObjectResult(CreateProblemDetails(StatusCodes.Status404NotFound, "Resource not found", error)),

            ErrorType.Conflict => new ConflictObjectResult(CreateProblemDetails(StatusCodes.Status409Conflict, "Resource conflict", error)),

            _ => throw new InvalidOperationException($"Unsupported error type: {error.Type}.")
        };
    }

    private static ProblemDetails CreateProblemDetails(int status, string title, Error error)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = error.Description,
            Extensions =
            {
                ["code"] = error.Code
            }
        };
    }

    private static void LogExpectedError(Error error, ILogger logger)
    {
        switch (error.Type)
        {
            case ErrorType.Validation:
                logger.LogWarning(
                    "Validation failed. Code: {ErrorCode}. Description: {ErrorDescription}",
                    error.Code,
                    error.Description);
                break;

            case ErrorType.Conflict:
                logger.LogWarning(
                    "Request failed with {ErrorType}. Code: {ErrorCode}. Description: {ErrorDescription}",
                    error.Type,
                    error.Code,
                    error.Description);
                break;

            case ErrorType.NotFound:
                logger.LogInformation(
                    "Resource not found. Code: {ErrorCode}. Description: {ErrorDescription}",
                    error.Code,
                    error.Description);
                break;
        }
    }

}