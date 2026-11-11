using InsuranceManagement.Application.DTOs.Metadata;
using InsuranceManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<ActionResult<IReadOnlyList<RiskFactorConfigurationDto>>> GetAll()
        {
            return Ok(await riskFactorConfigurationService.GetAllRiskFactorConfigurationsAsync());
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RiskFactorConfigurationDto>> Create(CreateRiskFactorConfigurationDto request)
        {
            var configuration = await riskFactorConfigurationService.CreateRiskFactorConfigurationAsync(request);
            return CreatedAtAction(nameof(GetAll), configuration);
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
