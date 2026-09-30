using Insurance.Domain.Entities;
using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Infrastructure.Extensions;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories;

public sealed class CurrencyRepository : ICurrencyRepository
{
    private readonly InsuranceDbContext _dbContext;

    public CurrencyRepository(InsuranceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddCurrencyAsync(
        Currency currency,
        CancellationToken cancellationToken)
    {
        await _dbContext.Currencies.AddAsync(currency, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Currency?> GetCurrencyByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        _dbContext.Currencies
            .AsNoTracking()
            .FirstOrDefaultAsync(currency => currency.Id == id, cancellationToken);

    public Task<Currency?> GetCurrencyByCodeAsync(
        string code,
        CancellationToken cancellationToken) =>
        _dbContext.Currencies
            .AsNoTracking()
            .FirstOrDefaultAsync(currency => currency.Code == code, cancellationToken);

    public async Task<PagedResult<Currency>> ListCurrenciesAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Currencies.AsNoTracking();
        return await query
            .OrderBy(currency => currency.Code)
            .ThenBy(currency => currency.Id)
            .ToPagedResultAsync(pagination, cancellationToken);
    }
}
