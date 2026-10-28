using InsuranceApp.Application.Policies;
using InsuranceApp.WebApi.Mappings;
using InsuranceApp.WebApi.Models.Policies;
using InsuranceApp.WebApi.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.WebApi.Controllers;

[ApiController]
[Route("api/brokers/policies")]
[Authorize(Policy = AuthorizationPolicies.BrokerOnly)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class PoliciesController(IPolicyService policyService) : ControllerBase
{
    private readonly IPolicyService _policyService = policyService;

    [HttpGet("{policyId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PolicyResponse>> GetPolicyById(
        [FromRoute] Guid policyId,
        CancellationToken cancellationToken
    )
    {
        var result = await _policyService.GetPolicyByIdAsync(policyId, cancellationToken);

        return Ok(result.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<PolicyResponse>> CreateDraftPolicy(
        [FromBody] CreateDraftPolicyRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _policyService.CreateDraftPolicyAsync(request.ToCommand(), cancellationToken);

        return CreatedAtAction(nameof(GetPolicyById), new { policyId = result.Id }, result.ToResponse());
    }

    [HttpPost("{policyId}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PolicyResponse>> ActivatePolicy(
        [FromRoute] Guid policyId,
        CancellationToken cancellationToken
    )
    {
        var result = await _policyService.ActivatePolicyAsync(policyId, cancellationToken);
        
        return Ok(result.ToResponse());
    }

    [HttpPost("{policyId}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PolicyResponse>> CancelPolicy(
        [FromRoute] Guid policyId,
        [FromBody] CancelPolicyRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _policyService.CancelPolicyAsync(request.ToCommand(policyId), cancellationToken);
        
        return Ok(result.ToResponse());
    }
}
