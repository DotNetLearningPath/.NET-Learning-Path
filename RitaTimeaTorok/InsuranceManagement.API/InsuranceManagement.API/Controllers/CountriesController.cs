using InsuranceManagement.Application.DTOs.Geography;
using Microsoft.AspNetCore.Mvc;
using InsuranceManagement.Application.Interfaces.Services;

namespace InsuranceManagement.API.Controllers
{
    [ApiController]
    [Route("api/brokers/countries")]
    public class CountriesController(ICountryService countryService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<List<CountryDto>>> GetAll() =>
            Ok(await countryService.GetAllAsync());

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CountryDto>> GetById(Guid id)
        {
            var country = await countryService.GetAsync(id);
            return Ok(country);
        }

        [HttpPost("/api/admin/countries")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CountryDto>> Create(CreateCountryDto createCountry)
        {
            var country = await countryService.CreateAsync(createCountry);
            return CreatedAtAction(nameof(GetById), new { id = country.Id }, country);
        }

        [HttpPut("/api/admin/countries/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CountryDto>> Update(Guid id, UpdateCountryDto updateCountry)
        {
            var country = await countryService.UpdateAsync(id, updateCountry);
            return Ok(country);
        }

        [HttpDelete("/api/admin/countries/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(Guid id)
        {
            await countryService.DeleteAsync(id);
            return NoContent();
        }
    }
}
