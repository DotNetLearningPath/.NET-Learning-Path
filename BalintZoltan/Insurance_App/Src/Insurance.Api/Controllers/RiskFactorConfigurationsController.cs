using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Application.DTO.RiskFactors;
using Microsoft.AspNetCore.Mvc;

namespace Insurance.Api.Controllers;

[ApiController]
[Route("api/admin/risk-factors")]
public sealed class RiskFactorConfigurationsController(
    IRiskFactorConfigurationService riskFactorConfigurationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<RiskFactorConfigurationDto>>> ListRiskFactorAsync(
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken) =>
        Ok(await riskFactorConfigurationService.ListRiskFactorAsync(pagination, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<RiskFactorConfigurationDto>> CreateRiskFactorAsync(
        SaveRiskFactorConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var configuration = await riskFactorConfigurationService.CreateRiskFactorAsync(request, cancellationToken);
        return Created($"/api/admin/risk-factors/{configuration.Id}", configuration);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RiskFactorConfigurationDto>> UpdateRiskFactorAsync(
        Guid id,
        SaveRiskFactorConfigurationRequest request,
        CancellationToken cancellationToken) =>
        Ok(await riskFactorConfigurationService.UpdateRiskFactorAsync(id, request, cancellationToken));

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<RiskFactorConfigurationDto>> ActivateRiskFactorAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var configuration = await riskFactorConfigurationService.ActivateRiskAsync(id, cancellationToken);
        return Ok(configuration);
    }

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<RiskFactorConfigurationDto>> DeactivateRiskFactorAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var configuration = await riskFactorConfigurationService.DeactivateRiskFactorAsync(id, cancellationToken);
        return Ok(configuration);
    }
}
