using Insurance.Application.Interfaces;
using Insurance.Application.Exceptions;
using Insurance.Domain.Entities;

namespace Insurance.Application.Services
{
    public class ClientService : IClientService
    {
        private const int MaximumPageSize = 100;

        private readonly IClientRepository _clientRepository;
        private readonly IClientValidator _clientValidator;

        public ClientService(IClientRepository clientRepository, IClientValidator clientValidator)
        {
            _clientRepository = clientRepository;
            _clientValidator = clientValidator;
        }

        public async Task<Client> CreateAsync(Client client, CancellationToken cancellationToken)
        {
            _clientValidator.Validate(client);

            bool alreadyExists = await _clientRepository.ExistsClientByIdentificationNumberAsync(client.IdentificationNumber, cancellationToken);

            if (alreadyExists)
            {
                throw new DuplicateClientException(client.IdentificationNumber);
            }

            client.Id = Guid.NewGuid();

            await _clientRepository.AddAsync(client, cancellationToken);

            await _clientRepository.SaveChangesAsync(cancellationToken);

            return client;
        }

        public async Task<Client?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Client ID cannot be empty.", nameof(id));
            }

            return await _clientRepository.GetClientByIdAsync(id, cancellationToken);
        }

        public async Task<Client?> GetByIdWithBuildingsAsync(Guid id, CancellationToken cancellationToken)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Client ID cannot be empty.", nameof(id));
            }

            return await _clientRepository.GetClientByIdWithBuildingsAsync(id, cancellationToken);
        }

        public async Task<IReadOnlyList<Client>> SearchAsync(string? name, string? identificationNumber, int page, int pageSize, CancellationToken cancellationToken)
        {
            if (page < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(page), "Page must be greater than zero.");
            }

            if (pageSize < 1 || pageSize > MaximumPageSize)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize), $"Page size must be between 1 and {MaximumPageSize}.");
            }

            return await _clientRepository.SearchClientsAsync(name, identificationNumber, page, pageSize, cancellationToken);
        }

        public async Task<Client?> UpdateAsync(Client client, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(client);

            var existingClient = await _clientRepository.GetClientByIdAsync(client.Id, cancellationToken);

            if (existingClient == null)
            {
                return null;
            }

            var clientToValidate = new Client
            {
                Id = existingClient.Id,
                ClientType = existingClient.ClientType,
                IdentificationNumber = existingClient.IdentificationNumber,

                Name = client.Name,
                Email = client.Email,
                Phone = client.Phone,
                PrimaryAddress = client.PrimaryAddress
            };

            _clientValidator.Validate(clientToValidate);


            existingClient.Name = client.Name;
            existingClient.Email = client.Email;
            existingClient.Phone = client.Phone;
            existingClient.PrimaryAddress = client.PrimaryAddress;


            await _clientRepository.SaveChangesAsync(cancellationToken);

            return existingClient;
        }
    }
}
