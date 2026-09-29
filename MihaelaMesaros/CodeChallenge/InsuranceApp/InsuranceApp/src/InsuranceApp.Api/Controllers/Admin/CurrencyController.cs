using InsuranceApp.Api.Common;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.DTOs.Currency;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers.Admin;

/// <summary>
/// Controller for managing currencies in the administrator context.
/// </summary>
/// <param name="currencyService">The currency service.</param>
[ApiController]
[Route("api/admin/currencies")]
public sealed class CurrenciesController(ICurrencyService currencyService) : ControllerBase
{
    private const string GetCurrencyByIdRouteName = "GetCurrencyById";

    /// <summary>
    /// Gets all currencies.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CurrencyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<CurrencyDto>>> GetCurrenciesAsync(CancellationToken cancellationToken)
    {
        var result = await currencyService.GetCurrenciesAsync(cancellationToken);

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a currency by its ID.
    /// </summary>
    /// <param name="currencyId">The ID of the currency.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The currency with the specified ID.</returns>
    [HttpGet("{currencyId:guid}", Name = GetCurrencyByIdRouteName)]
    [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CurrencyDto>> GetCurrencyByIdAsync(Guid currencyId, CancellationToken cancellationToken)
    {
        var result = await currencyService.GetCurrencyByIdAsync(currencyId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new currency.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CurrencyDto>> CreateCurrencyAsync(CreateCurrencyDto request, CancellationToken cancellationToken)
    {
        var result = await currencyService.CreateCurrencyAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult();
        }

        return CreatedAtRoute(GetCurrencyByIdRouteName, new { currencyId = result.Value!.CurrencyId }, result.Value);
    }

    /// <summary>
    /// Updates an existing currency.
    /// </summary>
    [HttpPut("{currencyId:guid}")]
    [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CurrencyDto>> UpdateCurrencyAsync(Guid currencyId, UpdateCurrencyDto request, CancellationToken cancellationToken)
    {
        var result = await currencyService.UpdateCurrencyAsync(currencyId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult();
        }

        return Ok(result.Value);
    }
}