using InsuranceApp.Application.Common.Exceptions;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Currencies;
using InsuranceApp.Application.Currencies.Exceptions;
using InsuranceApp.Domain.Currencies;
using InsuranceApp.Infrastructure.Persistence.Mappings;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly InsuranceDbContext _context;

    public CurrencyRepository(InsuranceDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public async Task<Currency?> GetCurrencyByIdAsync(Guid currencyId, CancellationToken cancellationToken)
    {
        var entity = await _context.Currencies
            .AsNoTracking()
            .SingleOrDefaultAsync(currency => currency.Id == currencyId, cancellationToken);

        return entity?.ToDomain();
    }

    public Task<PagedResult<Currency>> GetCurrenciesAsync(PageQuery query, CancellationToken cancellationToken)
    {
        return _context.Currencies
            .AsNoTracking()
            .OrderBy(currency => currency.Code)
            .ToDomainPageAsync(query.PageNumber, query.PageSize, entity => entity.ToDomain(), cancellationToken);
    }

    public Task<bool> CurrencyExistsByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return _context.Currencies.AnyAsync(
            currency => currency.Code == code,
            cancellationToken
        );
    }

    public async Task AddCurrencyAsync(Currency currency, CancellationToken cancellationToken)
    {
        _context.Currencies.Add(currency.ToEntity());

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: DatabaseNames.CurrencyCodeIndex
        })
        {
            throw new DuplicateCurrencyCodeException(exception);
        }
    }

    public async Task UpdateCurrencyAsync(Currency currency, CancellationToken cancellationToken)
    {
        var affected = await _context.Currencies
            .Where(entity => entity.Id == currency.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(entity => entity.Name, currency.Name)
                .SetProperty(entity => entity.ExchangeRateToBase, currency.ExchangeRateToBase)
                .SetProperty(entity => entity.IsActive, currency.IsActive), cancellationToken);

        if (affected == 0)
        {
            throw new EntityNotFoundException(nameof(Currency), currency.Id);
        }
    }
}
