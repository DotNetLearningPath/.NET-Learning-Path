using Insurance.Application.DTO.Common;
using Insurance.Domain.Entities;

namespace Insurance.Application.Abstractions;

public interface ICurrencyRepository
{
    Task AddCurrencyAsync(Currency currency, CancellationToken cancellationToken);

    Task<Currency?> GetCurrencyByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<Currency?> GetCurrencyByCodeAsync(
        string code,
        CancellationToken cancellationToken);

    Task<PagedResult<Currency>> ListCurrenciesAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken);
}
