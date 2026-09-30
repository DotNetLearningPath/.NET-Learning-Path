using Insurance.Domain.Entities;
using Insurance.Application.DTO.Common;

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
