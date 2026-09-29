using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Exceptions;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence.Repositories;

internal sealed class ClientRepository(InsuranceDbContext dbContext) : IClientRepository
{
    public async Task<(IReadOnlyList<Client> Items, int TotalCount)> SearchClientAsync(string? name, string? identificationNumber, int pageNumber, int pageSize, CancellationToken cancellationToken)
    {
        var query = dbContext.Clients.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(x => x.Name.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(identificationNumber))
        {
            query = query.Where(x => x.IdentificationNumber == identificationNumber);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var clients = await query
            .OrderBy(x => x.Name)
            .ThenBy(x => x.ClientId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (clients, totalCount);
    }

    public Task<Client?> GetClientByIdAsync(Guid clientId, CancellationToken cancellationToken)
    {
        return dbContext.Clients.AsNoTracking().FirstOrDefaultAsync(x => x.ClientId == clientId, cancellationToken);
    }

    public Task<bool> ClientIdentificationNumberExistsAsync(string identificationNumber, CancellationToken cancellationToken)
    {
        return dbContext.Clients.AnyAsync(x => x.IdentificationNumber == identificationNumber, cancellationToken);
    }

    public async Task AddClientAsync(Client client, CancellationToken cancellationToken)
    {
        dbContext.Clients.Add(client);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (DbExceptionHelper.IsUniqueConstraintViolation(ex))
        {
            throw new DuplicateEntityException(nameof(Client));
        }
    }

    public Task SaveClientChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task<Client?> GetClientForUpdateAsync(Guid clientId, CancellationToken cancellationToken)
    {
        return dbContext.Clients.FirstOrDefaultAsync(x => x.ClientId == clientId, cancellationToken);
    }
}