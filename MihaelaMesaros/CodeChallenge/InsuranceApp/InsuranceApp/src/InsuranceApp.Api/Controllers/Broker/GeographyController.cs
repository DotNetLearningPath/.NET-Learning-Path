using InsuranceApp.Api.Common;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.DTOs.Geography;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers.Broker;

/// <summary>
/// Controller for managing geographical data such as countries, counties, and cities.
/// </summary>
/// <param name="geographyService">The geography service.</param>
[ApiController]
[Route("api/brokers")]
public sealed class GeographyController(IGeographyService geographyService) : ControllerBase
{
    /// <summary>
    /// Gets all available countries.
    /// </summary>
    [HttpGet("countries")]
    [ProducesResponseType(typeof(IReadOnlyList<CountryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CountryDto>>> GetCountriesAsync(CancellationToken cancellationToken)
    {
        var countries = await geographyService.GetCountriesAsync(cancellationToken);

        return Ok(countries);
    }

    /// <summary>
    /// Gets all counties for the specified country.
    /// </summary>
    /// <param name="countryId">The country identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpGet("countries/{countryId:guid}/counties")]
    [ProducesResponseType(typeof(IReadOnlyList<CountyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CountyDto>>> GetCountiesAsync(Guid countryId, CancellationToken cancellationToken)
    {
        var countiesResult = await geographyService.GetCountiesByCountryAsync(countryId, cancellationToken);

        if (!countiesResult.IsSuccess)
        {
            return countiesResult.Error!.ToProblemResult();
        }

        return Ok(countiesResult.Value);
    }

    /// <summary>
    /// Gets all cities for the specified county.
    /// </summary>
    /// <param name="countyId">The county identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    [HttpGet("counties/{countyId:guid}/cities")]
    [ProducesResponseType(typeof(IReadOnlyList<CityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CityDto>>> GetCitiesAsync(Guid countyId, CancellationToken cancellationToken)
    {
        var citiesResult = await geographyService.GetCitiesByCountyAsync(countyId, cancellationToken);

        if (!citiesResult.IsSuccess)
        {
            return citiesResult.Error!.ToProblemResult();
        }

        return Ok(citiesResult.Value);
    }
}