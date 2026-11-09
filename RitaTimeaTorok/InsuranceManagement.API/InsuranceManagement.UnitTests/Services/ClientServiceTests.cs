using AutoMapper;
using InsuranceManagement.Application.DTOs.Client;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Exceptions;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Application.Mapping;
using InsuranceManagement.Application.Services;
using InsuranceManagement.Domain.Entities;
using InsuranceManagement.Domain.Enums;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace InsuranceManagement.UnitTests.Services
{
    [TestClass]
    public class ClientServiceTests
    {
        private Mock<IClientRepository> _clientRepository = null!;
        private Mock<IBuildingRepository> _buildingRepository = null!;
        private IMapper _mapper = null!;
        private ClientService _clientService = null!;

        [TestInitialize]
        public void Setup()
        {
            _clientRepository = new Mock<IClientRepository>();
            _buildingRepository = new Mock<IBuildingRepository>();

            var mapperConfig = new MapperConfiguration(
                cfg =>
                {
                    cfg.AddProfile<ClientMappingProfile>();
                    cfg.AddProfile<BuildingMappingProfile>();
                },
                NullLoggerFactory.Instance);

            _mapper = mapperConfig.CreateMapper();
            _clientService = new ClientService(
                _clientRepository.Object,
                _buildingRepository.Object,
                _mapper,
                NullLogger<ClientService>.Instance);
        }

        [TestMethod]
        public async Task GetAllClients_WhenNoClients_ReturnsEmptyPage()
        {
            _clientRepository
                .Setup(r => r.SearchAsync(null, null, 1, 10))
                .ReturnsAsync(new PagedResult<Client> { Items = new List<Client>(), PageNumber = 1, PageSize = 10, TotalCount = 0 });

            var result = await _clientService.GetAllClientsAsync(1, 10);

            Assert.AreEqual(0, result.Items.Count);
        }

        [TestMethod]
        public async Task GetAllClients_WhenClientExists_ReturnsClient()
        {
            var client = new Client
            {
                Type = ClientType.Individual,
                Name = "John Doe",
                IdentificationNumber = "ID-123",
                Email = "john@example.com",
                PhoneNumber = "555-0100",
                PrimaryAddress = "1 Main Street"
            };
            _clientRepository
                .Setup(r => r.SearchAsync(null, null, 1, 10))
                .ReturnsAsync(new PagedResult<Client> { Items = new List<Client> { client }, PageNumber = 1, PageSize = 10, TotalCount = 1 });

            var result = await _clientService.GetAllClientsAsync(1, 10);

            Assert.AreEqual(1, result.Items.Count);
            AssertClientDtoMatches(client, result.Items[0]);
        }

        [TestMethod]
        public async Task GetClientByIdAsync_WhenClientDoesNotExist_ThrowsNotFound()
        {
            var id = Guid.NewGuid();
            _clientRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((Client?)null);

            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(
                () => _clientService.GetClientByIdAsync(id));
        }

        [TestMethod]
        public async Task GetClientByIdAsync_WhenClientExists_ReturnsClient()
        {
            var id = Guid.NewGuid();
            var client = new Client
            {
                Id = id,
                Type = ClientType.Individual,
                Name = "John Doe",
                IdentificationNumber = "ID-123",
                Email = "john@example.com",
                PhoneNumber = "555-0100",
                PrimaryAddress = "1 Main Street"
            };
            _clientRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync(client);
            _buildingRepository
                .Setup(r => r.GetByClientIdAsync(id))
                .ReturnsAsync(new List<Building>());

            var result = await _clientService.GetClientByIdAsync(id);

            Assert.IsNotNull(result);
            AssertClientDtoMatches(client, result);
            Assert.AreEqual(0, result.Buildings.Count);
        }

        [TestMethod]
        public async Task CreateClientAsync_WhenNameIsEmpty_ThrowsValidationException()
        {
            var request = new CreateClientDto
            {
                Type = ClientType.Individual,
                Name = "",
                IdentificationNumber = "ID-123"
            };

            await Assert.ThrowsExceptionAsync<System.ComponentModel.DataAnnotations.ValidationException>(
                () => _clientService.CreateClientAsync(request));

            _clientRepository.Verify(
                r => r.AddAsync(It.IsAny<Client>()),
                Times.Never);
        }

        [TestMethod]
        public async Task CreateClientAsync_WhenIdentificationNumberIsEmpty_ThrowsValidationException()
        {
            var request = new CreateClientDto
            {
                Type = ClientType.Individual,
                Name = "John Doe",
                IdentificationNumber = ""
            };

            await Assert.ThrowsExceptionAsync<System.ComponentModel.DataAnnotations.ValidationException>(
                () => _clientService.CreateClientAsync(request));

            _clientRepository.Verify(
                r => r.AddAsync(It.IsAny<Client>()),
                Times.Never);
        }

        [TestMethod]
        public async Task CreateClientAsync_WhenIdentificationNumberAlreadyExists_ThrowsConflictException()
        {
            var request = new CreateClientDto
            {
                Type = ClientType.Individual,
                Name = "John Doe",
                IdentificationNumber = "ID-123",
                Email = "john@example.com",
                PhoneNumber = "555-0100",
                PrimaryAddress = "1 Main Street"
            };
            _clientRepository
                .Setup(r => r.ExistsByIdentificationNumberAsync("ID-123", null))
                .ReturnsAsync(true);

            await Assert.ThrowsExceptionAsync<ConflictException>(
                () => _clientService.CreateClientAsync(request));

            _clientRepository.Verify(
                r => r.AddAsync(It.IsAny<Client>()),
                Times.Never);
        }

        [TestMethod]
        public async Task CreateClientAsync_WhenValid_CreatesClient()
        {
            var request = new CreateClientDto
            {
                Type = ClientType.Individual,
                Name = "John Doe",
                IdentificationNumber = "ID-123"
            };
            Client? createdClient = null;
            _clientRepository
                .Setup(r => r.AddAsync(It.IsAny<Client>()))
                .ReturnsAsync((Client client) => createdClient = client);

            var result = await _clientService.CreateClientAsync(request);

            Assert.IsNotNull(createdClient);
            AssertClientDtoMatches(createdClient!, result);
            _clientRepository.Verify(
                r => r.AddAsync(It.Is<Client>(c => c.Name == "John Doe" && c.IdentificationNumber == "ID-123")),
                Times.Once);
        }

        [TestMethod]
        public async Task UpdateClientAsync_WhenClientDoesNotExist_ThrowsEntityNotFoundException()
        {
            var id = Guid.NewGuid();
            var request = new UpdateClientDto
            {
                Type = ClientType.Individual,
                Name = "John Doe"
            };
            _clientRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((Client?)null);

            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _clientService.UpdateClientAsync(id, request));
            _clientRepository.Verify(
                r => r.UpdateAsync(It.IsAny<Client>()),
                Times.Never);
        }

        [TestMethod]
        public async Task UpdateClientAsync_WhenValid_UpdatesClient()
        {
            var client = new Client
            {
                Type = ClientType.Individual,
                Name = "Old Name",
                IdentificationNumber = "ID-123"
            };
            var request = new UpdateClientDto
            {
                Type = ClientType.Company,
                Name = "Updated Name",
                Email = "updated@example.com",
                PhoneNumber = "555-0101",
                PrimaryAddress = "2 Main Street"
            };
            _clientRepository
                .Setup(r => r.GetAsync(client.Id))
                .ReturnsAsync(client);

            var result = await _clientService.UpdateClientAsync(client.Id, request);

            Assert.IsNotNull(result);
            AssertClientDtoMatches(client, result);
            _clientRepository.Verify(
                r => r.UpdateAsync(client),
                Times.Once);
        }

        [TestMethod]
        public async Task DeleteClientAsync_WhenClientDoesNotExist_ThrowsEntityNotFoundException()
        {
            var id = Guid.NewGuid();
            _clientRepository
                .Setup(r => r.GetAsync(id))
                .ReturnsAsync((Client?)null);

            await Assert.ThrowsExceptionAsync<EntityNotFoundException>(() => _clientService.DeleteClientAsync(id));
            _clientRepository.Verify(
                r => r.DeleteAsync(It.IsAny<Client>()),
                Times.Never);
        }

        [TestMethod]
        public async Task DeleteClientAsync_WhenClientHasBuildings_ThrowsConflictException()
        {
            var client = new Client
            {
                Type = ClientType.Individual,
                Name = "John Doe",
                IdentificationNumber = "ID-123"
            };
            _clientRepository
                .Setup(r => r.GetAsync(client.Id))
                .ReturnsAsync(client);
            _buildingRepository
                .Setup(r => r.ExistsByClientIdAsync(client.Id))
                .ReturnsAsync(true);

            await Assert.ThrowsExceptionAsync<ConflictException>(
                () => _clientService.DeleteClientAsync(client.Id));

            _clientRepository.Verify(
                r => r.DeleteAsync(It.IsAny<Client>()),
                Times.Never);
        }

        [TestMethod]
        public async Task DeleteClientAsync_WhenClientExists_CallsRepositoryDelete()
        {
            var client = new Client
            {
                Type = ClientType.Individual,
                Name = "John Doe",
                IdentificationNumber = "ID-123"
            };
            _clientRepository
                .Setup(r => r.GetAsync(client.Id))
                .ReturnsAsync(client);
            _buildingRepository
                .Setup(r => r.ExistsByClientIdAsync(client.Id))
                .ReturnsAsync(false);

            await _clientService.DeleteClientAsync(client.Id);
            _clientRepository.Verify(
                r => r.DeleteAsync(client),
                Times.Once);
        }

        private static void AssertClientDtoMatches(
            Client expectedClient, 
            ClientDto actualClient)
        {
            Assert.AreEqual(expectedClient.Id, actualClient.Id);
            Assert.AreEqual(expectedClient.Type, actualClient.Type);
            Assert.AreEqual(expectedClient.Name, actualClient.Name);
            Assert.AreEqual(expectedClient.IdentificationNumber, actualClient.IdentificationNumber);
            Assert.AreEqual(expectedClient.Email, actualClient.Email);
            Assert.AreEqual(expectedClient.PhoneNumber, actualClient.PhoneNumber);
            Assert.AreEqual(expectedClient.PrimaryAddress, actualClient.PrimaryAddress);
        }
    }
}
