using FluentValidation;
using Microsoft.AspNetCore.Identity;
using SupportAPI.Application.Abstractions;
using SupportAPI.Application.Commands;
using SupportAPI.Application.Results;
using SupportAPI.Domain.Entities;

namespace SupportAPI.Application.CommandHandlers
{
    public class RegisterUserCommandHandler(
        UserManager<User> userManager,
        IValidator<RegisterUserCommand> validator
        ) : ICommandHandler<RegisterUserCommand, RegisterUserResult>
    {
        public async Task<RegisterUserResult> Handle(RegisterUserCommand command, CancellationToken cancellationToken)
        {
            await validator.ValidateAndThrowAsync(command, cancellationToken);

            var user = new User
            {
                UserName = command.Email,
                Name = command.Name,
                Email = command.Email,
            };

            var result = await userManager.CreateAsync(user, command.Password);
            
            if (!result.Succeeded){

                return new RegistrationFailed(result.Errors.Select(error => error.Description).ToList());
            }

            return new RegistrationSucceeded(user.Id);
        }
    }
}
