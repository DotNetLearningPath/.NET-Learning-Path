using InsuranceManagement.Application.DTOs.Geography;

namespace InsuranceManagement.Application.Interfaces.Services
{
    public interface ICountryService
    {
        Task<List<CountryDto>> GetAllAsync();
        Task<CountryDto> GetAsync(Guid id);
        Task<CountryDto> CreateAsync(CreateCountryDto request);
        Task<CountryDto> UpdateAsync(Guid id, UpdateCountryDto request);
        Task DeleteAsync(Guid id);
    }
}
