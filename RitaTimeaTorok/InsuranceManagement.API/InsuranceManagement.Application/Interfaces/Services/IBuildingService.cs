using InsuranceManagement.Application.DTOs.Building;

using InsuranceManagement.Application.DTOs.Pagination;

namespace InsuranceManagement.Application.Interfaces.Services
{
    public interface IBuildingService
    {
        Task<PagedResult<BuildingDto>> GetAllBuildingsAsync(int pageNumber, int pageSize);
        Task<BuildingDetailsDto> GetBuildingByIdAsync(Guid id);
        Task<BuildingDto> CreateBuildingAsync(CreateBuildingDto request);
        Task<BuildingDto> UpdateBuildingAsync(Guid id, UpdateBuildingDto request);
        Task DeleteBuildingAsync(Guid id);
    }
}
