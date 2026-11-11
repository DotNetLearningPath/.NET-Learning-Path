using InsuranceManagement.Application.DTOs.Metadata;

namespace InsuranceManagement.Application.Interfaces.Services
{
    public interface ICurrencyService
    {
        Task<IReadOnlyList<CurrencyDto>> GetAllCurrenciesAsync();
        Task<CurrencyDto> CreateCurrencyAsync(CreateCurrencyDto request);
        Task<CurrencyDto> UpdateCurrencyAsync(Guid id, UpdateCurrencyDto request);
    }
}
