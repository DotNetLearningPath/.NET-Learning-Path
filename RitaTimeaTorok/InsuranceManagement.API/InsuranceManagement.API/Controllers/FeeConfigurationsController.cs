using InsuranceManagement.Application.DTOs.Metadata;
using InsuranceManagement.Application.Constants;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.API.Controllers
{
    [ApiController]
    [Route("api/admin/fees")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class FeeConfigurationsController(IFeeConfigurationService feeConfigurationService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<FeeConfigurationDto>>> GetAll(
            [FromQuery, Range(PaginationLimits.MinimumPageNumber, PaginationLimits.MaximumPageNumber)]
                int pageNumber = PaginationLimits.DefaultPageNumber,
            [FromQuery, Range(PaginationLimits.MinimumPageSize, PaginationLimits.MaximumPageSize)]
                int pageSize = PaginationLimits.DefaultPageSize)
        {
            return Ok(await feeConfigurationService.GetAllFeeConfigurationsAsync(pageNumber, pageSize));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FeeConfigurationDto>> Create(CreateFeeConfigurationDto request)
        {
            var configuration = await feeConfigurationService.CreateFeeConfigurationAsync(request);
            return StatusCode(StatusCodes.Status201Created, configuration);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FeeConfigurationDto>> Update(
            Guid id,
            UpdateFeeConfigurationDto request)
        {
            var configuration = await feeConfigurationService.UpdateFeeConfigurationAsync(id, request);
            return Ok(configuration);
        }
    }
}
