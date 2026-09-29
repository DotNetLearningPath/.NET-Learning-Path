using Application.DTO.Clients;
using Application.DTO.Common;

namespace Application.Abstractions;

public interface IClientService
{
    Task<ClientDto> CreateClientAsync(CreateClientRequest request, CancellationToken cancellationToken = default);

    Task<ClientDto?> GetClientByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedResult<ClientDto>> SearchClientAsync(
        string? name,
        string? identifier,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);

    Task<ClientDto> UpdateClientAsync(Guid id, UpdateClientRequest request, CancellationToken cancellationToken = default);
}
