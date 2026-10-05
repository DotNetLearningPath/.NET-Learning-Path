using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Extensions;
using Insurance.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories;

public sealed class BrokerRepository(
    InsuranceDbContext dbContext) : IBrokerRepository
{
    public async Task AddBrokerAsync(
        Broker broker,
        CancellationToken cancellationToken)
    {
        await dbContext.Brokers.AddAsync(broker, cancellationToken);
        await SaveChangesAsync(cancellationToken);
    }

    public Task<Broker?> GetBrokerByIdAsync(
        Guid id,
        CancellationToken cancellationToken) =>
        dbContext.Brokers
            .AsNoTracking()
            .FirstOrDefaultAsync(broker => broker.Id == id, cancellationToken);

    public Task<Broker?> GetBrokerByCodeAsync(
        string brokerCode,
        CancellationToken cancellationToken) =>
        dbContext.Brokers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                broker => broker.BrokerCode == brokerCode,
                cancellationToken);

    public async Task<PagedResult<Broker>> ListBrokersAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Brokers.AsNoTracking();
        return await query
            .OrderBy(broker => broker.Name)
            .ThenBy(broker => broker.Id)
            .ToPagedResultAsync(pagination, cancellationToken);
    }

    public async Task UpdateBrokerAsync(
        Broker broker,
        CancellationToken cancellationToken)
    {
        dbContext.Brokers.Update(broker);
        await SaveChangesAsync(cancellationToken);
    }

    private async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
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
