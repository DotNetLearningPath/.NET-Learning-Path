using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportAPI.Api.Requests;
using SupportAPI.Application.Abstractions;
using SupportAPI.Application.Commands;
using SupportAPI.Application.Results;

namespace SupportAPI.Api.Controllers
{
    [ApiController]
    [Route("api/auth/")]
    public class LoginController(
        ICommandHandler<LoginCommand, LoginResult> commandHandler) : ControllerBase
    {
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([
            FromBody] LoginRequest request, 
            CancellationToken cancellationToken)
        {
            var command = new LoginCommand(request.Email, request.Password);
            var result = await commandHandler.Handle(command, cancellationToken);
            return result switch
            {
                LoginSucceeded success => Ok(success),
                InvalidCredentials => Unauthorized(),
                _ => throw new InvalidOperationException() // to enhance
            };
        }
    }
}
