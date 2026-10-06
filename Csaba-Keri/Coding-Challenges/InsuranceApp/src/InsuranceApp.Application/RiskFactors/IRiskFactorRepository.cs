using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Domain.Buildings;
using InsuranceApp.Domain.RiskFactors;

namespace InsuranceApp.Application.RiskFactors;

public interface IRiskFactorRepository
{
    Task<RiskFactorConfiguration?> GetRiskFactorByIdAsync(Guid riskFactorId, CancellationToken cancellationToken);
    
    Task<PagedResult<RiskFactorConfiguration>> GetRiskFactorsAsync(PageQuery query, CancellationToken cancellationToken);
    
    Task<IReadOnlyList<RiskFactorConfiguration>> GetApplicableRiskFactorsAsync(Guid cityId, BuildingType buildingType, CancellationToken cancellationToken);
    
    Task AddRiskFactorAsync(RiskFactorConfiguration riskFactor, CancellationToken cancellationToken);
    
    Task UpdateRiskFactorAsync(RiskFactorConfiguration riskFactor, CancellationToken cancellationToken);
}
