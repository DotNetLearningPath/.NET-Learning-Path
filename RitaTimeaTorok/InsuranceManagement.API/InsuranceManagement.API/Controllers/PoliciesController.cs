using InsuranceManagement.Application.DTOs.Policy;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Constants;
using InsuranceManagement.Application.Interfaces.Services;
using InsuranceManagement.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceManagement.API.Controllers
{
    [ApiController]
    [Route("api/brokers/policies")]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public class PoliciesController(IPolicyService policyService) : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PagedResult<PolicyListItemDto>>> GetAll(
            [FromQuery] int pageNumber = PaginationLimits.DefaultPageNumber,
            [FromQuery] int pageSize = PaginationLimits.DefaultPageSize,
            [FromQuery] Guid? clientId = null,
            [FromQuery] Guid? brokerId = null,
            [FromQuery] PolicyStatus? status = null,
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null) =>
            Ok(await policyService.GetAllPoliciesAsync(
                pageNumber, pageSize, clientId, brokerId, status, startDate, endDate));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PolicyDto>> GetById(Guid id) =>
            Ok(await policyService.GetPolicyByIdAsync(id));

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<PolicyDto>> Create(CreatePolicyDto request)
        {
            var policy = await policyService.CreatePolicyAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = policy.Id }, policy);
        }

        [HttpPost("{id:guid}/activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<PolicyDto>> Activate(Guid id) =>
            Ok(await policyService.ActivatePolicyAsync(id));

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<PolicyDto>> Cancel(Guid id, CancelPolicyDto request) =>
            Ok(await policyService.CancelPolicyAsync(id, request));

        [HttpPost("{id:guid}/expire")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<PolicyDto>> Expire(Guid id) =>
            Ok(await policyService.ExpirePolicyAsync(id));
    }
}
