using InsuranceManagement.Application.DTOs.Metadata;

namespace InsuranceManagement.Application.Interfaces.Services
{
    public interface IRiskFactorConfigurationService
    {
        Task<IReadOnlyList<RiskFactorConfigurationDto>> GetAllRiskFactorConfigurationsAsync();
        Task<RiskFactorConfigurationDto> CreateRiskFactorConfigurationAsync(CreateRiskFactorConfigurationDto request);
        Task<RiskFactorConfigurationDto> UpdateRiskFactorConfigurationAsync(Guid id, UpdateRiskFactorConfigurationDto request);
    }
}
