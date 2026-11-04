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
        services.AddScoped<IValidator<CreateUserRequestCommand>, CreateUserRequestCommandValidator>();
        services.AddScoped<IValidator<GetUserRequestByIdQuery>, GetUserRequestByIdQueryValidator>();
        services.AddScoped<ICommandHandler<CreateUserRequestCommand, UserRequestResult>, CreateUserRequestCommandHandler>();
        services.AddScoped<IQueryHandler<GetUserRequestByIdQuery, UserRequestResult>, GetUserRequestByIdQueryHandler>();

        return services;
    }
}
