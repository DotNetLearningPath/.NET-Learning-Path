using InsuranceApp.Api.Common;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Policy;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers.Broker;

/// <summary>
/// Controller for managing policies in the broker context.
/// </summary>
/// <param name="policyService">The policy service.</param>
/// <param name="logger">The logger.</param>
[ApiController]
[Route("api/brokers/policies")]
public sealed class PoliciesController(IPolicyService policyService, ILogger<PoliciesController> logger) : ControllerBase
{
    /// <summary>
    /// Searches for policies based on the provided search criteria.
    /// </summary>
    /// <param name="search">The search criteria.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The paged result of policies.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PolicyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<PolicyDto>>> SearchPoliciesAsync([FromQuery] PolicySearchDto search, CancellationToken cancellationToken)
    {
        var result = await policyService.SearchPoliciesAsync(search, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult(logger);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a policy by identifier.
    /// </summary>
    /// <param name="policyId">The policy identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The policy.</returns>
    [HttpGet("{policyId:guid}")]
    [ProducesResponseType(typeof(PolicyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PolicyDto>> GetPolicyByIdAsync(Guid policyId, CancellationToken cancellationToken)
    {
        var result = await policyService.GetPolicyByIdAsync(policyId, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult(logger);
        }

        return Ok(result.Value);
    }
}