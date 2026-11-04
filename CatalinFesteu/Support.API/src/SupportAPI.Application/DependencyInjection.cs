using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SupportAPI.Application.Abstractions;
using SupportAPI.Application.CommandHandlers;
using SupportAPI.Application.Commands;
using SupportAPI.Application.Queries;
using SupportAPI.Application.QueryHandlers;
using SupportAPI.Application.Results;
using SupportAPI.Application.Validations;

namespace SupportAPI.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateSupportRequestCommand>, CreateSupportRequestCommandValidator>();
        services.AddScoped<IValidator<GetSupportRequestByIdQuery>, GetSupportRequestByIdQueryValidator>();
        services.AddScoped<ICommandHandler<CreateSupportRequestCommand, SupportRequestResult>, CreateSupportRequestCommandHandler>();
        services.AddScoped<IQueryHandler<GetSupportRequestByIdQuery, SupportRequestResult>, GetSupportRequestByIdQueryHandler>();

        return services;
    }
}
