using InsuranceManagement.Application.Interfaces;
using InsuranceManagement.Application.DTOs.Geography;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CountriesController(ICountryService countryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await countryService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var country = await countryService.GetAsync(id);
        return country is null ? NotFound() : Ok(country);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCountryDto createCountry)
    {
        var country = await countryService.CreateAsync(createCountry);
        return CreatedAtAction(nameof(GetById), new { id = country.Id }, country);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCountryDto updateCountry)
    {
        var country = await countryService.UpdateAsync(id, updateCountry);
        return country is null ? NotFound() : Ok(country);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await countryService.DeleteAsync(id) ? NoContent() : NotFound();
}
