using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Extensions;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories;

public sealed class CurrencyRepository(
    InsuranceDbContext dbContext) : ICurrencyRepository
{
    public async Task AddCurrencyAsync(
        Currency currency,
        CancellationToken cancellationToken)
    {
        await dbContext.Currencies.AddAsync(currency, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Currency?> GetCurrencyByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.Currencies
            .AsNoTracking()
            .FirstOrDefaultAsync(currency => currency.Id == id, cancellationToken);

    public Task<Currency?> GetCurrencyByCodeAsync(
        string code,
        CancellationToken cancellationToken) =>
        dbContext.Currencies
            .AsNoTracking()
            .FirstOrDefaultAsync(currency => currency.Code == code, cancellationToken);

    public Task<bool> ExistsCurrencyByCodeAsync(
        string code,
        Guid? excludedCurrencyId,
        CancellationToken cancellationToken) =>
        dbContext.Currencies.AnyAsync(currency =>
            currency.Code == code
            && (!excludedCurrencyId.HasValue || currency.Id != excludedCurrencyId.Value),
            cancellationToken);

    public async Task<PagedResult<Currency>> ListCurrenciesAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Currencies.AsNoTracking();
        return await query
            .OrderBy(currency => currency.Code)
            .ThenBy(currency => currency.Id)
            .ToPagedResultAsync(pagination, cancellationToken);
    }

    public async Task UpdateCurrencyAsync(Currency currency, CancellationToken cancellationToken)
    {
        dbContext.Currencies.Update(currency);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
