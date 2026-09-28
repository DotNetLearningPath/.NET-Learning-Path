using InsuranceApp.Application.Clients.Queries;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Domain.Clients;

namespace InsuranceApp.Application.Clients;

public interface IClientRepository
{
    Task<Client?> GetClientByIdAsync(Guid clientId, CancellationToken cancellationToken);

    Task<bool> ClientExistsByIdentificationNumberAsync(string identificationNumber, CancellationToken cancellationToken);

    Task<bool> ClientExistsByIdAsync(Guid clientId, CancellationToken cancellationToken);

    Task AddClientAsync(Client client, CancellationToken cancellationToken);

    Task UpdateClientAsync(Client client, CancellationToken cancellationToken);

    Task<PagedResult<Client>> SearchClientsAsync(SearchClientsQuery query, CancellationToken cancellationToken);
}
