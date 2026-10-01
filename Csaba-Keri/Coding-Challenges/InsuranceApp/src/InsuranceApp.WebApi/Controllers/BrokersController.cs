using InsuranceApp.Application.Brokers;
using InsuranceApp.WebApi.Mappings;
using InsuranceApp.WebApi.Models.Brokers;
using InsuranceApp.WebApi.Models.Common;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.WebApi.Controllers;

[ApiController]
[Route("api/admin/brokers")]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
public class BrokersController(IBrokerService brokerService) : ControllerBase
{
    private readonly IBrokerService _brokerService = brokerService;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<BrokerResponse>>> GetBrokers(
        [FromQuery] PageRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _brokerService.GetBrokersAsync(request.ToQuery(), cancellationToken);

        return Ok(result.ToResponse(broker => broker.ToResponse()));
    }

    [HttpGet("{brokerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BrokerResponse>> GetBrokerById(
        [FromRoute] Guid brokerId,
        CancellationToken cancellationToken
    )
    {
        var result = await _brokerService.GetBrokerByIdAsync(brokerId, cancellationToken);
        
        return Ok(result.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BrokerResponse>> CreateBroker(
        [FromBody] CreateBrokerRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _brokerService.CreateBrokerAsync(request.ToCommand(), cancellationToken);
        
        return CreatedAtAction(nameof(GetBrokerById), new { brokerId = result.Id }, result.ToResponse());
    }

    [HttpPut("{brokerId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BrokerResponse>> UpdateBroker(
        [FromRoute] Guid brokerId,
        [FromBody] UpdateBrokerRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _brokerService.UpdateBrokerAsync(request.ToCommand(brokerId), cancellationToken);
        
        return Ok(result.ToResponse());
    }

    [HttpPost("{brokerId}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BrokerResponse>> ActivateBroker(
        [FromRoute] Guid brokerId,
        CancellationToken cancellationToken
    )
    {
        var result = await _brokerService.ActivateBrokerAsync(brokerId, cancellationToken);
        
        return Ok(result.ToResponse());
    }

    [HttpPost("{brokerId}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BrokerResponse>> DeactivateBroker(
        [FromRoute] Guid brokerId,
        CancellationToken cancellationToken
    )
    {
        var result = await _brokerService.DeactivateBrokerAsync(brokerId, cancellationToken);
        
        return Ok(result.ToResponse());
    }
}
