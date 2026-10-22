using FluentValidation;
using InsuranceApp.Application.Common.Validation;
using InsuranceApp.Application.Reports.Queries;
using InsuranceApp.Application.Reports.Results;
using Microsoft.Extensions.Logging;

namespace InsuranceApp.Application.Reports;

public class PolicyReportService(
    IPolicyReportRepository policyReportRepository,
    IValidator<PolicyReportQuery> queryValidator,
    ILogger<PolicyReportService> logger
) : IPolicyReportService
{
    private readonly IPolicyReportRepository _policyReportRepository = policyReportRepository;
    private readonly IValidator<PolicyReportQuery> _queryValidator = queryValidator;
    private readonly ILogger<PolicyReportService> _logger = logger;

    public async Task<IReadOnlyList<PolicyReportRowResult>> GetReportAsync(PolicyReportGrouping grouping, PolicyReportQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();

        if (!Enum.IsDefined(grouping))
        {
            throw ValidationExceptionFactory.Create(nameof(grouping), "Report grouping is invalid.");
        }

        await _queryValidator.ValidateAndThrowAsync(query, cancellationToken);

        var rows = await _policyReportRepository.GetReportAsync(grouping, query, cancellationToken);
        _logger.LogInformation(
            "Generated {Grouping} policy report from {From} to {To} with {RowCount} rows.",
            grouping, query.From, query.To, rows.Count
        );

        return rows;
    }
}
