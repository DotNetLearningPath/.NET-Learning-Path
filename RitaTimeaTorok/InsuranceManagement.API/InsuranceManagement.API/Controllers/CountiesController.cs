using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.API.Controllers
{
    [ApiController]
    [Route("api/brokers/counties")]
    public class CountiesController(ICountyService countyService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResult<CountyDto>>> GetAll(
            [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
            [FromQuery, Range(1, 100)] int pageSize = 10) =>
            Ok(await countyService.GetAllCountiesAsync(pageNumber, pageSize));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CountyDto>> GetById(Guid id)
        {
            var county = await countyService.GetCountyByIdAsync(id);
            return Ok(county);
        }

        [HttpPost("/api/admin/counties")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CountyDto>> Create(CreateCountyDto createCounty)
        {
            var county = await countyService.CreateCountyAsync(createCounty);
            return CreatedAtAction(nameof(GetById), new { id = county.Id }, county);
        }

        [HttpPut("/api/admin/counties/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CountyDto>> Update(Guid id, UpdateCountyDto updateCounty)
        {
            var county = await countyService.UpdateCountyAsync(id, updateCounty);
            return Ok(county);
        }

        [HttpDelete("/api/admin/counties/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await countyService.DeleteCountyAsync(id);
            return NoContent();
        }
    }
}
