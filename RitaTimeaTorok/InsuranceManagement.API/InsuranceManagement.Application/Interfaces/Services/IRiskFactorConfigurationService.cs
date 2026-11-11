using InsuranceManagement.Application.DTOs.Metadata;
using InsuranceManagement.Application.DTOs.Pagination;

namespace InsuranceManagement.Application.Interfaces.Services
{
    public interface IRiskFactorConfigurationService
    {
        Task<PagedResult<RiskFactorConfigurationDto>> GetAllRiskFactorConfigurationsAsync(int pageNumber, int pageSize);
        Task<RiskFactorConfigurationDto> CreateRiskFactorConfigurationAsync(CreateRiskFactorConfigurationDto request);
        Task<RiskFactorConfigurationDto> UpdateRiskFactorConfigurationAsync(Guid id, UpdateRiskFactorConfigurationDto request);
    }
}
