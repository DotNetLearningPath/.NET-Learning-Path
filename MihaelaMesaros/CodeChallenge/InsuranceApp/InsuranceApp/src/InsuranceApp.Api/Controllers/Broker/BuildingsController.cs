using InsuranceApp.Api.Common;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.DTOs.Building;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers.Broker;

/// <summary>
/// Controller for managing buildings in the broker context.
/// </summary>
/// <param name="buildingService">The building service.</param>
[ApiController]
[Route("api/brokers")]
public sealed class BuildingsController(IBuildingService buildingService) : ControllerBase
{
    private const string GetBuildingByIdRouteName = "GetBuildingById";

    /// <summary>
    /// Gets a building by identifier.
    /// </summary>
    [HttpGet("buildings/{buildingId:guid}", Name = GetBuildingByIdRouteName)]
    [ProducesResponseType(typeof(BuildingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BuildingDto>> GetBuildingByIdAsync(Guid buildingId, CancellationToken cancellationToken)
    {
        var result = await buildingService.GetBuildingByIdAsync(buildingId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets all buildings for a specific client.
    /// </summary>
    /// <param name="clientId">The ID of the client.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of buildings for the specified client.</returns>
    [HttpGet("clients/{clientId:guid}/buildings")]
    [ProducesResponseType(typeof(IReadOnlyList<BuildingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<BuildingDto>>> GetBuildingsByClientAsync(Guid clientId, CancellationToken cancellationToken)
    {
        var result = await buildingService.GetBuildingsByClientAsync(clientId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new building for client.
    /// </summary>
    /// <param name="clientId">The ID of the client for whom the building is being created.</param>
    /// <param name="request">The building creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The created building.</returns>
    [HttpPost("clients/{clientId:guid}/buildings")]
    [ProducesResponseType(typeof(BuildingDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BuildingDto>> CreateBuildingForClientAsync(Guid clientId, CreateBuildingDto request, CancellationToken cancellationToken)
    {
        var result = await buildingService.CreateBuildingForClientAsync(clientId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult();
        }

        return CreatedAtRoute(GetBuildingByIdRouteName, new { buildingId = result.Value!.BuildingId }, result.Value);
    }

    /// <summary>
    /// Updates an existing building.
    /// </summary>
    /// <param name="buildingId">The ID of the building to update.</param>
    /// <param name="request">The building update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated building.</returns>
    [HttpPut("buildings/{buildingId:guid}")]
    [ProducesResponseType(typeof(BuildingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BuildingDto>> UpdateBuildingAsync(Guid buildingId, UpdateBuildingDto request, CancellationToken cancellationToken)
    {
        var result = await buildingService.UpdateBuildingAsync(buildingId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult();
        }

        return Ok(result.Value);
    }
}