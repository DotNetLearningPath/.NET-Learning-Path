using InsuranceApp.Api.Common;
using InsuranceApp.Api.Security;
using InsuranceApp.Application.Abstractions.Security;
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
/// <param name="refreshTokenCookieService">Handles operations for refresh token cookie.</param>
/// <param name="logger">Logs authentication requests, outcomes, and operational events.</param>
[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService, IRefreshTokenCookieService refreshTokenCookieService, ILogger<AuthController> logger) : ControllerBase
{
    /// <summary>
    /// Authenticates a user and returns login details when the credentials are valid.
    /// </summary>
    /// <param name="loginDto">The login request data.</param>
    /// <param name="cancellationToken">Token when cancelling the authentication operation.</param>
    /// <returns>The bearer token when authentication succeeds; otherwise a ProblemDetails response for unauthorized or forbidden access.</returns>
    [AllowAnonymous]
    [ServiceFilter(typeof(ValidateOriginFilter))]
    [HttpPost("login")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(AuthTokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthTokenResponseDto>> Login([FromBody] LoginDto loginDto, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(loginDto, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult();
        }

        refreshTokenCookieService.SetRefreshTokenCookie(result.Value!.RefreshToken, result.Value.RefreshTokenExpiresAt);

        logger.LogInformation("User {Username} logged in successfully.", loginDto.Username);

        return Ok(new AuthTokenResponseDto(result.Value.AccessToken, result.Value.ExpiresAt));
    }

    /// <summary>
    /// Refreshes authentication tokens using the refresh token stored in an HttpOnly cookie.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation.</param>
    /// <returns>A new access token when refresh succeeds.</returns>
    [AllowAnonymous]
    [ServiceFilter(typeof(ValidateOriginFilter))]
    [HttpPost("refresh")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    [ProducesResponseType(typeof(AuthTokenResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthTokenResponseDto>> RefreshToken(CancellationToken cancellationToken)
    {
        var refreshToken = refreshTokenCookieService.GetRefreshTokenCookie();

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized();
        }

        var result = await authService.RefreshTokenAsync(new RefreshTokenDto(refreshToken), cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult();
        }

        refreshTokenCookieService.SetRefreshTokenCookie(result.Value!.RefreshToken, result.Value.RefreshTokenExpiresAt);

        return Ok(new AuthTokenResponseDto(result.Value.AccessToken, result.Value.ExpiresAt));
    }

    /// <summary>
    /// Logs out the authenticated user identified by the subject claim.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the asynchronous operation.</param>
    /// <returns>An <see cref="IActionResult"/> that returns 204 No Content when logout succeeds, 401 Unauthorized when the user
    /// identifier is invalid, or an error response when logout fails.</returns>
    [Authorize]
    [ServiceFilter(typeof(ValidateOriginFilter))]
    [HttpPost("logout-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> LogoutAll(CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var result = await authService.LogoutAllAsync(userId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult();
        }

        refreshTokenCookieService.DeleteRefreshTokenCookie();

        return NoContent();
    }

    /// <summary>
    /// Revokes a refresh token to end the associated session.
    /// </summary>
    [AllowAnonymous]
    [ServiceFilter(typeof(ValidateOriginFilter))]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = refreshTokenCookieService.GetRefreshTokenCookie();

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized();
        }

        var result = await authService.LogoutAsync(new RefreshTokenDto(refreshToken), cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult();
        }

        refreshTokenCookieService.DeleteRefreshTokenCookie();

        return NoContent();
    }

}
