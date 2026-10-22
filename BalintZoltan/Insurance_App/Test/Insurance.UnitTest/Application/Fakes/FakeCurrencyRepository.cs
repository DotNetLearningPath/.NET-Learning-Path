using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Domain.Entities;

namespace Insurance.UnitTest.Application.Fakes;

public sealed class FakeCurrencyRepository : ICurrencyRepository
{
    public readonly List<Currency> Storage = [];

    public int AddCallCount { get; private set; }
    public int UpdateCallCount { get; private set; }

    public Task<Currency?> GetCurrencyByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Storage.FirstOrDefault(currency => currency.Id == id));
    }

    public Task AddCurrencyAsync(Currency currency, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Storage.Add(currency);
        AddCallCount++;
        return Task.CompletedTask;
    }

    public Task<Currency?> GetCurrencyByCodeAsync(string code, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Storage.FirstOrDefault(currency => currency.Code == code));
    }

    public Task<bool> ExistsCurrencyByCodeAsync(
        string code,
        Guid? excludedCurrencyId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Storage.Any(currency =>
            currency.Code == code && currency.Id != excludedCurrencyId));
    }

    public Task<PagedResult<Currency>> ListCurrenciesAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var allCurrencies = Storage
            .OrderBy(currency => currency.Code)
            .ThenBy(currency => currency.Id)
            .ToList();

        var pageNumber = Math.Max(pagination.PageNumber, 1);
        var pageSize = Math.Min(Math.Max(pagination.PageSize, 1), 100);

        return Task.FromResult(new PagedResult<Currency>
        {
            Items = [.. allCurrencies
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)],
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = allCurrencies.Count
        });
    }

    public Task UpdateCurrencyAsync(Currency currency, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        UpdateCallCount++;
        return Task.CompletedTask;
    }
}
