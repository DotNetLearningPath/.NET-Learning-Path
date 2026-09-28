using Application.Abstractions;
using Application.DTO.Common;
using Domain.Entities;

namespace Application.Fakes;

public sealed class FakeClientRepository : IClientRepository
{
    public readonly Dictionary<Guid, Client> Storage = new();

    public void Seed(Client client) => Storage[client.Id] = client;

    public Task AddClientAsync(Client client, CancellationToken cancellationToken = default)
    {
        Storage[client.Id] = client;
        return Task.CompletedTask;
    }

    public Task<bool> ExistsClientByIdentificationNumberAsync(
        string identificationNumber,
        Guid? excludedClientId = null,
        CancellationToken cancellationToken = default)
    {
        var exists = Storage.Values.Any(client =>
            client.IdentificationNumber == identificationNumber
            && client.Id != excludedClientId);

        return Task.FromResult(exists);
    }

    public Task<Client?> GetClientByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Storage.TryGetValue(id, out var client);
        return Task.FromResult(client);
    }

    public Task<PagedResult<Client>> SearchClientAsync(
        string? name,
        string? identifier,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var query = Storage.Values.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(client =>
                client.Name.Contains(name.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(identifier))
        {
            query = query.Where(client =>
                client.IdentificationNumber == identifier.Trim());
        }

        var all = query
            .OrderBy(client => client.Name)
            .ThenBy(client => client.Id)
            .ToList();

        var pageNumber = Math.Max(pagination.PageNumber, 1);
        var pageSize = Math.Min(Math.Max(pagination.PageSize, 1), 100);

        return Task.FromResult(new PagedResult<Client>
        {
            Items = all.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = all.Count
        });
    }

    public Task UpdateClientAsync(Client client, CancellationToken cancellationToken = default)
    {
        Storage[client.Id] = client;
        return Task.CompletedTask;
    }
}
