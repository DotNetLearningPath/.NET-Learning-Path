using InsuranceApp.Application.Buildings;
using InsuranceApp.WebApi.Mappings;
using InsuranceApp.WebApi.Models.Buildings;
using InsuranceApp.WebApi.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.WebApi.Controllers;

[ApiController]
[Route("api/brokers/buildings")]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
public class BuildingsController : ControllerBase
{
    private readonly IBuildingService _buildingService;

    public BuildingsController(IBuildingService buildingService)
    {
        ArgumentNullException.ThrowIfNull(buildingService);

        _buildingService = buildingService;
    }

    [HttpGet("/api/brokers/clients/{clientId}/buildings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResponse<BuildingResponse>>> GetBuildingsByClientId(
        [FromRoute] Guid clientId,
        [FromQuery] PageRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _buildingService.GetBuildingsByClientIdAsync(clientId, request.ToQuery(), cancellationToken);
        
        return Ok(result.ToResponse(building => building.ToResponse()));
    }

    [HttpGet("{buildingId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BuildingDetailsResponse>> GetBuildingDetailsById(
        [FromRoute] Guid buildingId,
        CancellationToken cancellationToken
    )
    {
        var result = await _buildingService.GetBuildingDetailsByIdAsync(buildingId, cancellationToken);
        
        return Ok(result.ToResponse());
    }

    [HttpPost("/api/brokers/clients/{clientId}/buildings")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BuildingDetailsResponse>> CreateBuilding(
        [FromRoute] Guid clientId,
        [FromBody] SaveBuildingRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _buildingService.CreateBuildingAsync(request.ToCreateCommand(clientId), cancellationToken);
        
        return CreatedAtAction(nameof(GetBuildingDetailsById), new { buildingId = result.Building.Id }, result.ToResponse());
    }

    [HttpPut("{buildingId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BuildingDetailsResponse>> UpdateBuilding(
        [FromRoute] Guid buildingId,
        [FromBody] SaveBuildingRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _buildingService.UpdateBuildingAsync(request.ToUpdateCommand(buildingId), cancellationToken);
        
        return Ok(result.ToResponse());
    }
}
