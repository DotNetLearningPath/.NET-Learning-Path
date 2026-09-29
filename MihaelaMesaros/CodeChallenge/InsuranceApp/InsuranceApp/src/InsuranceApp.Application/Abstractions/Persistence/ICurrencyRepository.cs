using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Currency;
using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Application.Abstractions.Persistence;

public interface ICurrencyRepository
{
    Task<IReadOnlyList<Currency>> GetCurrenciesAsync(CancellationToken cancellationToken);

    Task<Currency?> GetCurrencyByIdAsync(Guid currencyId, CancellationToken cancellationToken);

    Task<Currency?> GetCurrencyForUpdateAsync(Guid currencyId, CancellationToken cancellationToken);

    Task<bool> CurrencyCodeExistsAsync(string code, Guid? excludeCurrencyId, CancellationToken cancellationToken);

    Task AddCurrencyAsync(Currency currency, CancellationToken cancellationToken);

    Task SaveCurrencyChangesAsync(CancellationToken cancellationToken);
}
