using InsuranceManagement.Application.DTOs.Metadata;
using InsuranceManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceManagement.API.Controllers
{
    [ApiController]
    [Route("api/admin/fees")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class FeeConfigurationsController(IFeeConfigurationService feeConfigurationService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(typeof(IReadOnlyList<FeeConfigurationDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<FeeConfigurationDto>>> GetAll()
        {
            return Ok(await feeConfigurationService.GetAllFeeConfigurationsAsync());
        }

        [HttpPost]
        [ProducesResponseType(typeof(FeeConfigurationDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FeeConfigurationDto>> Create(CreateFeeConfigurationDto request)
        {
            var configuration = await feeConfigurationService.CreateFeeConfigurationAsync(request);
            return CreatedAtAction(nameof(GetAll), configuration);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(FeeConfigurationDto), StatusCodes.Status200OK)]
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
