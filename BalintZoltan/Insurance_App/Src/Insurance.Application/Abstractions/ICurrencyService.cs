using Insurance.Application.DTO.Common;
using Insurance.Application.DTO.Currencies;

namespace Insurance.Application.Abstractions;

public interface ICurrencyService
{
    Task<CurrencyDto> CreateCurrencyAsync(CreateCurrencyRequest request, CancellationToken cancellationToken);
    Task<CurrencyDto?> GetCurrencyByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<CurrencyDto>> ListCurrenciesAsync(PaginationRequest pagination, CancellationToken cancellationToken);
    Task<CurrencyDto> UpdateCurrencyAsync(Guid id, UpdateCurrencyRequest request, CancellationToken cancellationToken);
    Task<CurrencyDto> ActivateCurrencyAsync(Guid id, CancellationToken cancellationToken);
    Task<CurrencyDto> DeactivateCurrencyAsync(Guid id, CancellationToken cancellationToken);
}
