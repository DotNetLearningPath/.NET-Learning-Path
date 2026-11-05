using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitiesController(ICityService cityService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<CityDto>>> GetAll() =>
        Ok(await cityService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CityDto>> GetById(Guid id)
    {
        var city = await cityService.GetAsync(id);
        return city is null ? NotFound() : Ok(city);
    }

    [HttpPost]
    public async Task<ActionResult<CityDto>> Create(CreateCityDto createCity)
    {
        var city = await cityService.CreateAsync(createCity);
        return CreatedAtAction(nameof(GetById), new { id = city.Id }, city);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CityDto>> Update(Guid id, UpdateCityDto updateCity)
    {
        var city = await cityService.UpdateAsync(id, updateCity);
        return city is null ? NotFound() : Ok(city);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await cityService.DeleteAsync(id) ? NoContent() : NotFound();
}
