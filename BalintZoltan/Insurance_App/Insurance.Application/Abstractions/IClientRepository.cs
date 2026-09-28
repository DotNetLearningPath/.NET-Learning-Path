using Application.DTO.Clients;
using Application.DTO.Common;
using Domain.Entities;


namespace Application.Abstractions;

public interface IClientRepository
{
    Task AddClientAsync(Client client, CancellationToken cancellationToken = default);
    Task<bool> ExistsClientByIdentificationNumberAsync(
        string identificationNumber,
        Guid? excludedClientId = null,
        CancellationToken cancellationToken = default);

    Task<Client?> GetClientByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<Client>> SearchClientAsync(
        string? name,
        string? identifier,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);

    Task UpdateClientAsync(Client client, CancellationToken cancellationToken = default);
}


