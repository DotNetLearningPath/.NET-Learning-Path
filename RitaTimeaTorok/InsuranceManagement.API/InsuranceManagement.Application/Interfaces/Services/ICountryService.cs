using InsuranceManagement.Application.DTOs.Geography;

using InsuranceManagement.Application.DTOs.Pagination;

namespace InsuranceManagement.Application.Interfaces.Services
{
    public interface ICountryService
    {
        Task<PagedResult<CountryDto>> GetAllCountriesAsync(int pageNumber, int pageSize);
        Task<CountryDto> GetCountryByIdAsync(Guid id);
        Task<CountryDto> CreateCountryAsync(CreateCountryDto request);
        Task<CountryDto> UpdateCountryAsync(Guid id, UpdateCountryDto request);
        Task DeleteCountryAsync(Guid id);
    }
}
