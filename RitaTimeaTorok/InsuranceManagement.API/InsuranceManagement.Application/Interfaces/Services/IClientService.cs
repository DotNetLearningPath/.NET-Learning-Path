using InsuranceManagement.Application.DTOs.Client;

using InsuranceManagement.Application.DTOs.Pagination;

namespace InsuranceManagement.Application.Interfaces.Services
{
    public interface IClientService
    {
        Task<PagedResult<ClientDto>> GetAllClientsAsync(
            int pageNumber,
            int pageSize,
            string? name = null,
            string? identifier = null);
        Task<ClientDetailsDto> GetClientByIdAsync(Guid id);
        Task<ClientDto> CreateClientAsync(CreateClientDto request);
        Task<ClientDto> UpdateClientAsync(Guid id, UpdateClientDto request);
        Task DeleteClientAsync(Guid id);
    }
}
