using InsuranceManagement.Application.DTOs.Metadata;

namespace InsuranceManagement.Application.Interfaces.Services
{
    public interface IFeeConfigurationService
    {
        Task<IReadOnlyList<FeeConfigurationDto>> GetAllFeeConfigurationsAsync();
        Task<FeeConfigurationDto> CreateFeeConfigurationAsync(CreateFeeConfigurationDto request);
        Task<FeeConfigurationDto> UpdateFeeConfigurationAsync(Guid id, UpdateFeeConfigurationDto request);
    }
}
