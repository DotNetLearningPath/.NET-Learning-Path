using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Application.DTO.Currencies;
using Microsoft.AspNetCore.Mvc;

namespace Insurance.Api.Controllers;

[ApiController]
[Route("api/admin/currencies")]
public sealed class CurrenciesController(ICurrencyService currencyService) : ControllerBase
{
    [HttpGet("{id:guid}", Name = nameof(GetCurrencyByIdAsync))]
    public async Task<ActionResult<CurrencyDto>> GetCurrencyByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var currency = await currencyService.GetCurrencyByIdAsync(id, cancellationToken);
        return currency is null ? NotFound() : Ok(currency);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<CurrencyDto>>> ListCurrenciesAsync(
        [FromQuery] PaginationRequest pagination, CancellationToken cancellationToken) =>
        Ok(await currencyService.ListCurrenciesAsync(pagination, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<CurrencyDto>> CreateCurrencyAsync(
        CreateCurrencyRequest request, CancellationToken cancellationToken)
    {
        var currency = await currencyService.CreateCurrencyAsync(request, cancellationToken);
        return CreatedAtRoute(nameof(GetCurrencyByIdAsync), new { id = currency.Id }, currency);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CurrencyDto>> UpdateCurrencyAsync(
        Guid id, UpdateCurrencyRequest request, CancellationToken cancellationToken) =>
        Ok(await currencyService.UpdateCurrencyAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<CurrencyDto>> ActivateCurrencyAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await currencyService.ActivateCurrencyAsync(id, cancellationToken));

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<CurrencyDto>> DeactivateCurrencyAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await currencyService.DeactivateCurrencyAsync(id, cancellationToken));
}
