using System.Diagnostics;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace SupportAPI.Api;

public sealed class GlobalErrorHandlingMiddleware(
    RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            if (!context.Response.HasStarted)
            {
                context.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
            }
        }
        catch (Exception exception) when (!context.Response.HasStarted)
        {
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            ProblemDetails problem;

            if (exception is ValidationException validationException)
            {
                var errors = validationException.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).Distinct().ToArray());

                problem = new HttpValidationProblemDetails(errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred."
                };

            }
            else if (exception is BadHttpRequestException badRequestException)
            {
                problem = new ProblemDetails
                {
                    Status = badRequestException.StatusCode,
                    Title = "The HTTP request could not be processed."
                };
            }
            else
            {
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred."
                };
            }

            problem.Instance = context.Request.Path;
            problem.Extensions["traceId"] = traceId;

            context.Response.Clear();
            context.Response.StatusCode = problem.Status!.Value;
           
            await context.Response.WriteAsJsonAsync(
                (object)problem,
                options: null,
                contentType: "application/problem+json",
                cancellationToken: context.RequestAborted);
        }
    }
}
