using InsuranceApp.Application.Policies;
using InsuranceApp.WebApi.Mappings;
using InsuranceApp.WebApi.Models.Policies;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.WebApi.Controllers;

[ApiController]
[Route("api/brokers/policies")]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
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
}
