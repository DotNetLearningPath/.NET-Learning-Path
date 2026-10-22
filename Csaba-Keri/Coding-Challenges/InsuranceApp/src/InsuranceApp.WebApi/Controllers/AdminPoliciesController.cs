using InsuranceApp.Application.Policies;
using InsuranceApp.WebApi.Mappings;
using InsuranceApp.WebApi.Models.Policies;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.WebApi.Controllers;

[ApiController]
[Route("api/admin/policies")]
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
