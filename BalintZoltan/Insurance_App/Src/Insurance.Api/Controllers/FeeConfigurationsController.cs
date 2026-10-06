using Insurance.Application.DTO.Common;
using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Fees;
using Microsoft.AspNetCore.Mvc;
namespace Insurance.Api.Controllers;

[ApiController]
[Route("api/admin/fees")]
public sealed class FeeConfigurationsController(
    IFeeConfigurationService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<FeeConfigurationDto>>> ListAsync(
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var fees = await service.ListAsync(pagination, cancellationToken);
        return Ok(fees);
    }

    [HttpPost]
    public async Task<ActionResult<FeeConfigurationDto>> CreateAsync(
        SaveFeeConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var fee = await service.CreateAsync(request, cancellationToken);
        return Created($"/api/admin/fees/{fee.Id}", fee);
    }
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FeeConfigurationDto>> UpdateAsync(
        Guid id,
        SaveFeeConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var fee = await service.UpdateAsync(id, request, cancellationToken);
        return Ok(fee);
    }
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> DeactivateAsync(Guid id, CancellationToken cancellationToken)
    {
        await service.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}
