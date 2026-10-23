using InsuranceApp.Api.Common;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers.Admin;

/// <summary>
/// Endpoints for users management.
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/users")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    private const string GetUserByIdRouteName = "GetUserByIdAsync";

    /// <summary>
    /// Creates a new user with the specified role.
    /// </summary>
    /// <param name="createUserDto">User creation details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>HTTP 201 when the user is created successfully.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUserAsync([FromBody] CreateUserDto createUserDto, CancellationToken cancellationToken)
    {
        var result = await userService.CreateUserAsync(createUserDto, cancellationToken);

        if (!result.IsSuccess && result.Error != null)
        {
            return result.Error.ToProblemResult();
        }

        return CreatedAtRoute(GetUserByIdRouteName, new { userId = result.Value!.UserId }, result.Value);
    }

    /// <summary>
    /// Retrieves a user by their unique identifier.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user details.</returns>
    [HttpGet("{userId:guid}", Name = GetUserByIdRouteName)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await userService.GetUserByIdAsync(userId, cancellationToken);

        if (!result.IsSuccess && result.Error != null)
        {
            return result.Error.ToProblemResult();
        }

        return Ok(result.Value);
    }
}