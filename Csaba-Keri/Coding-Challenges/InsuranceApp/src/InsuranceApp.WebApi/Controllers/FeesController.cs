using InsuranceApp.Application.Fees;
using InsuranceApp.WebApi.Mappings;
using InsuranceApp.WebApi.Models.Common;
using InsuranceApp.WebApi.Models.Fees;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.WebApi.Controllers;

[ApiController]
[Route("api/admin/fees")]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
public class FeesController(IFeeService feeService) : ControllerBase
{
    private readonly IFeeService _feeService = feeService;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResponse<FeeResponse>>> GetFees(
        [FromQuery] PageRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _feeService.GetFeesAsync(request.ToQuery(), cancellationToken);
        
        return Ok(result.ToResponse(fee => fee.ToResponse()));
    }

    [HttpGet("{feeId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeeResponse>> GetFeeById(
        [FromRoute] Guid feeId,
        CancellationToken cancellationToken
    )
    {
        var result = await _feeService.GetFeeByIdAsync(feeId, cancellationToken);
        
        return Ok(result.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<FeeResponse>> CreateFee(
        [FromBody] SaveFeeRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _feeService.CreateFeeAsync(request.ToCreateCommand(), cancellationToken);
        
        return CreatedAtAction(nameof(GetFeeById), new { feeId = result.Id }, result.ToResponse());
    }

    [HttpPut("{feeId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FeeResponse>> UpdateFee(
        [FromRoute] Guid feeId,
        [FromBody] SaveFeeRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _feeService.UpdateFeeAsync(request.ToUpdateCommand(feeId), cancellationToken);
        
        return Ok(result.ToResponse());
    }
}
