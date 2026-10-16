using InsuranceApp.Application.DTOs.Reports;
using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Application.Abstractions.Persistence;

public interface IPolicyReportRepository
{
    Task<IReadOnlyList<PolicyReportDto>> GetReportPoliciesAsync(
        PolicyReportGroupType groupType, 
        PolicyReportFiltersDto policyReportFiltersDto, 
        CancellationToken cancellationToken
    );
}
