using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Domain.Currencies;

namespace InsuranceApp.Application.Currencies;

public interface ICurrencyRepository
{
    Task<Currency?> GetCurrencyByIdAsync(Guid currencyId, CancellationToken cancellationToken);
    
    Task<PagedResult<Currency>> GetCurrenciesAsync(PageQuery query, CancellationToken cancellationToken);
    
    Task<bool> CurrencyExistsByCodeAsync(string code, CancellationToken cancellationToken);
    
    Task AddCurrencyAsync(Currency currency, CancellationToken cancellationToken);
    
    Task UpdateCurrencyAsync(Currency currency, CancellationToken cancellationToken);
}
