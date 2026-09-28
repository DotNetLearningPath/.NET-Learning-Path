using Application.Abstractions;
using Application.DTO.Common;
using Domain.Entities;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class BrokerRepository : IBrokerRepository
{
    private readonly InsuranceDbContext _dbContext;

    public BrokerRepository(InsuranceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddBrokerAsync(
        Broker broker,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Brokers.AddAsync(broker, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Broker?> GetBrokerByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default) =>
        _dbContext.Brokers
            .AsNoTracking()
            .FirstOrDefaultAsync(broker => broker.Id == id, cancellationToken);

    public Task<Broker?> GetBrokerByCodeAsync(
        string brokerCode,
        CancellationToken cancellationToken = default) =>
        _dbContext.Brokers
            .AsNoTracking()
            .FirstOrDefaultAsync(
                broker => broker.BrokerCode == brokerCode,
                cancellationToken);

    public async Task<PagedResult<Broker>> ListBrokersAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Brokers.AsNoTracking();
        return await query
            .OrderBy(broker => broker.Name)
            .ThenBy(broker => broker.Id)
            .ToPagedResultAsync(pagination, cancellationToken);
    }
}
