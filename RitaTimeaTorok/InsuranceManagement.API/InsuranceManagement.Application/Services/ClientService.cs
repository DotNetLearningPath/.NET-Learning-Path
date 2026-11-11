using System.ComponentModel.DataAnnotations;
using AutoMapper;
using InsuranceManagement.Application.DTOs.Building;
using InsuranceManagement.Application.DTOs.Client;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Exceptions;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Application.Interfaces.Services;
using InsuranceManagement.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InsuranceManagement.Application.Services
{
    public class ClientService(
        IClientRepository clientRepository,
        IBuildingRepository buildingRepository,
        IMapper mapper,
        ILogger<ClientService> logger) : IClientService
    {
        public async Task<PagedResult<ClientDto>> GetAllClientsAsync(
            int pageNumber,
            int pageSize,
            string? name = null,
            string? identifier = null)
        {
            PaginationValidator.Validate(pageNumber, pageSize);
            var page = await clientRepository.SearchAsync(name, identifier, pageNumber, pageSize);
            logger.LogInformation(
                "Retrieved clients page {PageNumber} with page size {PageSize}; returned {ResultCount} of {TotalCount} clients",
                page.PageNumber, page.PageSize, page.Items.Count, page.TotalCount);

            return new PagedResult<ClientDto>
            {
                Items = mapper.Map<List<ClientDto>>(page.Items),
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount
            };
        }

        public async Task<ClientDetailsDto> GetClientByIdAsync(Guid id)
        {
            var client = await clientRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"Client '{id}' was not found.");

            var clientDetails = mapper.Map<ClientDetailsDto>(client);
            var buildings = await buildingRepository.GetByClientIdAsync(id);
            clientDetails.Buildings = mapper.Map<List<BuildingDto>>(buildings);
            logger.LogInformation(
                "Retrieved client {ClientId} with {BuildingCount} associated buildings",
                id, buildings.Count);

            return clientDetails;
        }

        public async Task<ClientDto> CreateClientAsync(CreateClientDto request)
        {
            var client = mapper.Map<Client>(request);
            await ValidateClientAsync(client);

            client = await clientRepository.AddAsync(client);

            logger.LogInformation("Created client {ClientId}", client.Id);
            return mapper.Map<ClientDto>(client);
        }

        public async Task<ClientDto> UpdateClientAsync(Guid id, UpdateClientDto request)
        {
            var client = await clientRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"Client '{id}' was not found.");

            mapper.Map(request, client);
            await ValidateClientAsync(client, id);

            await clientRepository.UpdateAsync(client);
            logger.LogInformation("Updated client {ClientId}", client.Id);
            return mapper.Map<ClientDto>(client);
        }

        public async Task DeleteClientAsync(Guid id)
        {
            var client = await clientRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"Client '{id}' was not found.");

            if (await buildingRepository.ExistsByClientIdAsync(id))
                throw new ConflictException(
                    "Cannot delete a client that owns buildings.");

            await clientRepository.DeleteAsync(client);
            logger.LogInformation("Deleted client {ClientId}", id);
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
