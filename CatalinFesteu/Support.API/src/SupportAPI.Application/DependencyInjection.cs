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
        services.AddScoped<IValidator<CreateUserRequestCommand>, CreateRequestCommandValidator>();
        services.AddScoped<IValidator<GetRequestByIdQuery>, GetRequestByIdQueryValidator>();
        services.AddScoped<ICommandHandler<CreateUserRequestCommand, UserRequestResult>, CreateRequestCommandHandler>();
        services.AddScoped<IQueryHandler<GetRequestByIdQuery, UserRequestResult>, GetRequestByIdQueryHandler>();

        return services;
    }
}
