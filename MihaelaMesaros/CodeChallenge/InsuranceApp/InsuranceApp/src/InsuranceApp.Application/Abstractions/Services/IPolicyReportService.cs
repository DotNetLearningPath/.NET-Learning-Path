using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Reports;
using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Application.Abstractions.Services;

public interface IPolicyReportService
{
    Task<Result<IReadOnlyList<PolicyReportDto>>> GetReportPoliciesAsync(PolicyReportGroupType groupType, PolicyReportFiltersDto policyReportFiltersDto, CancellationToken cancellationToken);
}
