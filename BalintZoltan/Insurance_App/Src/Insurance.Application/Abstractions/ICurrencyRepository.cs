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

    Task<bool> ExistsCurrencyByCodeAsync(
        string code,
        Guid? excludedCurrencyId,
        CancellationToken cancellationToken);

    Task<PagedResult<Currency>> ListCurrenciesAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken);

    Task UpdateCurrencyAsync(
        Currency currency,
        CancellationToken cancellationToken);
}
