using Insurance.Application.DTO.Clients;
using Insurance.Application.DTO.Common;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Insurance.Application.Exceptions;
using Insurance.Application.Services;
using Insurance.UnitTest.Application.Helper;

namespace Insurance.UnitTest.Application.Services
{
    public class ClientServiceTest
    {
        private readonly FakeRepositories _fakeRepositories;

        public ClientServiceTest()
        {
            _fakeRepositories = new FakeRepositories();
        }
        [Fact]
        public async Task CreateClientAsync_Should_Create_Individual_With_Valid_CNP()
        {
            // Arrange
            var service = new ClientService(_fakeRepositories.Client);

            var request = new CreateClientRequest
            {
                ClientType = ClientType.Individual,
                Name = "John",
                IdentificationNumber = "1234567890123",
                Email = "a@b.com",
                Phone = "123"
            };

            // Act
            var dto = await service.CreateClientAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(request.Name, dto.Name);
            Assert.Equal(request.IdentificationNumber, dto.IdentificationNumber);
            Assert.Single(_fakeRepositories.Client.Storage);
        }

        [Fact]
        public async Task CreateClientAsync_Should_Create_Company_With_RO_Prefix()
        {
            // Arrange
            var service = new ClientService(_fakeRepositories.Client);

            var request = new CreateClientRequest
            {
                ClientType = ClientType.Company,
                Name = "ACME",
                IdentificationNumber = "RO12345",
                Email = "info@acme.com"
            };

            // Act
            var dto = await service.CreateClientAsync(request, CancellationToken.None);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(request.Name, dto.Name);
            Assert.Equal(request.IdentificationNumber, dto.IdentificationNumber);
        }

        [Fact]
        public async Task CreateClientAsync_Should_Throw_When_Identification_Invalid_For_Individual()
        {
            // Arrange
            var service = new ClientService(_fakeRepositories.Client);

            var request = new CreateClientRequest
            {
                ClientType = ClientType.Individual,
                Name = "John",
                IdentificationNumber = "ABC"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateClientAsync(request, CancellationToken.None));
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_Duplicate_Identification()
        {
            // Arrange
            var existing = new Client(ClientType.Individual, "Existing", "1234567890123");
            await _fakeRepositories.Client.AddClientAsync(existing, CancellationToken.None);

            var service = new ClientService(_fakeRepositories.Client);

            var request = new CreateClientRequest
            {
                ClientType = ClientType.Individual,
                Name = "New",
                IdentificationNumber = "1234567890123"
            };

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateClientAsync(request, CancellationToken.None));

            // Assert
            Assert.Equal("A client with this identification number already exists.", ex.Message);
        }

        [Fact]
        public async Task GetClientByIdAsync_Should_Return_Dto_When_Found()
        {
            // Arrange
            var client = new Client(ClientType.Individual, "John", "1234567890123");
            await _fakeRepositories.Client.AddClientAsync(client, CancellationToken.None);

            var service = new ClientService(_fakeRepositories.Client);

            // Act
            var dto = await service.GetClientByIdAsync(client.Id, CancellationToken.None);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(client.Id, dto!.Id);
            Assert.Equal(client.Name, dto.Name);
        }

        [Fact]
        public async Task GetClientByIdAsync_Should_Return_Null_When_Not_Found()
        {
            // Arrange
            var service = new ClientService(_fakeRepositories.Client);

            // Act
            var dto = await service.GetClientByIdAsync(Guid.NewGuid(), CancellationToken.None);

            // Assert
            Assert.Null(dto);
        }

        [Fact]
        public async Task SearchClientAsync_Should_Return_All_Clients_When_No_Filters_Are_Provided()
        {
            // Arrange
            var c1 = new Client(ClientType.Individual, "Alice", "1111111111111");
            var c2 = new Client(ClientType.Company, "Acme", "RO22222");
            await _fakeRepositories.Client.AddClientAsync(c1, CancellationToken.None);
            await _fakeRepositories.Client.AddClientAsync(c2, CancellationToken.None);

            var service = new ClientService(_fakeRepositories.Client);

            // Act
            var all = await service.SearchClientAsync(null, null, new PaginationRequest { PageSize = 10 }, CancellationToken.None);

            // Assert
            Assert.Equal(2, all.TotalCount);
            Assert.Equal(2, all.Items.Count);
        }

