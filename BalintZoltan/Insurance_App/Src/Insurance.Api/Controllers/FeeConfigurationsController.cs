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
    public async Task<ActionResult<PagedResult<FeeConfigurationDto>>> ListAsync(
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var fees = await feeConfigurationService.ListAsync(pagination, cancellationToken);
        return Ok(fees);
    }

    [HttpPost]
    public async Task<ActionResult<FeeConfigurationDto>> CreateAsync(
        SaveFeeConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var fee = await feeConfigurationService.CreateAsync(request, cancellationToken);
        return Created($"/api/admin/fees/{fee.Id}", fee);
    }
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FeeConfigurationDto>> UpdateAsync(
        Guid id,
        SaveFeeConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var fee = await feeConfigurationService.UpdateAsync(id, request, cancellationToken);
        return Ok(fee);
    }
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        await feeConfigurationService.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}
