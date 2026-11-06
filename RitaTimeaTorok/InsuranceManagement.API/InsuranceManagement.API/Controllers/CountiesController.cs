using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceManagement.API.Controllers
{
    [ApiController]
    [Route("api/brokers/counties")]
    public class CountiesController(ICountyService countyService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<CountyDto>>> GetAll() =>
            Ok(await countyService.GetAllAsync());

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CountyDto>> GetById(Guid id)
        {
            var county = await countyService.GetAsync(id);
            return Ok(county);
        }

        [HttpPost("/api/admin/counties")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CountyDto>> Create(CreateCountyDto createCounty)
        {
            var county = await countyService.CreateAsync(createCounty);
            return CreatedAtAction(nameof(GetById), new { id = county.Id }, county);
        }

        [HttpPut("/api/admin/counties/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CountyDto>> Update(Guid id, UpdateCountyDto updateCounty)
        {
            var county = await countyService.UpdateAsync(id, updateCounty);
            return Ok(county);
        }

        [HttpDelete("/api/admin/counties/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await countyService.DeleteAsync(id);
            return NoContent();
        }
    }
}
