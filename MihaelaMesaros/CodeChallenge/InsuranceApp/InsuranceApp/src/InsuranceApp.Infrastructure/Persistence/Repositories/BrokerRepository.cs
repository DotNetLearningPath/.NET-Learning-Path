using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Exceptions;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

internal sealed class BrokerRepository(InsuranceDbContext dbContext) : IBrokerRepository
{
    public async Task<IReadOnlyList<Broker>> GetBrokersAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Brokers
            .AsNoTracking()
            .OrderBy(x => x.BrokerCode)
            .ToListAsync(cancellationToken);
    }

    public Task<Broker?> GetBrokerByIdAsync(Guid brokerId, CancellationToken cancellationToken)
    {
        return dbContext.Brokers.AsNoTracking().FirstOrDefaultAsync(x => x.BrokerId == brokerId, cancellationToken);
    }

    public Task<Broker?> GetBrokerForUpdateAsync(Guid brokerId, CancellationToken cancellationToken)
    {
        return dbContext.Brokers.FirstOrDefaultAsync(x => x.BrokerId == brokerId, cancellationToken);
    }

    public Task<bool> BrokerCodeExistsAsync(string brokerCode, Guid? excludeBrokerId, CancellationToken cancellationToken)
    {
        return dbContext.Brokers.AnyAsync(
            x => x.BrokerCode == brokerCode && (!excludeBrokerId.HasValue || x.BrokerId != excludeBrokerId.Value),
            cancellationToken);
    }

    public async Task AddBrokerAsync(Broker broker, CancellationToken cancellationToken)
    {
        dbContext.Brokers.Add(broker);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (DbExceptionHelper.IsUniqueConstraintViolation(ex))
        {
            throw new DuplicateEntityException(nameof(Broker));
        }
    }

    public async Task SaveBrokerChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (DbExceptionHelper.IsUniqueConstraintViolation(ex))
        {
            throw new DuplicateEntityException(nameof(Broker));
        }
    }
}
