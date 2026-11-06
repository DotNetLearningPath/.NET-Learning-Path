using InsuranceManagement.Application.Constants;
using InsuranceManagement.Application.DTOs.Building;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.API.Controllers
{
    [ApiController]
    [Route("api/brokers/buildings")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class BuildingsController(IBuildingService buildingService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<BuildingDto>>> GetAll(
            [FromQuery, Range(PaginationLimits.MinimumPageNumber, PaginationLimits.MaximumPageNumber)]
                int pageNumber = PaginationLimits.DefaultPageNumber,
            [FromQuery, Range(PaginationLimits.MinimumPageSize, PaginationLimits.MaximumPageSize)]
                int pageSize = PaginationLimits.DefaultPageSize) =>
            Ok(await buildingService.GetAllBuildingsAsync(pageNumber, pageSize));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BuildingDetailsDto>> GetById(Guid id)
        {
            var building = await buildingService.GetBuildingByIdAsync(id);
            return Ok(building);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<BuildingDto>> Create(CreateBuildingDto createBuilding)
        {
            var building = await buildingService.CreateBuildingAsync(createBuilding);
            return CreatedAtAction(nameof(GetById), new { id = building.Id }, building);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BuildingDto>> Update(Guid id, UpdateBuildingDto updateBuilding)
        {
            var building = await buildingService.UpdateBuildingAsync(id, updateBuilding);
            return Ok(building);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await buildingService.DeleteBuildingAsync(id);
            return NoContent();
        }
    }
}
