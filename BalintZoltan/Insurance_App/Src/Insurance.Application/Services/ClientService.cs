using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Clients;
using Insurance.Application.DTO.Common;
using Insurance.Application.Exceptions;
using Insurance.Application.Validation;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using System.Text.RegularExpressions;

namespace Insurance.Application.Services;

public class ClientService(
    IClientRepository clientRepository
    ) : IClientService
{
    private const int IndividualIdentificationNumberLength = 13;
    private const int CompanyIdentificationNumberType1Length = 2;
    private const int CompanyIdentificationNumberType2Length = 10;
    private async Task CheckClientIdentificationNumberExistAsync(string identificationNumber, CancellationToken cancellationToken)
    {
        var exists = await clientRepository
            .ExistsClientByIdentificationNumberAsync(identificationNumber, cancellationToken: cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException(
                "A client with this identification number already exists.");
        }
    }
    public async Task<ClientDto> CreateClientAsync(CreateClientRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidateIdentificationNumber(request.ClientType, request.IdentificationNumber);
        ValidateOptionalEmail(request.Email);
        await CheckClientIdentificationNumberExistAsync(request.IdentificationNumber, cancellationToken);

        var client = new Client(
            request.ClientType,
            request.Name,
            request.IdentificationNumber,
            request.Email,
            request.Phone,
            request.Address);

        await clientRepository.AddClientAsync(client, cancellationToken);

        return MapToClientDto(client);
    }
    public async Task<ClientDto?> GetClientByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetClientByIdAsync(id, cancellationToken);

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
        CancellationToken cancellationToken)
    {
        var result = await clientRepository.SearchClientAsync(
            name,
            identifier,
            pagination,
            cancellationToken);

        return new PagedResult<ClientDto>
        {
            Items = [.. result.Items.Select(MapToClientDto)],

            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<ClientDto> UpdateClientAsync(
           Guid id,
           UpdateClientRequest request,
           CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var client = await clientRepository.GetClientByIdAsync(id, cancellationToken)
                     ??
                     throw new NotFoundException("Client was not found.");
        if (client.IdentificationNumber != request.IdentificationNumber)
        {
            throw new InvalidOperationException(
                "The client identification number cannot be changed.");
        }

        if (request.Email != null)
        {
            ValidateOptionalEmail(request.Email);
        }

        client.ChangeType(request.ClientType);
        client.ChangeName(request.Name);
        client.UpdateContactDetails(
            request.Email,
            request.Phone,
            request.Address);

        await clientRepository.UpdateClientAsync(client, cancellationToken);

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
            !Regex.IsMatch(
                identificationNumber,
                $@"^\d{{{IndividualIdentificationNumberLength}}}$"))
        {
            throw new ArgumentException(
                $"CNP must contain exactly {IndividualIdentificationNumberLength} digits.");
        }

        if (clientType == ClientType.Company &&
            !Regex.IsMatch(
                identificationNumber,
                $@"^(RO)?\d{{{CompanyIdentificationNumberType1Length},{CompanyIdentificationNumberType2Length}}}$",
                RegexOptions.IgnoreCase))
        {
            throw new ArgumentException(
                $"CUI must contain {CompanyIdentificationNumberType1Length}-" +
                $"{CompanyIdentificationNumberType2Length} digits, optionally prefixed with RO.");
        }
    }

    private static void ValidateOptionalEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        EmailValidator.ValidateFormatAndLength(email);
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
