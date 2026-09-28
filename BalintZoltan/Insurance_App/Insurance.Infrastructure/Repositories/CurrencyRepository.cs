using Application.Abstractions;
using Application.DTO.Common;
using Domain.Entities;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class CurrencyRepository : ICurrencyRepository
{
    private readonly InsuranceDbContext _dbContext;

    public CurrencyRepository(InsuranceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddCurrencyAsync(
        Currency currency,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Currencies.AddAsync(currency, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Currency?> GetCurrencyByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        _dbContext.Currencies
            .AsNoTracking()
            .FirstOrDefaultAsync(currency => currency.Id == id, cancellationToken);

    public Task<Currency?> GetCurrencyByCodeAsync(
        string code,
        CancellationToken cancellationToken = default) =>
        _dbContext.Currencies
            .AsNoTracking()
            .FirstOrDefaultAsync(currency => currency.Code == code, cancellationToken);

    public async Task<PagedResult<Currency>> ListCurrenciesAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Currencies.AsNoTracking();
        return await query
            .OrderBy(currency => currency.Code)
            .ThenBy(currency => currency.Id)
            .ToPagedResultAsync(pagination, cancellationToken);
    }
}
