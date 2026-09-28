using InsuranceApp.Application.Clients.Commands;
using InsuranceApp.Application.Clients.Queries;
using InsuranceApp.Application.Clients.Results;
using InsuranceApp.Application.Common.Pagination;

namespace InsuranceApp.Application.Clients;

public interface IClientService
{
    Task<ClientResult> GetClientByIdAsync(Guid clientId, CancellationToken cancellationToken);

    Task<ClientResult> CreateClientAsync(CreateClientCommand command, CancellationToken cancellationToken);

    Task<ClientResult> UpdateClientAsync(UpdateClientCommand command, CancellationToken cancellationToken);

    Task<PagedResult<ClientResult>> SearchClientsAsync(SearchClientsQuery query, CancellationToken cancellationToken);
}
