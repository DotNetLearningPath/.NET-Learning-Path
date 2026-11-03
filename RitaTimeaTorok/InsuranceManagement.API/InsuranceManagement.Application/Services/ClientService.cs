using System.ComponentModel.DataAnnotations;
using AutoMapper;
using InsuranceManagement.Application.DTOs.Client;
using InsuranceManagement.Application.Interfaces;
using InsuranceManagement.Domain.Entities;

namespace InsuranceManagement.Application.Services;

public class ClientService(
    IGenericRepository<Client> clientRepository,
    IMapper mapper) : IClientService
{
    public async Task<List<ClientDto>> GetAllAsync() =>
        mapper.Map<List<ClientDto>>(await clientRepository.GetAllAsync());

    public async Task<ClientDto?> GetAsync(Guid id) =>
        mapper.Map<ClientDto?>(await clientRepository.GetAsync(id));

    public async Task<ClientDto> CreateAsync(CreateClientDto request)
    {
        var client = mapper.Map<Client>(request);
        ValidateClient(client);

        client = await clientRepository.AddAsync(client);
        return mapper.Map<ClientDto>(client);
    }

    public async Task<ClientDto?> UpdateAsync(Guid id, UpdateClientDto request)
    {
        var client = await clientRepository.GetAsync(id);
        if (client is null)
            return null;

        mapper.Map(request, client);
        ValidateClient(client);
        await clientRepository.UpdateAsync(client);
        return mapper.Map<ClientDto>(client);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var client = await clientRepository.GetAsync(id);
        if (client is null)
            return false;

        await clientRepository.DeleteAsync(client);
        return true;
    }

    private static void ValidateClient(Client client)
    {
        if (string.IsNullOrWhiteSpace(client.Name))
            throw new ValidationException("Client name is required.");

        if (string.IsNullOrWhiteSpace(client.IdentificationNumber))
            throw new ValidationException("Identification number is required.");
    }
}
