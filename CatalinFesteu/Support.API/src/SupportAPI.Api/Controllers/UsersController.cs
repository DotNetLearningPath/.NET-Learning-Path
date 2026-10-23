using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SupportAPI.Api.Requests;
using SupportAPI.Application.Abstractions;
using SupportAPI.Application.Commands;
using SupportAPI.Application.Results;

namespace SupportAPI.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController(ICommandHandler<RegisterUserCommand, RegisterUserResult> commandHandler) : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Create(RegisterUserRequest request, CancellationToken cancellationToken)
        {
            var command = new RegisterUserCommand(request.Name, request.Email, request.Password);
            
            var result = await commandHandler.Handle(command, cancellationToken);

            return result switch
            {
                RegistrationSucceeded succeeded => StatusCode(StatusCodes.Status201Created, succeeded.UserId),
                RegistrationFailed failed => StatusCode(StatusCodes.Status400BadRequest, failed.Errors),
                _ => throw new InvalidOperationException()
            };
            
        }
    }
}
