using InsuranceApp.Api.Common;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.DTOs.Broker;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers.Admin;

/// <summary>
/// Controller for managing brokers in the administrator context.
/// </summary>
/// <param name="brokerService">The broker service.</param>
/// <param name="logger">The logger.</param>
[ApiController]
[Route("api/admin/brokers")]
public sealed class BrokersController(IBrokerService brokerService, ILogger<BrokersController> logger) : ControllerBase
{
    private const string GetBrokerByIdRouteName = "GetBrokerById";

    /// <summary>
    /// Gets all brokers.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BrokerDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BrokerDto>>> GetBrokersAsync(CancellationToken cancellationToken)
    {
        var result = await brokerService.GetBrokersAsync(cancellationToken);

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a broker by its ID.
    /// </summary>
    /// <param name="brokerId">The ID of the broker.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The broker with the specified ID.</returns>
    [HttpGet("{brokerId:guid}", Name = GetBrokerByIdRouteName)]
    [ProducesResponseType(typeof(BrokerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BrokerDto>> GetBrokerByIdAsync(Guid brokerId, CancellationToken cancellationToken)
    {
        var result = await brokerService.GetBrokerByIdAsync(brokerId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult(logger);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new broker.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BrokerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BrokerDto>> CreateBrokerAsync(CreateBrokerDto request, CancellationToken cancellationToken)
    {
        var result = await brokerService.CreateBrokerAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult(logger);
        }

        return CreatedAtRoute(GetBrokerByIdRouteName, new { brokerId = result.Value!.BrokerId }, result.Value);
    }

    /// <summary>
    /// Updates an existing broker.
    /// </summary>
    [HttpPut("{brokerId:guid}")]
    [ProducesResponseType(typeof(BrokerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BrokerDto>> UpdateBrokerAsync(Guid brokerId, UpdateBrokerDto request, CancellationToken cancellationToken)
    {
        var result = await brokerService.UpdateBrokerAsync(brokerId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult(logger);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Activates an existing broker.
    /// </summary>
    [HttpPost("{brokerId:guid}/activate")]
    [ProducesResponseType(typeof(BrokerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BrokerDto>> ActivateBrokerAsync(Guid brokerId, CancellationToken cancellationToken)
    {
        var result = await brokerService.ActivateBrokerAsync(brokerId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult(logger);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Deactivates an existing broker.
    /// </summary>
    [HttpPost("{brokerId:guid}/deactivate")]
    [ProducesResponseType(typeof(BrokerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BrokerDto>> DeactivateBrokerAsync(Guid brokerId, CancellationToken cancellationToken)
    {
        var result = await brokerService.DeactivateBrokerAsync(brokerId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult(logger);
        }

        return Ok(result.Value);
    }
}