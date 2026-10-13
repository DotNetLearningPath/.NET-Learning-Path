using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.RiskFactors.Commands;
using InsuranceApp.Application.RiskFactors.Results;

namespace InsuranceApp.Application.RiskFactors;

public interface IRiskFactorService
{
    Task<RiskFactorResult> GetRiskFactorByIdAsync(Guid riskFactorId, CancellationToken cancellationToken);
    
    Task<PagedResult<RiskFactorResult>> GetRiskFactorsAsync(PageQuery query, CancellationToken cancellationToken);
    
    Task<RiskFactorResult> CreateRiskFactorAsync(CreateRiskFactorCommand command, CancellationToken cancellationToken);
    
    Task<RiskFactorResult> UpdateRiskFactorAsync(UpdateRiskFactorCommand command, CancellationToken cancellationToken);
}
