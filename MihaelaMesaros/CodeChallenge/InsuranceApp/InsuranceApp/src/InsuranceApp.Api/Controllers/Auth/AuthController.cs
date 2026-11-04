using InsuranceApp.Api.Common;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace InsuranceApp.Api.Controllers.Auth;

/// <summary>
/// Provides API endpoints for user authentication.
/// </summary>
/// <param name="authService">Handles authentication operations and credential validation.</param>
/// <param name="logger">Logs authentication requests, outcomes, and operational events.</param>
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
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
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

    /// <summary>
    /// Refreshes authentication tokens using the provided refresh token.
    /// </summary>
    /// <param name="refreshTokenDto">Refresh token request data used to validate and issue new authentication tokens.</param>
    /// <param name="cancellationToken">Token that propagates notification that the request should be canceled.</param>
    /// <returns>An ActionResult containing a LoginResponseDto when refresh succeeds; otherwise an error response for unauthorized or forbidden access.</returns>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken)
    {
        var result = await authService.RefreshTokenAsync(refreshTokenDto, cancellationToken);

        if (!result.IsSuccess && result.Error != null)
        {
            return result.Error.ToProblemResult();
        }

        Response.Headers.CacheControl = "no-store";

        return Ok(result.Value);
    }

    /// <summary>
    /// Logs out the authenticated user identified by the subject claim.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    /// <returns>An <see cref="IActionResult"/> that returns 204 No Content when logout succeeds, 401 Unauthorized when the user
    /// identifier is invalid, or an error response when logout fails.</returns>
    [Authorize]
    [HttpPost("logout-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> LogoutAll(
    CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await authService.LogoutAllAsync(userId, cancellationToken);

        if (!result.IsSuccess && result.Error != null)
        {
            return result.Error.ToProblemResult();
        }

        return NoContent();
    }

    /// <summary>
    /// Revokes a refresh token to end the associated session.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken)
    {
        var result = await authService.LogoutAsync(refreshTokenDto, cancellationToken);

        if (!result.IsSuccess && result.Error != null)
        {
            return result.Error.ToProblemResult();
        }

        return NoContent();
    }

}
