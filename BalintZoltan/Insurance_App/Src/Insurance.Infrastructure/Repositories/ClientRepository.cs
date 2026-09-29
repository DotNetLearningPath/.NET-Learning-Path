using Application.Abstractions;
using Application.DTO.Common;
using Domain.Entities;
using Infrastructure.Extensions;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public sealed class ClientRepository : IClientRepository
{
    private readonly InsuranceDbContext _dbContext;

    public ClientRepository(InsuranceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddClientAsync(Client client, CancellationToken cancellationToken = default)
    {
        await _dbContext.Clients.AddAsync(client, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsClientByIdentificationNumberAsync(
        string identificationNumber,
        Guid? excludedClientId = null,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Clients.AnyAsync(client =>
            client.IdentificationNumber == identificationNumber
            && (!excludedClientId.HasValue
                || client.Id != excludedClientId.Value), cancellationToken);
    }

    public async Task<Client?> GetClientByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Clients
            .FirstOrDefaultAsync(client => client.Id == id, cancellationToken);
    }

    public async Task<PagedResult<Client>> SearchClientAsync(
        string? name,
        string? identifier,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Clients
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

    public async Task UpdateClientAsync(Client client, CancellationToken cancellationToken = default)
    {
        _dbContext.Clients.Update(client);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
