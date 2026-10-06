using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Geography;
using Microsoft.AspNetCore.Mvc;

namespace Insurance.Api.Controllers;

[ApiController]
[Route("api/brokers")]
public class GeographyController(IGeographyService geographyService) : ControllerBase
{
    [HttpGet("countries")]
    public async Task<ActionResult<List<CountryDto>>> GetCountriesAsync(CancellationToken cancellationToken)
    {
        var countries = await geographyService.GetCountriesAsync(cancellationToken);

        return Ok(countries);
    }

    [HttpGet("countries/{countryId:guid}/counties")]
    public async Task<ActionResult<List<CountyDto>>> GetCountiesByCountryIdAsync(Guid countryId, CancellationToken cancellationToken)
    {
        var counties = await geographyService
            .GetCountiesByCountryIdAsync(countryId, cancellationToken);

        return Ok(counties);
    }

    [HttpGet("counties/{countyId:guid}/cities")]
    public async Task<ActionResult<List<CityDto>>> GetCitiesByCountyIdAsync(Guid countyId, CancellationToken cancellationToken)
    {
        var cities = await geographyService
            .GetCitiesByCountyIdAsync(countyId, cancellationToken);

        return Ok(cities);
    }
}
