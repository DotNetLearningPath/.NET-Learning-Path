using InsuranceManagement.Application.DTOs.Building;
using InsuranceManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceManagement.API.Controllers
{
    [ApiController]
    [Route("api/brokers/buildings")]
    public class BuildingsController(IBuildingService buildingService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<BuildingDto>>> GetAll() =>
            Ok(await buildingService.GetAllAsync());

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BuildingDetailsDto>> GetById(Guid id)
        {
            var building = await buildingService.GetAsync(id);
            return building is null ? NotFound() : Ok(building);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BuildingDto>> Create(CreateBuildingDto createBuilding)
        {
            var building = await buildingService.CreateAsync(createBuilding);
            return CreatedAtAction(nameof(GetById), new { id = building.Id }, building);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<BuildingDto>> Update(Guid id, UpdateBuildingDto updateBuilding)
        {
            var building = await buildingService.UpdateAsync(id, updateBuilding);
            return building is null ? NotFound() : Ok(building);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id) =>
            await buildingService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
