using Insurance.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Insurance.Api.Controllers
{
    [ApiController]
    [Route("api/brokers")]
    public class GeographyController: ControllerBase
    {
        private readonly IGeographyService _geographyService;

        public GeographyController(IGeographyService geographyService)
        {
            _geographyService = geographyService;
        }

        [HttpGet("countries")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCountriesAsync(CancellationToken cancellationToken)
        {
            var countries = await _geographyService.GetCountriesAsync(cancellationToken);

            return Ok(countries.Select(country => new
            {
                country.Id,
                country.Name
            }).ToList());
        }

        [HttpGet("countries/{countryId:guid}/counties")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCountiesByCountryIdAsync(Guid countryId, CancellationToken cancellationToken)
        {
            var counties = await _geographyService.GetCountiesByCountryIdAsync(countryId, cancellationToken);

            return Ok(counties.Select(county => new
            {
                county.Id,
                county.Name,
                county.CountryId
            }).ToList());
        }

        [HttpGet("counties/{countyId:guid}/cities")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCitiesByCountyIdAsync(Guid countyId, CancellationToken cancellationToken)
        {
            var cities = await _geographyService.GetCitiesByCountyIdAsync(countyId, cancellationToken);
            return Ok(cities.Select(city => new
            {
                city.Id,
                city.Name,
                city.CountyId
            }).ToList());
        }
    }
}