using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Application.DTO.Policies;
using Insurance.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Insurance.Api.Controllers;

[ApiController]
[Route("api/brokers/policies")]
public sealed class PoliciesController(IPolicyService policyService) : ControllerBase
{
    [HttpGet]
    public Task<PagedResult<PolicyDto>> SearchPolicyAsync(
        [FromQuery] string? policyNumber,
        [FromQuery] Guid? clientId,
        [FromQuery] Guid? brokerId,
        [FromQuery] PolicyStatus? status,
        [FromQuery] DateTime? startDateFrom,
        [FromQuery] DateTime? startDateTo,
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken) =>
        policyService.SearchPolicyAsync(
            policyNumber,
            clientId,
            brokerId,
            status,
            startDateFrom,
            startDateTo,
            pagination,
            cancellationToken);

    [HttpGet("{id:guid}", Name = nameof(GetPolicyByIdAsync))]
    public async Task<ActionResult<PolicyDto>> GetPolicyByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var policy = await policyService.GetPolicyByIdAsync(id, cancellationToken);

        return policy is null ? NotFound() : Ok(policy);
    }

    [HttpPost]
    public async Task<ActionResult<PolicyDto>> CreatePolicyAsync(CreatePolicyRequest request, CancellationToken cancellationToken)
    {
        var result = await policyService.CreatePolicyAsync(request, cancellationToken);
        return CreatedAtRoute(nameof(GetPolicyByIdAsync), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/activate")]
    public Task<PolicyDto> ActivatePolicyAsync(Guid id, CancellationToken cancellationToken) => policyService.ActivatePolicyAsync(id, cancellationToken);

    [HttpPost("{id:guid}/cancel")]
    public Task<PolicyDto> CancelPolicyAsync(
        Guid id,
        CancelPolicyRequest request,
        CancellationToken cancellationToken) =>
        policyService.CancelPolicyAsync(id, request, cancellationToken);
}
