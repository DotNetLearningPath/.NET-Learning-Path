using Application.Abstractions;
using Application.DTO.Brokers;
using Application.DTO.Common;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers;

[ApiController]
[Route("api/admin/brokers")]
public sealed class AdminBrokersController : ControllerBase
{
    private readonly IBrokerService _brokerService;

    public AdminBrokersController(IBrokerService brokerService) => _brokerService = brokerService;

    [HttpGet("{id:guid}", Name = nameof(GetByIdAsync))]
    public async Task<ActionResult<BrokerDto>> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var broker = await _brokerService.GetBrokerByIdAsync(id, cancellationToken);
        return broker is null ? NotFound() : Ok(broker);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<BrokerDto>>> ListAsync([FromQuery] PaginationRequest pagination, CancellationToken cancellationToken) =>
        Ok(await _brokerService.ListBrokersAsync(pagination, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<BrokerDto>> CreateAsync(CreateBrokerRequest request, CancellationToken cancellationToken)
    {
        var broker = await _brokerService.CreateBrokerAsync(request, cancellationToken);
        return CreatedAtRoute(nameof(GetByIdAsync), new { id = broker.Id }, broker);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BrokerDto>> UpdateAsync(Guid id, UpdateBrokerRequest request, CancellationToken cancellationToken) =>
        Ok(await _brokerService.UpdateBrokerAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<BrokerDto>> ActivateAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await _brokerService.ActivateBrokerAsync(id, cancellationToken));

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<BrokerDto>> DeactivateAsync(Guid id, CancellationToken cancellationToken) =>
        Ok(await _brokerService.DeactivateBrokerAsync(id, cancellationToken));
}
