using InsuranceManagement.Application.DTOs.Geography;

namespace InsuranceManagement.Application.Interfaces;

public interface ICityService
{
    Task<List<CityDto>> GetAllAsync();
    Task<CityDto?> GetAsync(Guid id);
    Task<CityDto> CreateAsync(CreateCityDto request);
    Task<CityDto?> UpdateAsync(Guid id, UpdateCityDto request);
    Task<bool> DeleteAsync(Guid id);
}
