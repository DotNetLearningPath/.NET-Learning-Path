using InsuranceApp.Application.Policies;
using InsuranceApp.WebApi.Mappings;
using InsuranceApp.WebApi.Models.Common;
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
public class PolicyQueriesController(IPolicyQueryService policyQueryService) : ControllerBase
{
    private readonly IPolicyQueryService _policyQueryService = policyQueryService;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<PolicyListItemResponse>>> SearchPolicies(
        [FromQuery] SearchPoliciesRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _policyQueryService.SearchPoliciesAsync(request.ToQuery(), cancellationToken);

        return Ok(result.ToResponse(policy => policy.ToResponse()));
    }

    [HttpGet("{policyId}/details")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PolicyDetailsResponse>> GetPolicyDetailsById(
        [FromRoute] Guid policyId,
        CancellationToken cancellationToken
    )
    {
        var result = await _policyQueryService.GetPolicyDetailsAsync(policyId, cancellationToken);

        return Ok(result.ToResponse());
    }
}
