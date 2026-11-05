using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CountiesController(ICountyService countyService) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<CountyDto>>> GetAll() =>
            Ok(await countyService.GetAllAsync());

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CountyDto>> GetById(Guid id)
        {
            var county = await countyService.GetAsync(id);
            return county is null ? NotFound() : Ok(county);
        }

        [HttpPost]
        public async Task<ActionResult<CountyDto>> Create(CreateCountyDto createCounty)
        {
            var county = await countyService.CreateAsync(createCounty);
            return CreatedAtAction(nameof(GetById), new { id = county.Id }, county);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<CountyDto>> Update(Guid id, UpdateCountyDto updateCounty)
        {
            var county = await countyService.UpdateAsync(id, updateCounty);
            return county is null ? NotFound() : Ok(county);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id) =>
            await countyService.DeleteAsync(id) ? NoContent() : NotFound();
    }
}
