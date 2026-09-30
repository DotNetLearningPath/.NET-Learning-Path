using Application.DTO.Common;
using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Fees;
using Microsoft.AspNetCore.Mvc;
namespace Insurance.Api.Controllers;

[ApiController]
[Route("api/admin/fees")]
public sealed class FeeConfigurationsController : ControllerBase
{
    private readonly IFeeConfigurationService _service;
    public FeeConfigurationsController(IFeeConfigurationService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<FeeConfigurationDto>>> List(
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var fees = await _service.ListAsync(pagination, cancellationToken);
        return Ok(fees);
    }

    [HttpPost]
    public async Task<ActionResult<FeeConfigurationDto>> Create(
        SaveFeeConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var fee = await _service.CreateAsync(request, cancellationToken);
        return Created($"/api/admin/fees/{fee.Id}", fee);
    }
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<FeeConfigurationDto>> Update(
        Guid id,
        SaveFeeConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var fee = await _service.UpdateAsync(id, request, cancellationToken);
        return Ok(fee);
    }
    [HttpPost("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeactivateAsync(id, cancellationToken);
        return NoContent();
    }
}
