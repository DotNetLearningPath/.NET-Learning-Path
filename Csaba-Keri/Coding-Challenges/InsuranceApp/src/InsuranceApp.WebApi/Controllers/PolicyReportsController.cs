using InsuranceApp.Application.Reports;
using InsuranceApp.WebApi.Mappings;
using InsuranceApp.WebApi.Models.Reports;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.WebApi.Controllers;

[ApiController]
[Route("api/admin/reports")]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
public class PolicyReportsController(IPolicyReportService policyReportService) : ControllerBase
{
    private readonly IPolicyReportService _policyReportService = policyReportService;

    [HttpGet("policies-by-country")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PolicyReportRowResponse>>> GetPoliciesByCountry(
        [FromQuery] PolicyReportRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _policyReportService.GetReportAsync(PolicyReportGrouping.Country, request.ToQuery(), cancellationToken);

        return Ok(result.Select(row => row.ToResponse()));
    }

    [HttpGet("policies-by-county")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PolicyReportRowResponse>>> GetPoliciesByCounty(
        [FromQuery] PolicyReportRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _policyReportService.GetReportAsync(PolicyReportGrouping.County, request.ToQuery(), cancellationToken);

        return Ok(result.Select(row => row.ToResponse()));
    }

    [HttpGet("policies-by-city")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PolicyReportRowResponse>>> GetPoliciesByCity(
        [FromQuery] PolicyReportRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = await _policyReportService.GetReportAsync(PolicyReportGrouping.City, request.ToQuery(), cancellationToken);

        return Ok(result.Select(row => row.ToResponse()));
    }
}
