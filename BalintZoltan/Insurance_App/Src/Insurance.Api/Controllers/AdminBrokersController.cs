using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Brokers;
using Insurance.Application.DTO.Common;
using Microsoft.AspNetCore.Mvc;

namespace Insurance.Api.Controllers;

[ApiController]
[Route("api/admin/brokers")]
public sealed class AdminBrokersController(IBrokerService brokerService) : ControllerBase
{
    [HttpGet("{id:guid}", Name = nameof(GetByIdAsync))]
    public async Task<ActionResult<BrokerDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var broker = await brokerService.GetBrokerByIdAsync(id, cancellationToken);
        return broker is null ? NotFound() : Ok(broker);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<BrokerDto>>> ListAsync(
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken) =>
        Ok(await brokerService.ListBrokersAsync(
            pagination,
            cancellationToken));

    [HttpPost]
    public async Task<ActionResult<BrokerDto>> CreateAsync(CreateBrokerRequest request, CancellationToken cancellationToken)
    {
        var broker = await brokerService.CreateBrokerAsync(request, cancellationToken);
        return CreatedAtRoute(nameof(GetByIdAsync), new { id = broker.Id }, broker);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BrokerDto>> UpdateAsync(Guid id, UpdateBrokerRequest request, CancellationToken cancellationToken) =>
        Ok(await brokerService.UpdateBrokerAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<BrokerDto>> ActivateAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await brokerService.ActivateBrokerAsync(id, cancellationToken));

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<BrokerDto>> DeactivateAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await brokerService.DeactivateBrokerAsync(id, cancellationToken));
}
