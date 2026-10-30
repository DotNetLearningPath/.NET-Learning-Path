using InsuranceManagement.Application.DTOs.Geography;

namespace InsuranceManagement.Application.Interfaces;

public interface ICountyService
{
    Task<List<CountyDto>> GetAllAsync();
    Task<CountyDto?> GetAsync(Guid id);
    Task<CountyDto> CreateAsync(CreateCountyDto request);
    Task<CountyDto?> UpdateAsync(Guid id, UpdateCountyDto request);
    Task<bool> DeleteAsync(Guid id);
}
