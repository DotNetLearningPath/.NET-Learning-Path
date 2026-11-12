using InsuranceManagement.Application.DTOs.Metadata;
using InsuranceManagement.Application.Constants;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace InsuranceManagement.API.Controllers
{
    [ApiController]
    [Route("api/admin/risk-factors")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class RiskFactorConfigurationsController(
        IRiskFactorConfigurationService riskFactorConfigurationService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<PagedResult<RiskFactorConfigurationDto>>> GetAll(
            [FromQuery, Range(PaginationLimits.MinimumPageNumber, PaginationLimits.MaximumPageNumber)]
                int pageNumber = PaginationLimits.DefaultPageNumber,
            [FromQuery, Range(PaginationLimits.MinimumPageSize, PaginationLimits.MaximumPageSize)]
                int pageSize = PaginationLimits.DefaultPageSize)
        {
            return Ok(await riskFactorConfigurationService.GetAllRiskFactorConfigurationsAsync(pageNumber, pageSize));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RiskFactorConfigurationDto>> Create(CreateRiskFactorConfigurationDto request)
        {
            var configuration = await riskFactorConfigurationService.CreateRiskFactorConfigurationAsync(request);
            return StatusCode(StatusCodes.Status201Created, configuration);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RiskFactorConfigurationDto>> Update(
            Guid id,
            UpdateRiskFactorConfigurationDto request)
        {
            var configuration = await riskFactorConfigurationService.UpdateRiskFactorConfigurationAsync(id, request);
            return Ok(configuration);
        }
    }
}
