using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Application.Abstractions.Persistence;

public interface IClientRepository
{
    Task<(IReadOnlyList<Client> Items, int TotalCount)> SearchClientAsync(string? name, string? identificationNumber, int pageNumber, int pageSize, CancellationToken cancellationToken);

    Task<Client?> GetClientByIdAsync(int clientId, CancellationToken cancellationToken);

    Task<bool> ClientIdentificationNumberExistsAsync(string identificationNumber, CancellationToken cancellationToken);

    Task AddClientAsync(Client client, CancellationToken cancellationToken);

    Task SaveClientChangesAsync(CancellationToken cancellationToken);

    Task<Client?> GetClientForUpdateAsync(int clientId, CancellationToken cancellationToken);
}