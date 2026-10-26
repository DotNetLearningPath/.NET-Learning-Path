using InsuranceApp.Application.Reports.Queries;
using InsuranceApp.Application.Reports.Results;

namespace InsuranceApp.Application.Reports;

public interface IPolicyReportRepository
{
    Task<IReadOnlyList<PolicyReportRowResult>> GetReportAsync(PolicyReportGrouping grouping, PolicyReportQuery query, CancellationToken cancellationToken);
}
