using InsuranceManagement.Application.DTOs.Geography;

using InsuranceManagement.Application.DTOs.Pagination;

namespace InsuranceManagement.Application.Interfaces.Services
{
    public interface ICountyService
    {
        Task<PagedResult<CountyDto>> GetAllCountiesAsync(int pageNumber, int pageSize);
        Task<CountyDto> GetCountyByIdAsync(Guid id);
        Task<CountyDto> CreateCountyAsync(CreateCountyDto request);
        Task<CountyDto> UpdateCountyAsync(Guid id, UpdateCountyDto request);
        Task DeleteCountyAsync(Guid id);
    }
}
