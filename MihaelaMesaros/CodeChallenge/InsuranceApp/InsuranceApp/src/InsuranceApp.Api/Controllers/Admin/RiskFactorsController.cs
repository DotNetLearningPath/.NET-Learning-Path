using InsuranceApp.Api.Common;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.DTOs.RiskFactorConfig;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers.Admin;

/// <summary>
/// Controller for managing risk factor configurations in the admin context.
/// </summary>
/// <param name="riskFactorConfigService">The service used to manage risk factor configurations.</param>
[ApiController]
[Route("api/admin/risk-factors")]
public sealed class RiskFactorsController(IRiskFactorConfigService riskFactorConfigService) : ControllerBase
{
    private const string GetRiskFactorConfigByIdRouteName = "GetRiskFactorConfigById";

    /// <summary>
    /// Retrieves a list of all risk factor configurations.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<RiskFactorConfigDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<RiskFactorConfigDto>>> GetRiskFactorConfigsAsync(CancellationToken cancellationToken)
    {
        var result = await riskFactorConfigService.GetRiskFactorConfigsAsync(cancellationToken);

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a risk factor configuration by its ID.
    /// </summary>
    /// <param name="riskFactorConfigId">The ID of the risk factor configuration.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The risk factor configuration with the specified ID.</returns>
    [HttpGet("{riskFactorConfigId:guid}", Name = GetRiskFactorConfigByIdRouteName)]
    [ProducesResponseType(typeof(RiskFactorConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RiskFactorConfigDto>> GetRiskFactorConfigByIdAsync(Guid riskFactorConfigId, CancellationToken cancellationToken)
    {
        var result = await riskFactorConfigService.GetRiskFactorConfigByIdAsync(riskFactorConfigId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult();
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new risk factor configuration.
    /// </summary>
    /// <param name="request">The risk factor configuration to create.</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpPost]
    [ProducesResponseType(typeof(RiskFactorConfigDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RiskFactorConfigDto>> CreateRiskFactorConfigAsync(CreateRiskFactorConfigDto request, CancellationToken cancellationToken)
    {
        var result = await riskFactorConfigService.CreateRiskFactorConfigAsync(request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult();
        }

        return CreatedAtRoute(GetRiskFactorConfigByIdRouteName, new { riskFactorConfigId = result.Value!.RiskFactorConfigId }, result.Value);
    }

    /// <summary>
    /// Updates an existing risk factor configuration.
    /// </summary>
    /// <param name="riskFactorConfigId">The ID of the risk factor configuration to update.</param>
    /// <param name="request">The updated risk factor configuration data.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated risk factor configuration.</returns>
    [HttpPut("{riskFactorConfigId:guid}")]
    [ProducesResponseType(typeof(RiskFactorConfigDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RiskFactorConfigDto>> UpdateRiskFactorConfigAsync(Guid riskFactorConfigId, UpdateRiskFactorConfigDto request, CancellationToken cancellationToken)
    {
        var result = await riskFactorConfigService.UpdateRiskFactorConfigAsync(riskFactorConfigId, request, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult();
        }

        return Ok(result.Value);
    }
}