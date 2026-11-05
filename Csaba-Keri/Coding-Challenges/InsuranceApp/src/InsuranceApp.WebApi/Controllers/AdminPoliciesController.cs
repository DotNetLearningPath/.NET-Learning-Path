using InsuranceApp.Application.Policies;
using InsuranceApp.WebApi.Mappings;
using InsuranceApp.WebApi.Models.Policies;
using InsuranceApp.WebApi.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.WebApi.Controllers;

[ApiController]
[Route("api/admin/policies")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class AdminPoliciesController(IPolicyService policyService) : ControllerBase
{
    private readonly IPolicyService _policyService = policyService;

    [HttpPost("expire")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ExpirePoliciesResponse>> ExpirePolicies(CancellationToken cancellationToken)
    {
        var result = await _policyService.ExpirePoliciesAsync(cancellationToken);
        
        return Ok(result.ToResponse());
    }
}
