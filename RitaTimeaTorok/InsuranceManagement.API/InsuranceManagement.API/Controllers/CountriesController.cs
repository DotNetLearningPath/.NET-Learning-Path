using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.DTOs.Pagination;
using Microsoft.AspNetCore.Mvc;
using InsuranceManagement.Application.Interfaces.Services;
using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.API.Controllers
{
    [ApiController]
    [Route("api/brokers/countries")]
    public class CountriesController(ICountryService countryService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResult<CountryDto>>> GetAll(
            [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
            [FromQuery, Range(1, 100)] int pageSize = 10) =>
            Ok(await countryService.GetAllCountriesAsync(pageNumber, pageSize));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CountryDto>> GetById(Guid id)
        {
            var country = await countryService.GetCountryByIdAsync(id);
            return Ok(country);
        }

        [HttpPost("/api/admin/countries")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CountryDto>> Create(CreateCountryDto createCountry)
        {
            var country = await countryService.CreateCountryAsync(createCountry);
            return CreatedAtAction(nameof(GetById), new { id = country.Id }, country);
        }

        [HttpPut("/api/admin/countries/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CountryDto>> Update(Guid id, UpdateCountryDto updateCountry)
        {
            var country = await countryService.UpdateCountryAsync(id, updateCountry);
            return Ok(country);
        }

        [HttpDelete("/api/admin/countries/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await countryService.DeleteCountryAsync(id);
            return NoContent();
        }
    }
}
