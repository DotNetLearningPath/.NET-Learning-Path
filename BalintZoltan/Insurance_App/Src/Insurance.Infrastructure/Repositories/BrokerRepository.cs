using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Extensions;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;

namespace Insurance.Infrastructure.Repositories;

public sealed class BrokerRepository : IBrokerRepository
{
    private readonly InsuranceDbContext _dbContext;

    public BrokerRepository(InsuranceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddBrokerAsync(
        Broker broker,
        CancellationToken cancellationToken)
    {
        await _dbContext.Brokers.AddAsync(broker, cancellationToken);
        await SaveChangesAsync(cancellationToken);
    }

    public Task<Broker?> GetBrokerByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        _dbContext.Brokers
            .AsNoTracking()
            .FirstOrDefaultAsync(broker => broker.Id == id, cancellationToken);

    public Task<Broker?> GetBrokerByCodeAsync(
        string brokerCode,
        CancellationToken cancellationToken) =>
        _dbContext.Brokers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                broker => broker.BrokerCode == brokerCode,
                cancellationToken);

    public async Task<PagedResult<Broker>> ListBrokersAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Brokers.AsNoTracking();
        return await query
            .OrderBy(broker => broker.Name)
            .ThenBy(broker => broker.Id)
            .ToPagedResultAsync(pagination, cancellationToken);
    }

    public async Task UpdateBrokerAsync(
        Broker broker,
        CancellationToken cancellationToken)
    {
        _dbContext.Brokers.Update(broker);
        await SaveChangesAsync(cancellationToken);
    }

    private async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsBrokerCodeUniqueViolation(exception))
        {
            throw new InvalidOperationException(
                "A broker with this code already exists.",
                exception);
        }
    }

    private static bool IsBrokerCodeUniqueViolation(DbUpdateException exception)
    {
        if (exception.InnerException is not SqliteException sqliteException)
        {
            return false;
        }

        return sqliteException.SqliteErrorCode == 19
            && sqliteException.Message.Contains(
                "BrokerCode",
                StringComparison.OrdinalIgnoreCase);
    }
}
