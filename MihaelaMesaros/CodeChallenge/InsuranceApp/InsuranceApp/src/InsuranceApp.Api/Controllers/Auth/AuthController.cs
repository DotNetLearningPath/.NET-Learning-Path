using InsuranceApp.Api.Common;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers.Auth;

/// <summary>
/// Provides API endpoints for user authentication.
/// </summary>
/// <param name="authService">Handles authentication operations and credential validation.</param>
/// <param name="logger">Logs authentication requests, outcomes, and operational events.</param>
[AllowAnonymous]
[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService, ILogger<AuthController> logger) : ControllerBase
{
    /// <summary>
    /// Authenticates a user and returns login details when the credentials are valid.
    /// </summary>
    /// <param name="loginDto">The login request data.</param>
    /// <param name="cancellationToken">Token when cancelling the authentication operation.</param>
    /// <returns>The bearer token when authentication succeeds; otherwise a ProblemDetails response for unauthorized or forbidden access.</returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto loginDto, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(loginDto, cancellationToken);

        if (!result.IsSuccess && result.Error != null)
        {
            return result.Error.ToProblemResult();
        }

        logger.LogInformation("User {Username} logged in successfully.", loginDto.Username);

        return Ok(result.Value);
    }
}
