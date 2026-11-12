using InsuranceManagement.Application.DTOs.Metadata;
using InsuranceManagement.Application.DTOs.Pagination;

namespace InsuranceManagement.Application.Interfaces.Services
{
    public interface IFeeConfigurationService
    {
        Task<PagedResult<FeeConfigurationDto>> GetAllFeeConfigurationsAsync(int pageNumber, int pageSize);
        Task<FeeConfigurationDto> CreateFeeConfigurationAsync(CreateFeeConfigurationDto request);
        Task<FeeConfigurationDto> UpdateFeeConfigurationAsync(Guid id, UpdateFeeConfigurationDto request);
    }
}
