using System.ComponentModel.DataAnnotations;
using AutoMapper;
using InsuranceManagement.Application.DTOs.Building;
using InsuranceManagement.Application.DTOs.Client;
using InsuranceManagement.Application.Exceptions;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Application.Interfaces.Services;
using InsuranceManagement.Domain.Entities;

namespace InsuranceManagement.Application.Services
{
    public class ClientService(
        IClientRepository clientRepository,
        IBuildingRepository buildingRepository,
        IMapper mapper) : IClientService
    {
        public async Task<List<ClientDto>> GetAllAsync(string? name = null, string? identifier = null) =>
            mapper.Map<List<ClientDto>>(await clientRepository.SearchAsync(name, identifier));

        public async Task<ClientDetailsDto?> GetAsync(Guid id)
        {
            var client = await clientRepository.GetAsync(id);
            if (client is null)
                return null;

            var clientDetails = mapper.Map<ClientDetailsDto>(client);
            var buildings = await buildingRepository.GetByClientIdAsync(id);
            clientDetails.Buildings = mapper.Map<List<BuildingDto>>(buildings);

            return clientDetails;
        }

        public async Task<ClientDto> CreateAsync(CreateClientDto request)
        {
            var client = mapper.Map<Client>(request);
            await ValidateClientAsync(client);

            client = await clientRepository.AddAsync(client);
            return mapper.Map<ClientDto>(client);
        }

        public async Task<ClientDto?> UpdateAsync(Guid id, UpdateClientDto request)
        {
            var client = await clientRepository.GetAsync(id);
            if (client is null)
                return null;

            mapper.Map(request, client);
            await ValidateClientAsync(client, id);

            await clientRepository.UpdateAsync(client);
            return mapper.Map<ClientDto>(client);
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var client = await clientRepository.GetAsync(id);
            if (client is null)
                return false;

            if (await buildingRepository.ExistsByClientIdAsync(id))
                throw new ConflictException(
                    "Cannot delete a client that owns buildings.");

            await clientRepository.DeleteAsync(client);
            return true;
        }

        private async Task ValidateClientAsync(
            Client client,
            Guid? excludeClientId = null)
        {
            if (string.IsNullOrWhiteSpace(client.Name))
                throw new ValidationException("Client name is required.");

            if (string.IsNullOrWhiteSpace(client.IdentificationNumber))
                throw new ValidationException("Identification number is required.");

            if (await clientRepository.ExistsByIdentificationNumberAsync(
                client.IdentificationNumber, excludeClientId))
                throw new ConflictException("A client with this identification number already exists.");
        }
    }
}
