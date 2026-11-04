using System.ComponentModel.DataAnnotations;
using InsuranceManagement.Application.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace InsuranceManagement.API.Middleware
{
    public class ApiExceptionHandler(
        ILogger<ApiExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext, 
            Exception exception, 
            CancellationToken cancellationToken)
        {
            int statusCode;
            string message;

            switch (exception)
            {
                case ValidationException:
                    statusCode = StatusCodes.Status400BadRequest;
                    message = exception.Message;
                    break;

                case ConflictException:
                    statusCode = StatusCodes.Status409Conflict;
                    message = exception.Message;
                    break;

                default:
                    statusCode = StatusCodes.Status500InternalServerError;
                    message = "An unexpected error occurred.";
                    break;
            };

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception,
                    "An unexpected error occurred while processing {Path}",
                    httpContext.Request.Path);
            }
            else
            {
                logger.LogWarning(exception,
                    "Handled request failure with status code {StatusCode} while processing {Path}",
                    statusCode,
                    httpContext.Request.Path);
            }

            httpContext.Response.StatusCode = statusCode;

            await Results.Problem(
                statusCode: statusCode,
                detail: message
            ).ExecuteAsync(httpContext);

            return true;
        }
    }
}
