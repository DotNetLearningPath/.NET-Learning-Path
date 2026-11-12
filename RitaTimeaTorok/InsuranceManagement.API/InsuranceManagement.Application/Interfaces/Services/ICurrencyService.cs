using InsuranceManagement.Application.DTOs.Metadata;
using InsuranceManagement.Application.DTOs.Pagination;

namespace InsuranceManagement.Application.Interfaces.Services
{
    public interface ICurrencyService
    {
        Task<PagedResult<CurrencyDto>> GetAllCurrenciesAsync(int pageNumber, int pageSize);
        Task<CurrencyDto> CreateCurrencyAsync(CreateCurrencyDto request);
        Task<CurrencyDto> UpdateCurrencyAsync(Guid id, UpdateCurrencyDto request);
    }
}
