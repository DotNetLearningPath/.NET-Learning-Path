using Application.Abstractions;
using Application.DTO.Geography;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers;

[ApiController]
[Route("api/brokers")]
public class GeographyController : ControllerBase
{
    private readonly IGeographyService _geographyService;

    public GeographyController(IGeographyService geographyService)
    {
        _geographyService = geographyService;
    }

    [HttpGet("countries")]
    public async Task<ActionResult<IReadOnlyCollection<CountryDto>>>
        GetCountriesAsync(CancellationToken cancellationToken)
    {
        var countries = await _geographyService.GetCountriesAsync(cancellationToken);

        return Ok(countries);
    }

    [HttpGet("countries/{countryId:guid}/counties")]
    public async Task<ActionResult<IReadOnlyCollection<CountyDto>>>
        GetCountiesByCountryIdAsync(Guid countryId, CancellationToken cancellationToken)
    {
        var counties = await _geographyService
            .GetCountiesByCountryIdAsync(countryId, cancellationToken);

        return Ok(counties);
    }

    [HttpGet("counties/{countyId:guid}/cities")]
    public async Task<ActionResult<IReadOnlyCollection<CityDto>>>
        GetCitiesByCountyIdAsync(Guid countyId, CancellationToken cancellationToken)
    {
        var cities = await _geographyService
            .GetCitiesByCountyIdAsync(countyId, cancellationToken);

        return Ok(cities);
    }
}
