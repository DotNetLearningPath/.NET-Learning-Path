using Insurance.Api.DTOs;
using Insurance.Application.Interfaces;
using Insurance.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Insurance.Api.Controllers
{
    [ApiController]
    [Route("api/brokers")]
    public class BuildingsController : ControllerBase
    {
        private readonly IBuildingService _buildingService;
        
        public BuildingsController(IBuildingService buildingService)
        {
            _buildingService = buildingService;
        }

        [HttpPost("clients/{clientId:guid}/buildings")]
        [ProducesResponseType(typeof(BuildingResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateAsync(Guid clientId, [FromBody] CreateBuildingDto request, CancellationToken cancellationToken)
        {
            var building = new Building
            {
                ClientId = clientId,
                Street = request.Street,
                StreetNumber = request.StreetNumber,
                CityId = request.CityId,
                ConstructionYear = request.ConstructionYear,
                BuildingType = request.BuildingType,
                NumberOfFloors = request.NumberOfFloors,
                SurfaceArea = request.SurfaceArea,
                InsuredValue = request.InsuredValue
            };

            var createdBuilding = await _buildingService.CreateAsync(building, cancellationToken);

            return CreatedAtRoute("GetBuildingById", new { id = createdBuilding.Id }, ToResponse(createdBuilding));
        }

        [HttpGet("buildings/{id:guid}", Name = "GetBuildingById")]
        [ProducesResponseType(typeof(BuildingResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var building = await _buildingService.GetByIdAsync(id, cancellationToken);

            if (building == null)
            {
                return NotFound();
            }

            return Ok(ToResponse(building));
        }

        [HttpGet("clients/{clientId:guid}/buildings")]
        [ProducesResponseType(typeof(List<BuildingResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetByClientIdAsync(Guid clientId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var buildings = await _buildingService.GetByClientIdAsync(
                clientId,
                page,
                pageSize,
                cancellationToken);

            return Ok(buildings.Select(ToResponse).ToList());
        }

        [HttpPut("buildings/{id:guid}")]
        [ProducesResponseType(typeof(BuildingResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateBuildingDto request, CancellationToken cancellationToken)
        {
            var building = new Building
            {
                Id = id,
                Street = request.Street,
                StreetNumber = request.StreetNumber,
                CityId = request.CityId,
                ConstructionYear = request.ConstructionYear,
                BuildingType = request.BuildingType,
                NumberOfFloors = request.NumberOfFloors,
                SurfaceArea = request.SurfaceArea,
                InsuredValue = request.InsuredValue
            };

            var updatedBuilding = await _buildingService.UpdateAsync(building, cancellationToken);

            if (updatedBuilding == null)
            {
                return NotFound();
            }

            return Ok(ToResponse(updatedBuilding));
        }
        private static BuildingResponseDto ToResponse(Building building)
        {
            return new BuildingResponseDto
            {
                Id = building.Id,
                ClientId = building.ClientId,
                Street = building.Street,
                StreetNumber = building.StreetNumber,
                CityId = building.CityId,
                City = building.City?.Name,
                County = building.City?.County?.Name,
                Country = building.City?.County?.Country?.Name,
                ConstructionYear = building.ConstructionYear,
                BuildingType = building.BuildingType,
                NumberOfFloors = building.NumberOfFloors,
                SurfaceArea = building.SurfaceArea,
                InsuredValue = building.InsuredValue
            };
        }
    }
}
