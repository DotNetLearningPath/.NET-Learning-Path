using Application.DTO.Common;
using Domain.Entities;

namespace Application.Abstractions;

public interface ICurrencyRepository
{
    Task AddCurrencyAsync(Currency currency, CancellationToken cancellationToken = default);

    Task<Currency?> GetCurrencyByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Currency?> GetCurrencyByCodeAsync(
        string code,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Currency>> ListCurrenciesAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);
}
