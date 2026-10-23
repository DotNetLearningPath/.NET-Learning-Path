using Microsoft.AspNetCore.Identity;
using SupportAPI.Application.Abstractions;
using SupportAPI.Application.Commands;
using SupportAPI.Application.Results;
using SupportAPI.Domain.Entities;

namespace SupportAPI.Application.CommandHandlers
{
    public class LoginCommandHandler(
        UserManager<User> userManager,
        IAccessTokenGenerator accessTokenGenerator
        ) : ICommandHandler<LoginCommand, LoginResult>
    {
        public async Task<LoginResult> Handle(LoginCommand command, CancellationToken cancellationToken)
        {
            var user = await userManager.FindByEmailAsync(command.Email);

            if (user is null || !await userManager.CheckPasswordAsync(user, command.Password))
            {
                return new InvalidCredentials();
            }
            var token = accessTokenGenerator.Create(user.Id, user.Name);

            return new LoginSucceeded(token.AccessToken, token.ExpiresAt);
        }
    }
}
