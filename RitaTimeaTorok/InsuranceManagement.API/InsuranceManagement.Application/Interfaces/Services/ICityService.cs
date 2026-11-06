using InsuranceManagement.Application.DTOs.Geography;

using InsuranceManagement.Application.DTOs.Pagination;

namespace InsuranceManagement.Application.Interfaces.Services
{
    public interface ICityService
    {
        Task<PagedResult<CityDto>> GetAllCitiesAsync(int pageNumber, int pageSize);
        Task<CityDto> GetCityByIdAsync(Guid id);
        Task<CityDto> CreateCityAsync(CreateCityDto request);
        Task<CityDto> UpdateCityAsync(Guid id, UpdateCityDto request);
        Task DeleteCityAsync(Guid id);
    }
}
