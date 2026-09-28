using Application.Abstractions;
using Application.DTO.Clients;
using Domain.Entities;
using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Application.DTO.Common;

namespace Application.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;
    public ClientService(IClientRepository clientRepository)
    {
        _clientRepository = clientRepository;
    }
    private async Task CheckClientIdentificationNumberExistAsync(string identificationNumber, CancellationToken cancellationToken)
    {
        var exists = await _clientRepository
            .ExistsClientByIdentificationNumberAsync(identificationNumber, cancellationToken: cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                "A client with this identification number already exists.");
        }


    }
    public async Task<ClientDto> CreateClientAsync(CreateClientRequest request, CancellationToken cancellationToken = default)
    {
        ValidateIdentificationNumber(request.ClientType, request.IdentificationNumber);
        ValidateEmail(request.Email);
        await CheckClientIdentificationNumberExistAsync(request.IdentificationNumber, cancellationToken);

        var client = new Client(
            request.ClientType,
            request.Name,
            request.IdentificationNumber,
            request.Email,
            request.Phone,
            request.Address);

        await _clientRepository.AddClientAsync(client, cancellationToken);

        return new ClientDto
        {
            Id = client.Id,
            ClientType = client.Type,
            Name = client.Name,
            IdentificationNumber = client.IdentificationNumber,
            Email = client.Email,
            Phone = client.Phone,
            Address = client.Address
        };
    }
    public async Task<ClientDto?> GetClientByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var client = await _clientRepository.GetClientByIdAsync(id, cancellationToken);

        if (client is null)
        {
            return null;
        }

        return MapToClientDto(client);
    }
    public async Task<PagedResult<ClientDto>> SearchClientAsync(
        string? name,
        string? identifier,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        var result = await _clientRepository.SearchClientAsync(
            name,
            identifier,
            pagination,
            cancellationToken);

        return new PagedResult<ClientDto>
        {
            Items = result.Items
                .Select(MapToClientDto)
                .ToList(),

            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<ClientDto> UpdateClientAsync(
           Guid id,
           UpdateClientRequest request,
           CancellationToken cancellationToken = default)
    {
        var client = await _clientRepository.GetClientByIdAsync(id, cancellationToken);

        if (client is null)
        {
            throw new InvalidOperationException("Client was not found.");
        }

        if (client.IdentificationNumber != request.IdentificationNumber)
        {
            throw new InvalidOperationException(
                "The client identification number cannot be changed.");
        }

        if (request.Email != null)
        {
            ValidateEmail(request.Email);
        }

        client.ChangeType(request.ClientType);
        client.ChangeName(request.Name);
        client.UpdateContactDetails(
            request.Email,
            request.Phone,
            request.Address);

        await _clientRepository.UpdateClientAsync(client, cancellationToken);

        return MapToClientDto(client);
    }
    private static void ValidateIdentificationNumber(
            ClientType clientType,
            string identificationNumber)
    {
        if (string.IsNullOrWhiteSpace(identificationNumber))
        {
            throw new ArgumentException("Identification number is required.");
        }

        if (clientType == ClientType.Individual &&
            !Regex.IsMatch(identificationNumber, @"^\d{13}$"))
        {
            throw new ArgumentException("CNP must contain exactly 13 digits.");
        }

        if (clientType == ClientType.Company &&
            !Regex.IsMatch(identificationNumber, @"^(RO)?\d{2,10}$",
                RegexOptions.IgnoreCase))
        {
            throw new ArgumentException(
                "CUI must contain 2-10 digits, optionally prefixed with RO.");
        }
    }

    private static void ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        var emailTrim = email.Trim();

        if (emailTrim.Length > 254)
        {
            throw new ArgumentException(
                "Email cannot be longer than 254 characters.");
        }

        var emailAttribute = new EmailAddressAttribute();

        if (!emailAttribute.IsValid(emailTrim))
        {
            throw new ArgumentException(
                "Invalid email address format.");
        }
    }

    private static ClientDto MapToClientDto(Client client)
    {
        return new ClientDto
        {
            Id = client.Id,
            ClientType = client.Type,
            Name = client.Name,
            IdentificationNumber = client.IdentificationNumber,
            Email = client.Email,
            Phone = client.Phone,
            Address = client.Address
        };
    }
}
