using InsuranceApp.Application.Geography;
using InsuranceApp.WebApi.Mappings;
using InsuranceApp.WebApi.Models.Common;
using InsuranceApp.WebApi.Models.Geography;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.WebApi.Controllers;

[ApiController]
[Route("api/brokers")]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
public class GeographyController : ControllerBase
{
    private readonly IGeographyService _geographyService;

    public GeographyController(IGeographyService geographyService)
    {
        ArgumentNullException.ThrowIfNull(geographyService);

        _geographyService = geographyService;
    }

    [HttpGet("countries")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<CountryResponse>>> GetCountries(
        [FromQuery] PageRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _geographyService.GetCountriesAsync(request.ToQuery(), cancellationToken);
        
        return Ok(result.ToResponse(country => country.ToResponse()));
    }

    [HttpGet("countries/{countryId}/counties")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResponse<CountyResponse>>> GetCountiesByCountryId(
        [FromRoute] Guid countryId,
        [FromQuery] PageRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _geographyService.GetCountiesByCountryIdAsync(countryId, request.ToQuery(), cancellationToken);
        
        return Ok(result.ToResponse(county => county.ToResponse()));
    }

    [HttpGet("counties/{countyId}/cities")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResponse<CityResponse>>> GetCitiesByCountyId(
        [FromRoute] Guid countyId,
        [FromQuery] PageRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _geographyService.GetCitiesByCountyIdAsync(countyId, request.ToQuery(), cancellationToken);
        
        return Ok(result.ToResponse(city => city.ToResponse()));
    }
}
