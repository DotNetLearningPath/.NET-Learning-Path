using Insurance.Application.DTO.Clients;
using Insurance.Application.DTO.Common;

namespace Insurance.Application.Abstractions;

public interface IClientService
{
    Task<ClientDto> CreateClientAsync(CreateClientRequest request, CancellationToken cancellationToken);

    Task<ClientDto?> GetClientByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedResult<ClientDto>> SearchClientAsync(
        string? name,
        string? identifier,
        PaginationRequest pagination,
        CancellationToken cancellationToken);

    Task<ClientDto> UpdateClientAsync(Guid id, UpdateClientRequest request, CancellationToken cancellationToken);
}
