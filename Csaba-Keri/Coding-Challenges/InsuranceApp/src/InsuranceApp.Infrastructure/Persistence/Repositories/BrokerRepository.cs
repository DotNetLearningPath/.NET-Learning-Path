using InsuranceApp.Application.Brokers;
using InsuranceApp.Application.Brokers.Exceptions;
using InsuranceApp.Application.Common.Exceptions;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Domain.Brokers;
using InsuranceApp.Infrastructure.Persistence.Mappings;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

public class BrokerRepository(InsuranceDbContext context) : IBrokerRepository
{
    private readonly InsuranceDbContext _context = context;

    public async Task<Broker?> GetBrokerByIdAsync(Guid brokerId, CancellationToken cancellationToken)
    {
        var entity = await _context.Brokers
            .AsNoTracking()
            .SingleOrDefaultAsync(broker => broker.Id == brokerId, cancellationToken);

        return entity?.ToDomain();
    }

    public Task<PagedResult<Broker>> GetBrokersAsync(PageQuery query, CancellationToken cancellationToken)
    {
        return _context.Brokers
            .AsNoTracking()
            .OrderBy(broker => broker.Name)
            .ThenBy(broker => broker.Id)
            .ToDomainPageAsync(query.PageNumber, query.PageSize, entity => entity.ToDomain(), cancellationToken);
    }

    public Task<bool> BrokerExistsByCodeAsync(string code, CancellationToken cancellationToken)
    {
        return _context.Brokers.AnyAsync(
            broker => broker.Code == code,
            cancellationToken
        );
    }

    public async Task AddBrokerAsync(Broker broker, CancellationToken cancellationToken)
    {
        _context.Brokers.Add(broker.ToEntity());

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: DatabaseNames.BrokerCodeIndex
        })
        {
            throw new DuplicateBrokerCodeException(exception);
        }
    }

    public async Task UpdateBrokerAsync(Broker broker, CancellationToken cancellationToken)
    {
        var affected = await _context.Brokers
            .Where(entity => entity.Id == broker.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(entity => entity.Name, broker.Name)
                .SetProperty(entity => entity.Email, broker.Email)
                .SetProperty(entity => entity.Phone, broker.Phone)
                .SetProperty(entity => entity.Status, broker.Status), cancellationToken);

        if (affected == 0)
        {
            throw new EntityNotFoundException(nameof(Broker), broker.Id);
        }
    }
}
