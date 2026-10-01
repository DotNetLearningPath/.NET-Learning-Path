using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Exceptions;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

internal sealed class CurrencyRepository(InsuranceDbContext dbContext) : ICurrencyRepository
{
    public async Task<IReadOnlyList<Currency>> GetCurrenciesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Currencies
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);
    }

    public Task<Currency?> GetCurrencyByIdAsync(Guid currencyId, CancellationToken cancellationToken)
    {
        return dbContext.Currencies.AsNoTracking().FirstOrDefaultAsync(x => x.CurrencyId == currencyId, cancellationToken);
    }

    public Task<Currency?> GetCurrencyForUpdateAsync(Guid currencyId, CancellationToken cancellationToken)
    {
        return dbContext.Currencies.FirstOrDefaultAsync(x => x.CurrencyId == currencyId, cancellationToken);
    }

    public Task<bool> CurrencyCodeExistsAsync(string code, Guid? excludeCurrencyId, CancellationToken cancellationToken)
    {
        return dbContext.Currencies.AnyAsync(
            x => x.Code == code && (!excludeCurrencyId.HasValue || x.CurrencyId != excludeCurrencyId.Value),
            cancellationToken
        );
    }

    public async Task AddCurrencyAsync(Currency currency, CancellationToken cancellationToken)
    {
        dbContext.Currencies.Add(currency);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (DbExceptionHelper.IsUniqueConstraintViolation(ex))
        {
            throw new DuplicateEntityException(nameof(Currency));
        }
    }

    public async Task SaveCurrencyChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (DbExceptionHelper.IsUniqueConstraintViolation(ex))
        {
            throw new DuplicateEntityException(nameof(Currency));
        }
    }
}