using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.Interfaces;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CitiesController(ICityService cityService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await cityService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var city = await cityService.GetAsync(id);
        return city is null ? NotFound() : Ok(city);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCityDto createCity)
    {
        try
        {
            var city = await cityService.CreateAsync(createCity);
            return CreatedAtAction(nameof(GetById), new { id = city.Id }, city);
        }
        catch (ValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCityDto updateCity)
    {
        try
        {
            var city = await cityService.UpdateAsync(id, updateCity);
            return city is null ? NotFound() : Ok(city);
        }
        catch (ValidationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await cityService.DeleteAsync(id) ? NoContent() : NotFound();
}
