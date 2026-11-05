using InsuranceManagement.Application.DTOs.Building;
using InsuranceManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BuildingsController(IBuildingService buildingService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<BuildingDto>>> GetAll() =>
            Ok(await buildingService.GetAllAsync());

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<BuildingDetailsDto>> GetById(Guid id)
        {
            var building = await buildingService.GetAsync(id);
            return building is null ? NotFound() : Ok(building);
        }

        [HttpPost]
        public async Task<ActionResult<BuildingDto>> Create(CreateBuildingDto createBuilding)
        {
            var building = await buildingService.CreateAsync(createBuilding);
            return CreatedAtAction(nameof(GetById), new { id = building.Id }, building);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<BuildingDto>> Update(Guid id, UpdateBuildingDto updateBuilding)
        {
            var building = await buildingService.UpdateAsync(id, updateBuilding);
            return building is null ? NotFound() : Ok(building);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) =>
            await buildingService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
