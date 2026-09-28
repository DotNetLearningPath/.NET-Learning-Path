using Application.Abstractions;
using Application.DTO.Buildings;
using Application.DTO.Common;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers;

[ApiController]
[Route("api/brokers")]
public class BuildingsController : ControllerBase
{
    private readonly IBuildingService _buildingService;

    public BuildingsController(IBuildingService buildingService)
    {
        _buildingService = buildingService;
    }

    [HttpGet("buildings/{buildingId:guid}", Name = nameof(GetBuildingByIdAsync))]
    public async Task<ActionResult<BuildingDto>> GetBuildingByIdAsync(Guid buildingId, CancellationToken cancellationToken)
    {
        var building = await _buildingService.GetBuildingByIdAsync(buildingId, cancellationToken);

        if (building is null)
        {
            return NotFound();
        }

        return Ok(building);
    }

    [HttpGet("clients/{clientId:guid}/buildings")]
    public async Task<ActionResult<PagedResult<BuildingDto>>>
        GetByClientIdAsync(
            Guid clientId,
            [FromQuery] PaginationRequest pagination,
            CancellationToken cancellationToken)
    {
        var buildings = await _buildingService.GetBuildingByClientIdAsync(
            clientId,
            pagination,
            cancellationToken);

        return Ok(buildings);
    }

    [HttpPost("clients/{clientId:guid}/buildings")]
    public async Task<ActionResult<BuildingDto>> CreateBuildingAsync(
        Guid clientId,
        CreateBuildingRequest request,
        CancellationToken cancellationToken)
    {
        request.ClientId = clientId;
        var building = await _buildingService.CreateBuildingAsync(request, cancellationToken);

        return CreatedAtRoute(
            nameof(GetBuildingByIdAsync),
            new { buildingId = building.Id },
            building);
    }

    [HttpPut("buildings/{buildingId:guid}")]
    public async Task<ActionResult<BuildingDto>> UpdateBuildingAsync(
        Guid buildingId,
        UpdateBuildingRequest request,
        CancellationToken cancellationToken)
    {
        var building = await _buildingService.UpdateBuildingAsync(buildingId, request, cancellationToken);

        return Ok(building);
    }
}
