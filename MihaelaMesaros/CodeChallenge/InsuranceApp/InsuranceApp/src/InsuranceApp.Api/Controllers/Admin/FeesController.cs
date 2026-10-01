using InsuranceApp.Api.Common;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.DTOs.FeeConfig;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers.Admin;

/// <summary>
/// Controller for managing fee configurations in the admin context.
/// </summary>
/// <param name="feeConfigService">The service used to manage fee configurations.</param>
/// <param name="logger">The logger.</param>
[ApiController]
[Route("api/admin/fees")]
public sealed class FeesController(IFeeConfigService feeConfigService, ILogger<FeesController> logger) : ControllerBase
{
    private const string GetFeeConfigByIdRouteName = "GetFeeConfigById";

    /// <summary>
    /// Retrieves a list of all fee configurations.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FeeConfigDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FeeConfigDto>>> GetFeeConfigsAsync(CancellationToken cancellationToken)
    {
        var result = await feeConfigService.GetFeeConfigsAsync(cancellationToken);

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a fee configuration by its ID.
    /// </summary>
    /// <param name="feeConfigId">The ID of the fee configuration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The fee configuration with the specified ID.</returns>
    [HttpGet("{feeConfigId:guid}", Name = GetFeeConfigByIdRouteName)]
    [ProducesResponseType(typeof(FeeConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeeConfigDto>> GetFeeConfigByIdAsync(Guid feeConfigId, CancellationToken cancellationToken)
    {
        var result = await feeConfigService.GetFeeConfigByIdAsync(feeConfigId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult(logger);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new fee configuration.
    /// </summary>
    /// <param name="request">The fee configuration to create.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(FeeConfigDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FeeConfigDto>> CreateFeeConfigAsync(CreateFeeConfigDto request, CancellationToken cancellationToken)
    {
        var result = await feeConfigService.CreateFeeConfigAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult(logger);
        }

        return CreatedAtRoute(GetFeeConfigByIdRouteName, new { feeConfigId = result.Value!.FeeConfigId }, result.Value);
    }

    /// <summary>
    /// Updates an existing fee configuration.
    /// </summary>
    /// <param name="feeConfigId">The ID of the fee configuration to update.</param>
    /// <param name="request">The updated fee configuration data.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated fee configuration.</returns>
    [HttpPut("{feeConfigId:guid}")]
    [ProducesResponseType(typeof(FeeConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeeConfigDto>> UpdateFeeConfigAsync(Guid feeConfigId, UpdateFeeConfigDto request, CancellationToken cancellationToken)
    {
        var result = await feeConfigService.UpdateFeeConfigAsync(feeConfigId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult(logger);
        }

        return Ok(result.Value);
    }
}