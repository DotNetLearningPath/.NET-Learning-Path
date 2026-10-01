using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;

namespace InsuranceApp.Application.Abstractions.Persistence;

public interface IRiskFactorConfigRepository
{
    Task<IReadOnlyList<RiskFactorConfig>> GetRiskFactorConfigsAsync(CancellationToken cancellationToken);

    Task<RiskFactorConfig?> GetRiskFactorConfigByIdAsync(Guid riskFactorConfigId, CancellationToken cancellationToken);

    Task<RiskFactorConfig?> GetRiskFactorConfigForUpdateAsync(Guid riskFactorConfigId, CancellationToken cancellationToken);

    Task<bool> RiskFactorConfigExistsAsync(RiskFactorLevel level, Guid referenceId, Guid? excludeRiskFactorConfigId, CancellationToken cancellationToken);

    Task<bool> ReferenceExistsAsync(RiskFactorLevel level, Guid referenceId, CancellationToken cancellationToken);

    Task AddRiskFactorConfigAsync(RiskFactorConfig riskFactorConfig, CancellationToken cancellationToken);

    Task SaveRiskFactorConfigChangesAsync(CancellationToken cancellationToken);
}