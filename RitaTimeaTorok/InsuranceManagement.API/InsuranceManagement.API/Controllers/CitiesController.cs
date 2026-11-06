using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.API.Controllers
{
    [ApiController]
    [Route("api/brokers/cities")]
    public class CitiesController(ICityService cityService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResult<CityDto>>> GetAll(
            [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
            [FromQuery, Range(1, 100)] int pageSize = 10) =>
            Ok(await cityService.GetAllCitiesAsync(pageNumber, pageSize));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CityDto>> GetById(Guid id)
        {
            var city = await cityService.GetCityByIdAsync(id);
            return Ok(city);
        }

        [HttpPost("/api/admin/cities")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CityDto>> Create(CreateCityDto createCity)
        {
            var city = await cityService.CreateCityAsync(createCity);
            return CreatedAtAction(nameof(GetById), new { id = city.Id }, city);
        }

        [HttpPut("/api/admin/cities/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CityDto>> Update(Guid id, UpdateCityDto updateCity)
        {
            var city = await cityService.UpdateCityAsync(id, updateCity);
            return Ok(city);
        }

        [HttpDelete("/api/admin/cities/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await cityService.DeleteCityAsync(id);
            return NoContent();
        }
    }
}
