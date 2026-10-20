using Insurance.Application.DTO.Common;
using Insurance.Domain.Entities;


namespace Insurance.Application.Abstractions;

public interface IClientRepository
{
    Task AddClientAsync(Client client, CancellationToken cancellationToken);
    Task<bool> ExistsClientByIdentificationNumberAsync(
        string identificationNumber,
        CancellationToken cancellationToken,
        Guid? excludedClientId = null);

    Task<Client?> GetClientByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<Client>> SearchClientAsync(
        string? name,
        string? identifier,
        PaginationRequest pagination,
        CancellationToken cancellationToken);

    Task UpdateClientAsync(Client client, CancellationToken cancellationToken);
}


