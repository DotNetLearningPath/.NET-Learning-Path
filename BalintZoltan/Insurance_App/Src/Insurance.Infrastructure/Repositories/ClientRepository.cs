using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Domain.Entities;
using Insurance.Infrastructure.Extensions;
using Insurance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Repositories;

public sealed class ClientRepository(
    InsuranceDbContext dbContext) : IClientRepository
{
    public async Task AddClientAsync(Client client, CancellationToken cancellationToken)
    {
        await dbContext.Clients.AddAsync(client, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsClientByIdentificationNumberAsync(
        string identificationNumber,
        CancellationToken cancellationToken,
        Guid? excludedClientId = null)
    {
        return await dbContext.Clients.AnyAsync(client =>
            client.IdentificationNumber == identificationNumber
            && (!excludedClientId.HasValue
                || client.Id != excludedClientId.Value), cancellationToken);
    }

    public async Task<Client?> GetClientByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await dbContext.Clients
            .FirstOrDefaultAsync(client => client.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Client>> SearchClientAsync(
        string? name,
        string? identifier,
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        var query = dbContext.Clients
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var normalizedName = name.Trim();
            query = query.Where(client =>
                client.Name.Contains(normalizedName));
        }

        if (!string.IsNullOrWhiteSpace(identifier))
        {
            var normalizedIdentifier = identifier.Trim();
            query = query.Where(client =>
                client.IdentificationNumber == normalizedIdentifier);
        }

        return await query
            .OrderBy(client => client.Name)
            .ThenBy(client => client.Id)
            .ToPagedResultAsync(pagination, cancellationToken);
    }

    public async Task UpdateClientAsync(Client client, CancellationToken cancellationToken)
    {
        dbContext.Clients.Update(client);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