        [Fact]
        public async Task SearchClientAsync_Should_Return_Matching_Clients_By_Name()
        {
            // Arrange
            var c1 = new Client(ClientType.Individual, "Alice", "1111111111111");
            var c2 = new Client(ClientType.Company, "Acme", "RO22222");
            await _fakeRepositories.Client.AddClientAsync(c1, CancellationToken.None);
            await _fakeRepositories.Client.AddClientAsync(c2, CancellationToken.None);

            var service = new ClientService(_fakeRepositories.Client);

            // Act
            var filtered = await service.SearchClientAsync("Acme", null, new PaginationRequest(), CancellationToken.None);

            // Assert
            Assert.Equal(1, filtered.TotalCount);
            var result = Assert.Single(filtered.Items);
            Assert.Equal(c2.Id, result.Id);
        }

        [Fact]
        public async Task UpdateClientAsync_Should_Update_When_Valid()
        {
            // Arrange
            var client = new Client(ClientType.Individual, "John", "1234567890123");
            await _fakeRepositories.Client.AddClientAsync(client, CancellationToken.None);

            var service = new ClientService(_fakeRepositories.Client);

            var update = new UpdateClientRequest
            {
                ClientType = ClientType.Company,
                Name = "John Updated",
                IdentificationNumber = "1234567890123",
                Email = "new@a.com",
                Phone = "999",
                Address = "Addr"
            };

            // Act
            var dto = await service.UpdateClientAsync(client.Id, update, CancellationToken.None);

            // Assert
            Assert.Equal(client.Id, dto.Id);
            Assert.Equal(update.Name, dto.Name);
            Assert.Equal(update.Email, dto.Email);
            Assert.Equal(ClientType.Company, dto.ClientType);
        }

        [Fact]
        public async Task UpdateClientAsync_Should_Throw_When_Client_Not_Found()
        {
            // Arrange
            var service = new ClientService(_fakeRepositories.Client);

            var update = new UpdateClientRequest
            {
                ClientType = ClientType.Individual,
                Name = "X",
                IdentificationNumber = "1234567890123"
            };

            // Act
            var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateClientAsync(Guid.NewGuid(), update, CancellationToken.None));

            // Assert
            Assert.Equal("Client was not found.", ex.Message);
        }

        [Fact]
        public async Task UpdateClientAsync_Should_Throw_When_Identification_Changed()
        {
            // Arrange
            var client = new Client(ClientType.Individual, "John", "1234567890123");
            await _fakeRepositories.Client.AddClientAsync(client, CancellationToken.None);

            var service = new ClientService(_fakeRepositories.Client);

            var update = new UpdateClientRequest
            {
                ClientType = ClientType.Individual,
                Name = "John",
                IdentificationNumber = "9999999999999"
            };

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateClientAsync(client.Id, update, CancellationToken.None));

            // Assert
            Assert.Equal("The client identification number cannot be changed.", ex.Message);
        }

        [Fact]
        public async Task UpdateClientAsync_Should_Throw_When_Identification_Exists_For_Other()
        {
            // Arrange
            var client1 = new Client(ClientType.Individual, "A", "1234567890123");
            var client2 = new Client(ClientType.Individual, "B", "9999999999999");
            await _fakeRepositories.Client.AddClientAsync(client1, CancellationToken.None);
            await _fakeRepositories.Client.AddClientAsync(client2, CancellationToken.None);

            var service = new ClientService(_fakeRepositories.Client);

            var update = new UpdateClientRequest
            {
                ClientType = ClientType.Individual,
                Name = "A",
                IdentificationNumber = client2.IdentificationNumber // attempt to set to other client's id
            };

            // Act
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateClientAsync(client1.Id, update, CancellationToken.None));

            // Assert
            Assert.Equal("The client identification number cannot be changed.", ex.Message);
        }
    }
}
