using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Currencies.Commands;
using InsuranceApp.Application.Currencies.Results;

namespace InsuranceApp.Application.Currencies;

public interface ICurrencyService
{
    Task<CurrencyResult> GetCurrencyByIdAsync(Guid currencyId, CancellationToken cancellationToken);
    
    Task<PagedResult<CurrencyResult>> GetCurrenciesAsync(PageQuery query, CancellationToken cancellationToken);
    
    Task<CurrencyResult> CreateCurrencyAsync(CreateCurrencyCommand command, CancellationToken cancellationToken);
    
    Task<CurrencyResult> UpdateCurrencyAsync(UpdateCurrencyCommand command, CancellationToken cancellationToken);
}
