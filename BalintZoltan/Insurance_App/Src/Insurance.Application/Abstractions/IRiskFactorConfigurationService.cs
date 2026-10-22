using Insurance.Application.DTO.Common;
using Insurance.Application.DTO.RiskFactors;

namespace Insurance.Application.Abstractions;

public interface IRiskFactorConfigurationService
{
    Task<PagedResult<RiskFactorConfigurationDto>> ListRiskFactorAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken);

    Task<RiskFactorConfigurationDto> CreateRiskFactorAsync(
        SaveRiskFactorConfigurationRequest request,
        CancellationToken cancellationToken);

    Task<RiskFactorConfigurationDto> UpdateRiskFactorAsync(
        Guid id,
        SaveRiskFactorConfigurationRequest request,
        CancellationToken cancellationToken);

    Task<RiskFactorConfigurationDto> ActivateRiskAsync(Guid id, CancellationToken cancellationToken);

    Task<RiskFactorConfigurationDto> DeactivateRiskFactorAsync(Guid id, CancellationToken cancellationToken);
}
