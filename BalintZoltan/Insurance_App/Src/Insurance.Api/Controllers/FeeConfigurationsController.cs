using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Application.DTO.Fees;
using Microsoft.AspNetCore.Mvc;
namespace Insurance.Api.Controllers;

[ApiController]
[Route("api/admin/fees")]
public sealed class FeeConfigurationsController(
    IFeeConfigurationService feeConfigurationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<FeeConfigurationDto>>> ListFeeAsync(
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var fees = await feeConfigurationService.ListFeeAsync(pagination, cancellationToken);
        return Ok(fees);
    }

    [HttpGet("{id:guid}", Name = nameof(GetFeeByIdAsync))]
    public async Task<ActionResult<FeeConfigurationDto>> GetFeeByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var fee = await feeConfigurationService.GetFeeByIdAsync(id, cancellationToken);
        return Ok(fee);
    }

    [HttpPost]
    public async Task<ActionResult<FeeConfigurationDto>> CreateFeeAsync(
        SaveFeeConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var fee = await feeConfigurationService.CreateFeeAsync(request, cancellationToken);
        return CreatedAtRoute(nameof(GetFeeByIdAsync), new { id = fee.Id }, fee);
    }
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FeeConfigurationDto>> UpdateFeeAsync(
        Guid id,
        SaveFeeConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var fee = await feeConfigurationService.UpdateFeeAsync(id, request, cancellationToken);
        return Ok(fee);
    }
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateFeeAsync(Guid id, CancellationToken cancellationToken)
    {
        await feeConfigurationService.DeactivateFeeAsync(id, cancellationToken);
        return NoContent();
    }
}
