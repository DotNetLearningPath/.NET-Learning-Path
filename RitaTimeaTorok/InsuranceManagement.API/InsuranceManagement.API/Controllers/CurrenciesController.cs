using InsuranceManagement.Application.DTOs.Metadata;
using InsuranceManagement.Application.Constants;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.API.Controllers
{
    [ApiController]
    [Route("api/admin/currencies")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class CurrenciesController(ICurrencyService currencyService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<CurrencyDto>>> GetAll(
            [FromQuery, Range(PaginationLimits.MinimumPageNumber, PaginationLimits.MaximumPageNumber)]
                int pageNumber = PaginationLimits.DefaultPageNumber,
            [FromQuery, Range(PaginationLimits.MinimumPageSize, PaginationLimits.MaximumPageSize)]
                int pageSize = PaginationLimits.DefaultPageSize)
        {
            return Ok(await currencyService.GetAllCurrenciesAsync(pageNumber, pageSize));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CurrencyDto>> Create(CreateCurrencyDto request)
        {
            var currency = await currencyService.CreateCurrencyAsync(request);
            return StatusCode(StatusCodes.Status201Created, currency);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CurrencyDto>> Update(Guid id, UpdateCurrencyDto request)
        {
            var currency = await currencyService.UpdateCurrencyAsync(id, request);
            return Ok(currency);
        }
    }
}
