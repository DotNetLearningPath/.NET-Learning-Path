using InsuranceManagement.Application.Constants;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.API.Controllers
{
    [ApiController]
    [Route("api/brokers/counties")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class CountiesController(ICountyService countyService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<CountyDto>>> GetAll(
            [FromQuery, Range(PaginationLimits.MinimumPageNumber, PaginationLimits.MaximumPageNumber)]
                int pageNumber = PaginationLimits.DefaultPageNumber,
            [FromQuery, Range(PaginationLimits.MinimumPageSize, PaginationLimits.MaximumPageSize)]
                int pageSize = PaginationLimits.DefaultPageSize) =>
            Ok(await countyService.GetAllCountiesAsync(pageNumber, pageSize));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CountyDto>> GetById(Guid id)
        {
            var county = await countyService.GetCountyByIdAsync(id);
            return Ok(county);
        }

        [HttpPost("/api/admin/counties")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CountyDto>> Create(CreateCountyDto createCounty)
        {
            var county = await countyService.CreateCountyAsync(createCounty);
            return CreatedAtAction(nameof(GetById), new { id = county.Id }, county);
        }

        [HttpPut("/api/admin/counties/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CountyDto>> Update(Guid id, UpdateCountyDto updateCounty)
        {
            var county = await countyService.UpdateCountyAsync(id, updateCounty);
            return Ok(county);
        }

        [HttpDelete("/api/admin/counties/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await countyService.DeleteCountyAsync(id);
            return NoContent();
        }
    }
}
