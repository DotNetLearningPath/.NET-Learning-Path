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
    }
}
