using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Buildings;
using Insurance.Application.DTO.Common;
using Microsoft.AspNetCore.Mvc;

namespace Insurance.Api.Controllers;

[ApiController]
[Route("api/brokers")]
public class BuildingsController(IBuildingService buildingService) : ControllerBase
{
    [HttpGet("buildings/{buildingId:guid}", Name = nameof(GetBuildingByIdAsync))]
    public async Task<ActionResult<BuildingDto>> GetBuildingByIdAsync(Guid buildingId, CancellationToken cancellationToken)
    {
        var building = await buildingService.GetBuildingByIdAsync(buildingId, cancellationToken);

        if (building is null)
        {
            return NotFound();
        }

        return Ok(building);
    }

    [HttpGet("clients/{clientId:guid}/buildings")]
    public async Task<ActionResult<PagedResult<BuildingDto>>> GetByClientIdAsync(
            Guid clientId,
            [FromQuery] PaginationRequest pagination,
            CancellationToken cancellationToken)
    {
        var buildings = await buildingService.GetBuildingByClientIdAsync(
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
        var building = await buildingService.CreateBuildingAsync(request, cancellationToken);

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
        var building = await buildingService.UpdateBuildingAsync(buildingId, request, cancellationToken);

        return Ok(building);
    }
}
