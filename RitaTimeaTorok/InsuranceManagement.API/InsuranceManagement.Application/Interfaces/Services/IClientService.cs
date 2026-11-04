using InsuranceManagement.Application.DTOs.Client;

namespace InsuranceManagement.Application.Interfaces.Services;

public interface IClientService
{
    Task<List<ClientDto>> GetAllAsync();
    Task<ClientDetailsDto?> GetAsync(Guid id);
    Task<ClientDto> CreateAsync(CreateClientDto request);
    Task<ClientDto?> UpdateAsync(Guid id, UpdateClientDto request);
    Task<bool> DeleteAsync(Guid id);
}
