using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Domain.Entities;

namespace Insurance.UnitTest.Application.Fakes;

public sealed class FakeCurrencyRepository : ICurrencyRepository
{
    public Currency? Item { get; set; }

    public Task<Currency?> GetCurrencyByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Task.FromResult(Item?.Id == id ? Item : null);

    public Task AddCurrencyAsync(Currency currency, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task<Currency?> GetCurrencyByCodeAsync(string code, CancellationToken cancellationToken) =>
        Task.FromResult(Item?.Code == code ? Item : null);

    public Task<PagedResult<Currency>> ListCurrenciesAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken) => Task.FromResult(new PagedResult<Currency>());
}
