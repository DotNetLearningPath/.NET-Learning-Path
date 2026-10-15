using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Reports;
using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Application.Services;

public sealed class PolicyReportService(IPolicyReportRepository policyReportRepository) : IPolicyReportService
{
    public async Task<Result<IReadOnlyList<PolicyReportDto>>> GetReportPoliciesAsync(
        PolicyReportGroupType groupType, 
        PolicyReportFiltersDto policyReportFiltersDto, 
        CancellationToken cancellationToken
    )
    {
        var result = await policyReportRepository.GetReportPoliciesAsync(groupType, policyReportFiltersDto, cancellationToken);

        return Result<IReadOnlyList<PolicyReportDto>>.Success(result);
    }
}
