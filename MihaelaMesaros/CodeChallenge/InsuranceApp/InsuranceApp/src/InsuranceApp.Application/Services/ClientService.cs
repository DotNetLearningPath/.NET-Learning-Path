using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Client;
using InsuranceApp.Application.Exceptions;
using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Net.Mail;

namespace InsuranceApp.Application.Services;

public sealed class ClientService(IClientRepository clientRepository, ILogger<ClientService> logger) : IClientService
{
    public async Task<Result<PagedResult<ClientDto>>> SearchClientsAsync(ClientSearchDto clientSearchDto, CancellationToken cancellationToken)
    {
        if (clientSearchDto.PageNumber is < CommonConstraints.MinPageNumber or > CommonConstraints.MaxPageNumber)
        {
            return Result<PagedResult<ClientDto>>.Failure(ClientErrors.InvalidPageNumber);
        }

        if (clientSearchDto.PageSize is < CommonConstraints.MinPageSize or > CommonConstraints.MaxPageSize)
        {
            return Result<PagedResult<ClientDto>>.Failure(ClientErrors.InvalidPageSize);
        }

        var (clients, totalCount) =
            await clientRepository.SearchClientAsync(
                clientSearchDto.Name?.Trim(),
                clientSearchDto.Identifier?.Trim(),
                clientSearchDto.PageNumber,
                clientSearchDto.PageSize,
                cancellationToken);

        var items = clients.Select(MapClientToDto).ToList();

        var pagedResult = new PagedResult<ClientDto>(items, clientSearchDto.PageNumber, clientSearchDto.PageSize, totalCount);

        return Result<PagedResult<ClientDto>>.Success(pagedResult);
    }

    public async Task<Result<ClientDto>> GetClientByIdAsync(Guid clientId, CancellationToken cancellationToken)
    {
        if (clientId == Guid.Empty)
        {
            return Result<ClientDto>.Failure(ClientErrors.InvalidClientId);
        }

        var client = await clientRepository.GetClientByIdAsync(clientId, cancellationToken);

        if (client is null)
        {
            return Result<ClientDto>.Failure(ClientErrors.NotFound(clientId));
        }

        return Result<ClientDto>.Success(MapClientToDto(client));
    }

    public async Task<Result<ClientDto>> CreateClientAsync(CreateClientDto createClientDto, CancellationToken cancellationToken)
    {
        createClientDto = createClientDto with
        {
            Name = createClientDto.Name?.Trim()!,
            IdentificationNumber = createClientDto.IdentificationNumber?.Trim()!,
            Email = createClientDto.Email?.Trim(),
            Phone = createClientDto.Phone?.Trim(),
            Address = createClientDto.Address?.Trim()
        };

        var validationClientName = ValidateClientName(createClientDto.Name);

        if (validationClientName is not null)
        {
            return Result<ClientDto>.Failure(validationClientName);
        }

        var validationClientContactInfo = ValidateClientContactInfo(createClientDto.Email, createClientDto.Phone, createClientDto.Address);

        if (validationClientContactInfo is not null)
        {
            return Result<ClientDto>.Failure(validationClientContactInfo);
        }

        var validationClientType = ValidateClientType(createClientDto.ClientType);

        if (validationClientType is not null)
        {
            return Result<ClientDto>.Failure(validationClientType);
        }

        var validationIdentificationNumber = ValidateIdentificationNumber(createClientDto.IdentificationNumber);

        if (validationIdentificationNumber is not null)
        {
            return Result<ClientDto>.Failure(validationIdentificationNumber);
        }

        var identificationNumberExists = await clientRepository.ClientIdentificationNumberExistsAsync(createClientDto.IdentificationNumber, cancellationToken);

        if (identificationNumberExists)
        {
            return Result<ClientDto>.Failure(ClientErrors.DuplicateIdentificationNumber);
        }


        var client = new Client
        {
            ClientType = createClientDto.ClientType,
            Name = createClientDto.Name,
            IdentificationNumber = createClientDto.IdentificationNumber,
            Email = createClientDto.Email,
            Phone = createClientDto.Phone,
            Address = createClientDto.Address,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            await clientRepository.AddClientAsync(client, cancellationToken);
        }
        catch (DuplicateEntityException)
        {
            return Result<ClientDto>.Failure(ClientErrors.DuplicateIdentificationNumber);
        }

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Client {ClientId} created.", client.ClientId);
        }

