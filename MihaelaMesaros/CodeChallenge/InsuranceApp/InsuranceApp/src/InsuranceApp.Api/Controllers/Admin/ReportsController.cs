using InsuranceApp.Api.Common;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.DTOs.Reports;
using InsuranceApp.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InsuranceApp.Api.Controllers.Admin;

/// <summary>
/// Controller for managing reports in the administrator context.
/// </summary>
/// <param name="policyReportService">The service for policies reports.</param>
/// <param name="logger">The logger.</param>
//[Authorize(Roles = "Admin")]
[AllowAnonymous]
[ApiController]
[Route("api/admin/reports")]
public sealed class ReportsController(IPolicyReportService policyReportService, ILogger<ReportsController> logger) : ControllerBase
{
    /// <summary>
    /// Gets a report of policies grouped by country based on the provided filters.
    /// </summary>
    /// <param name="reportFilters">The filters to apply to the report.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of policy reports grouped by country.</returns>
    [HttpGet("policies-by-country")]
    [ProducesResponseType(typeof(IReadOnlyList<PolicyReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetReportPoliciesByCountry([FromQuery] PolicyReportFiltersDto reportFilters, CancellationToken cancellationToken)
    {
        var result = await policyReportService.GetReportPoliciesAsync(PolicyReportGroupType.Country, reportFilters, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult(logger);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a report of policies grouped by county based on the provided filters.
    /// </summary>
    /// <param name="reportFilters">The filters to apply to the report.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of policy reports grouped by county.</returns>
    [HttpGet("policies-by-county")]
    [ProducesResponseType(typeof(IReadOnlyList<PolicyReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetReportPoliciesByCounty([FromQuery] PolicyReportFiltersDto reportFilters, CancellationToken cancellationToken)
    {
        var result = await policyReportService.GetReportPoliciesAsync(PolicyReportGroupType.County, reportFilters, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult(logger);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a report of policies grouped by city based on the provided filters.
    /// </summary>
    /// <param name="reportFilters">The filters to apply to the report.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of policy reports grouped by city.</returns>
    [HttpGet("policies-by-city")]
    [ProducesResponseType(typeof(IReadOnlyList<PolicyReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetReportPoliciesByCity([FromQuery] PolicyReportFiltersDto reportFilters, CancellationToken cancellationToken)
    {
        var result = await policyReportService.GetReportPoliciesAsync(PolicyReportGroupType.City, reportFilters, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult(logger);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a report of policies grouped by broker based on the provided filters.
    /// </summary>
    /// <param name="reportFilters">The filters to apply to the report.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A list of policy reports grouped by broker.</returns>
    [HttpGet("policies-by-broker")]
    [ProducesResponseType(typeof(IReadOnlyList<PolicyReportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetReportPoliciesByBroker([FromQuery] PolicyReportFiltersDto reportFilters, CancellationToken cancellationToken)
    {
        var result = await policyReportService.GetReportPoliciesAsync(PolicyReportGroupType.Broker, reportFilters, cancellationToken);

        if (!result.IsSuccess)
        {
            return result.Error!.ToProblemResult(logger);
        }

        return Ok(result.Value);
    }

}
