using InsuranceApp.Application.Clients;
using InsuranceApp.Application.Clients.Exceptions;
using InsuranceApp.Application.Clients.Queries;
using InsuranceApp.Application.Common.Exceptions;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Domain.Clients;
using InsuranceApp.Infrastructure.Persistence.Mappings;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly InsuranceDbContext _context;

    public ClientRepository(InsuranceDbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        _context = context;
    }

    public Task<bool> ClientExistsByIdAsync(Guid clientId, CancellationToken cancellationToken)
    {
        return _context.Clients.AnyAsync(
            client => client.Id == clientId,
            cancellationToken
        );
    }

    public Task<bool> ClientExistsByIdentificationNumberAsync(string identificationNumber, CancellationToken cancellationToken)
    {
        return _context.Clients.AnyAsync(
            client => client.IdentificationNumber == identificationNumber,
            cancellationToken
        );
    }

    public async Task<Client?> GetClientByIdAsync(Guid clientId, CancellationToken cancellationToken)
    {
        var entity = await _context.Clients
            .AsNoTracking()
            .SingleOrDefaultAsync(client => client.Id == clientId, cancellationToken);

        return entity?.ToDomain();
    }

    public async Task AddClientAsync(Client client, CancellationToken cancellationToken)
    {
        _context.Clients.Add(client.ToEntity());

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: DatabaseNames.ClientIdentifierIndex
        })
        {
            throw new DuplicateClientIdentificationException(exception);
        }
    }

    public async Task UpdateClientAsync(Client client, CancellationToken cancellationToken)
    {
        var affected = await _context.Clients
            .Where(entity => entity.Id == client.Id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(entity => entity.Name, client.Name)
                .SetProperty(entity => entity.Email, client.Email)
                .SetProperty(entity => entity.Phone, client.Phone)
                .SetProperty(entity => entity.PrimaryAddress, client.PrimaryAddress), cancellationToken);

        if (affected == 0)
        {
            throw new EntityNotFoundException(nameof(Client), client.Id);
        }
    }

    public Task<PagedResult<Client>> SearchClientsAsync(SearchClientsQuery query, CancellationToken cancellationToken)
    {
        var clients = _context.Clients.AsNoTracking();

        if (query.Name is not null)
        {
            var pattern = "%" + EscapeLike(query.Name) + "%";
            clients = clients.Where(client => EF.Functions.ILike(client.Name, pattern, "\\"));
        }

        if (query.Identifier is not null)
        {
            clients = clients.Where(client => client.IdentificationNumber == query.Identifier);
        }

        return clients
            .OrderBy(client => client.Name)
            .ThenBy(client => client.Id)
            .ToDomainPageAsync(query.PageNumber, query.PageSize, entity => entity.ToDomain(), cancellationToken);
    }

    private static string EscapeLike(string value)
    {
        return value
            .Replace("\\", "\\\\")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
    }
}
