using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.RiskFactorConfig;

namespace InsuranceApp.Application.Abstractions.Services;

public interface IRiskFactorConfigService
{
    Task<Result<IReadOnlyList<RiskFactorConfigDto>>> GetRiskFactorConfigsAsync(CancellationToken cancellationToken);

    Task<Result<RiskFactorConfigDto>> GetRiskFactorConfigByIdAsync(Guid riskFactorConfigId, CancellationToken cancellationToken);

    Task<Result<RiskFactorConfigDto>> CreateRiskFactorConfigAsync(CreateRiskFactorConfigDto createRiskFactorConfigDto, CancellationToken cancellationToken);

    Task<Result<RiskFactorConfigDto>> UpdateRiskFactorConfigAsync(Guid riskFactorConfigId, UpdateRiskFactorConfigDto updateRiskFactorConfigDto, CancellationToken cancellationToken);
}