        return Result<ClientDto>.Success(MapClientToDto(client));
    }

    public async Task<Result<ClientDto>> UpdateClientAsync(Guid clientId, UpdateClientDto updateClientDto, CancellationToken cancellationToken)
    {
        if (clientId == Guid.Empty)
        {
            return Result<ClientDto>.Failure(ClientErrors.InvalidClientId);
        }

        updateClientDto = updateClientDto with
        {
            Name = updateClientDto.Name?.Trim()!,
            Email = updateClientDto.Email?.Trim(),
            Phone = updateClientDto.Phone?.Trim(),
            Address = updateClientDto.Address?.Trim()
        };

        var validationClientName = ValidateClientName(updateClientDto.Name);

        if (validationClientName is not null)
        {
            return Result<ClientDto>.Failure(validationClientName);
        }

        var validationClientContactInfo = ValidateClientContactInfo(updateClientDto.Email, updateClientDto.Phone, updateClientDto.Address);

        if (validationClientContactInfo is not null)
        {
            return Result<ClientDto>.Failure(validationClientContactInfo);
        }


        var client = await clientRepository.GetClientForUpdateAsync(clientId, cancellationToken);

        if (client is null)
        {
            return Result<ClientDto>.Failure(ClientErrors.NotFound(clientId));
        }

        client.Name = updateClientDto.Name;
        client.Email = updateClientDto.Email;
        client.Phone = updateClientDto.Phone;
        client.Address = updateClientDto.Address;
        client.ModifiedAt = DateTime.UtcNow;

        await clientRepository.SaveClientChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Client {ClientId} updated.", clientId);
        }

        return Result<ClientDto>.Success(MapClientToDto(client));
    }


    private static ClientDto MapClientToDto(Client client)
    {
        return new ClientDto(
            client.ClientId,
            client.ClientType,
            client.Name,
            client.IdentificationNumber,
            client.Email,
            client.Phone,
            client.Address);
    }

    private static Error? ValidateClientName(string? clientName)
    {
        if (string.IsNullOrWhiteSpace(clientName))
        {
            return ClientErrors.NameRequired;
        }

        if (clientName.Length is < ClientConstraints.NameMinLength or > ClientConstraints.NameMaxLength)
        {
            return ClientErrors.InvalidNameLength;
        }

        return null;
    }

    private static Error? ValidateClientContactInfo(string? clientEmail, string? clientPhone, string? clientAddress)
    {
        if (!string.IsNullOrWhiteSpace(clientEmail) 
            && (clientEmail.Length > ClientConstraints.EmailMaxLength || !MailAddress.TryCreate(clientEmail, out _)))
        {
            return ClientErrors.InvalidEmail;
        }

        if (!string.IsNullOrWhiteSpace(clientPhone) && clientPhone.Length > ClientConstraints.PhoneMaxLength)
        {
            return ClientErrors.InvalidPhoneLength;
        }

        if (!string.IsNullOrWhiteSpace(clientAddress) && clientAddress.Length > ClientConstraints.AddressMaxLength)
        {
            return ClientErrors.InvalidAddressLength;
        }

        return null;
    }

    private static Error? ValidateClientType(ClientType clientType)
    {
        return Enum.IsDefined(clientType) ? null : ClientErrors.InvalidClientType;
    }

    private static Error? ValidateIdentificationNumber(string? identificationNumber)
    {
        if (string.IsNullOrWhiteSpace(identificationNumber))
        {
            return ClientErrors.IdentificationNumberRequired;
        }

        if (identificationNumber.Length is < ClientConstraints.IdentificationNumberMinLength 
            or > ClientConstraints.IdentificationNumberMaxLength)
        {
            return ClientErrors.InvalidIdentificationNumberLength;
        }

        return null;
    }

}