using InsuranceManagement.Application.DTOs.Building;

namespace InsuranceManagement.Application.Interfaces;

public interface IBuildingService
{
    Task<List<BuildingDto>> GetAllAsync();
    Task<BuildingDto?> GetAsync(Guid id);
    Task<BuildingDto> CreateAsync(CreateBuildingDto request);
    Task<BuildingDto?> UpdateAsync(Guid id, UpdateBuildingDto request);
    Task<bool> DeleteAsync(Guid id);
}
