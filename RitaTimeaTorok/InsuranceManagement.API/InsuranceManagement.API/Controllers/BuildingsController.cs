using System.ComponentModel.DataAnnotations;
using InsuranceManagement.Application.DTOs.Building;
using InsuranceManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BuildingsController(IBuildingService buildingService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await buildingService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var building = await buildingService.GetAsync(id);
        return building is null ? NotFound() : Ok(building);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateBuildingDto createBuilding)
    {
        try
        {
            var building = await buildingService.CreateAsync(createBuilding);
            return CreatedAtAction(nameof(GetById), new { id = building.Id }, building);
        }
        catch (ValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateBuildingDto updateBuilding)
    {
        try
        {
            var building = await buildingService.UpdateAsync(id, updateBuilding);
            return building is null ? NotFound() : Ok(building);
        }
        catch (ValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await buildingService.DeleteAsync(id) ? NoContent() : NotFound();
}
