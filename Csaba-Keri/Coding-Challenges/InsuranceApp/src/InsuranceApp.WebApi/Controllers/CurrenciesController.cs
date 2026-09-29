using InsuranceApp.Application.Currencies;
using InsuranceApp.WebApi.Mappings;
using InsuranceApp.WebApi.Models.Common;
using InsuranceApp.WebApi.Models.Currencies;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.WebApi.Controllers;

[ApiController]
[Route("api/admin/currencies")]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
public class CurrenciesController : ControllerBase
{
    private readonly ICurrencyService _currencyService;

    public CurrenciesController(ICurrencyService currencyService)
    {
        ArgumentNullException.ThrowIfNull(currencyService);

        _currencyService = currencyService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<CurrencyResponse>>> GetCurrencies(
        [FromQuery] PageRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _currencyService.GetCurrenciesAsync(request.ToQuery(), cancellationToken);
        
        return Ok(result.ToResponse(currency => currency.ToResponse()));
    }

    [HttpGet("{currencyId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CurrencyResponse>> GetCurrencyById(
        [FromRoute] Guid currencyId,
        CancellationToken cancellationToken
    )
    {
        var result = await _currencyService.GetCurrencyByIdAsync(currencyId, cancellationToken);
        
        return Ok(result.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CurrencyResponse>> CreateCurrency(
        [FromBody] CreateCurrencyRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _currencyService.CreateCurrencyAsync(request.ToCommand(), cancellationToken);
        
        return CreatedAtAction(nameof(GetCurrencyById), new { currencyId = result.Id }, result.ToResponse());
    }

    [HttpPut("{currencyId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CurrencyResponse>> UpdateCurrency(
        [FromRoute] Guid currencyId,
        [FromBody] UpdateCurrencyRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _currencyService.UpdateCurrencyAsync(request.ToCommand(currencyId), cancellationToken);
        
        return Ok(result.ToResponse());
    }
}
