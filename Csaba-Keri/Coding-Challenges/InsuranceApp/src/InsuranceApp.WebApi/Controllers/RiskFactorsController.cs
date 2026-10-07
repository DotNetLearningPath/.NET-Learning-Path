using InsuranceApp.Application.RiskFactors;
using InsuranceApp.WebApi.Mappings;
using InsuranceApp.WebApi.Models.Common;
using InsuranceApp.WebApi.Models.RiskFactors;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.WebApi.Controllers;

[ApiController]
[Route("api/admin/risk-factors")]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
public class RiskFactorsController(IRiskFactorService riskFactorService) : ControllerBase
{
    private readonly IRiskFactorService _riskFactorService = riskFactorService;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<RiskFactorResponse>>> GetRiskFactors(
        [FromQuery] PageRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _riskFactorService.GetRiskFactorsAsync(request.ToQuery(), cancellationToken);
        
        return Ok(result.ToResponse(riskFactor => riskFactor.ToResponse()));
    }

    [HttpGet("{riskFactorId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RiskFactorResponse>> GetRiskFactorById(
        [FromRoute] Guid riskFactorId,
        CancellationToken cancellationToken
    )
    {
        var result = await _riskFactorService.GetRiskFactorByIdAsync(riskFactorId, cancellationToken);
        
        return Ok(result.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<RiskFactorResponse>> CreateRiskFactor(
        [FromBody] SaveRiskFactorRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _riskFactorService.CreateRiskFactorAsync(request.ToCreateCommand(), cancellationToken);
        
        return CreatedAtAction(nameof(GetRiskFactorById), new { riskFactorId = result.Id }, result.ToResponse());
    }

    [HttpPut("{riskFactorId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RiskFactorResponse>> UpdateRiskFactor(
        [FromRoute] Guid riskFactorId,
        [FromBody] SaveRiskFactorRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _riskFactorService.UpdateRiskFactorAsync(request.ToUpdateCommand(riskFactorId), cancellationToken);
        
        return Ok(result.ToResponse());
    }
}
