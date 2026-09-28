using FluentValidation;
using InsuranceApp.Application.Clients.Commands;
using InsuranceApp.Application.Clients.Mappings;
using InsuranceApp.Application.Clients.Queries;
using InsuranceApp.Application.Clients.Results;
using InsuranceApp.Application.Common.Exceptions;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Common.Validation;
using InsuranceApp.Domain.Clients;
using Microsoft.Extensions.Logging;

namespace InsuranceApp.Application.Clients;

public class ClientService : IClientService
{
    private readonly IClientRepository _clientRepository;
    private readonly IValidator<CreateClientCommand> _createValidator;
    private readonly IValidator<UpdateClientCommand> _updateValidator;
    private readonly IValidator<SearchClientsQuery> _searchValidator;
    private readonly ILogger<ClientService> _logger;

    public ClientService(
        IClientRepository clientRepository,
        IValidator<CreateClientCommand> createValidator,
        IValidator<UpdateClientCommand> updateValidator,
        IValidator<SearchClientsQuery> searchValidator,
        ILogger<ClientService> logger
    )
    {
        ArgumentNullException.ThrowIfNull(clientRepository);
        ArgumentNullException.ThrowIfNull(createValidator);
        ArgumentNullException.ThrowIfNull(updateValidator);
        ArgumentNullException.ThrowIfNull(searchValidator);
        ArgumentNullException.ThrowIfNull(logger);

        _clientRepository = clientRepository;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _searchValidator = searchValidator;
        _logger = logger;
    }

    public async Task<ClientResult> GetClientByIdAsync(Guid clientId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (clientId == Guid.Empty)
        {
            throw ValidationExceptionFactory.Create("ClientId", "Client identifier must not be empty.");
        }

        var client = await GetClientByIdOrThrowAsync(clientId, cancellationToken);

        return client.ToResult();
    }

    public async Task<ClientResult> CreateClientAsync(CreateClientCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        cancellationToken.ThrowIfCancellationRequested();
        await _createValidator.ValidateAndThrowAsync(command, cancellationToken);

        var client = new Client(
            id: Guid.NewGuid(),
            type: command.Type,
            identificationNumber: command.IdentificationNumber!,
            name: command.Name!,
            email: command.Email!,
            phone: command.Phone!,
            primaryAddress: command.PrimaryAddress
        );

        await _clientRepository.AddClientAsync(client, cancellationToken);
        _logger.LogInformation("Client {ClientId} created.", client.Id);

        return client.ToResult();
    }

    public async Task<ClientResult> UpdateClientAsync(UpdateClientCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);
        cancellationToken.ThrowIfCancellationRequested();
        await _updateValidator.ValidateAndThrowAsync(command, cancellationToken);

        var client = await GetClientByIdOrThrowAsync(command.ClientId, cancellationToken);

        client.UpdateDetails(
            name: command.Name!,
            email: command.Email!,
            phone: command.Phone!,
            primaryAddress: command.PrimaryAddress
        );

        await _clientRepository.UpdateClientAsync(client, cancellationToken);
        _logger.LogInformation("Client {ClientId} updated.", client.Id);

        return client.ToResult();
    }

    public async Task<PagedResult<ClientResult>> SearchClientsAsync(SearchClientsQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        cancellationToken.ThrowIfCancellationRequested();
        await _searchValidator.ValidateAndThrowAsync(query, cancellationToken);

        var normalizedQuery = query with
        {
            Name = NormalizeOptionalFilter(query.Name),
            Identifier = NormalizeOptionalFilter(query.Identifier)
        };

        var page = await _clientRepository.SearchClientsAsync(normalizedQuery, cancellationToken);

        return page.Map(client => client.ToResult());
    }

    private async Task<Client> GetClientByIdOrThrowAsync(Guid clientId, CancellationToken cancellationToken)
    {
        var client = await _clientRepository.GetClientByIdAsync(clientId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Client), clientId);

        return client;
    }

    private static string? NormalizeOptionalFilter(string? filter)
    {
        return string.IsNullOrWhiteSpace(filter) ? null : filter.Trim();
    }
}
