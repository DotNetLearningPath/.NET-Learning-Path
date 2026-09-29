using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Client;

namespace InsuranceApp.Application.Abstractions.Services;

public interface IClientService
{
    Task<Result<PagedResult<ClientDto>>> SearchClientsAsync(ClientSearchDto clientSearchDto, CancellationToken cancellationToken);

    Task<Result<ClientDto>> GetClientByIdAsync(int clientId, CancellationToken cancellationToken);

    Task<Result<ClientDto>> CreateClientAsync(CreateClientDto createClientDto, CancellationToken cancellationToken);

    Task<Result<ClientDto>> UpdateClientAsync(int clientId, UpdateClientDto updateClientDto, CancellationToken cancellationToken);
}