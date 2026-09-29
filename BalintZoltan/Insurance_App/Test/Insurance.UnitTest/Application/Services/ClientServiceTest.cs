using Application.DTO.Clients;
using Application.DTO.Common;
using Application.Exceptions;
using Application.Helper;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
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
            var service = new ClientService(_fakeRepositories.Client);

            var request = new CreateClientRequest
            {
                ClientType = ClientType.Individual,
                Name = "John",
                IdentificationNumber = "1234567890123",
                Email = "a@b.com",
                Phone = "123"
            };

            var dto = await service.CreateClientAsync(request);

            Assert.NotNull(dto);
            Assert.Equal(request.Name, dto.Name);
            Assert.Equal(request.IdentificationNumber, dto.IdentificationNumber);
            Assert.Single(_fakeRepositories.Client.Storage);
        }

        [Fact]
        public async Task CreateClientAsync_Should_Create_Company_With_RO_Prefix()
        {
            var service = new ClientService(_fakeRepositories.Client);

            var request = new CreateClientRequest
            {
                ClientType = ClientType.Company,
                Name = "ACME",
                IdentificationNumber = "RO12345",
                Email = "info@acme.com"
            };

            var dto = await service.CreateClientAsync(request);

            Assert.NotNull(dto);
            Assert.Equal(request.Name, dto.Name);
            Assert.Equal(request.IdentificationNumber, dto.IdentificationNumber);
        }

        [Fact]
        public async Task CreateClientAsync_Should_Throw_When_Identification_Invalid_For_Individual()
        {
            var service = new ClientService(_fakeRepositories.Client);

            var request = new CreateClientRequest
            {
                ClientType = ClientType.Individual,
                Name = "John",
                IdentificationNumber = "ABC"
            };

            await Assert.ThrowsAsync<ArgumentException>(() => service.CreateClientAsync(request));
        }

        [Fact]
        public async Task CreateAsync_Should_Throw_When_Duplicate_Identification()
        {
            var existing = new Client(ClientType.Individual, "Existing", "1234567890123");
            await _fakeRepositories.Client.AddClientAsync(existing);

            var service = new ClientService(_fakeRepositories.Client);

            var request = new CreateClientRequest
            {
                ClientType = ClientType.Individual,
                Name = "New",
                IdentificationNumber = "1234567890123"
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateClientAsync(request));
            Assert.Equal("A client with this identification number already exists.", ex.Message);
        }

        [Fact]
        public async Task GetClientByIdAsync_Should_Return_Dto_When_Found()
        {
            var client = new Client(ClientType.Individual, "John", "1234567890123");
            await _fakeRepositories.Client.AddClientAsync(client);

            var service = new ClientService(_fakeRepositories.Client);

            var dto = await service.GetClientByIdAsync(client.Id);

            Assert.NotNull(dto);
            Assert.Equal(client.Id, dto!.Id);
            Assert.Equal(client.Name, dto.Name);
        }

        [Fact]
        public async Task GetClientByIdAsync_Should_Return_Null_When_Not_Found()
        {
            var service = new ClientService(_fakeRepositories.Client);

            var dto = await service.GetClientByIdAsync(Guid.NewGuid());

            Assert.Null(dto);
        }

        [Fact]
        public async Task SearchClientAsync_Should_Return_All_Clients_When_No_Filters_Are_Provided()
        {
            var c1 = new Client(ClientType.Individual, "Alice", "1111111111111");
            var c2 = new Client(ClientType.Company, "Acme", "RO22222");
            await _fakeRepositories.Client.AddClientAsync(c1);
            await _fakeRepositories.Client.AddClientAsync(c2);

            var service = new ClientService(_fakeRepositories.Client);

            var all = await service.SearchClientAsync(null, null, new PaginationRequest { PageSize = 10 });
            Assert.Equal(2, all.TotalCount);
            Assert.Equal(2, all.Items.Count);
        }

        [Fact]
        public async Task SearchClientAsync_Should_Return_Matching_Clients_By_Name()
        {
            var c1 = new Client(ClientType.Individual, "Alice", "1111111111111");
            var c2 = new Client(ClientType.Company, "Acme", "RO22222");
            await _fakeRepositories.Client.AddClientAsync(c1);
            await _fakeRepositories.Client.AddClientAsync(c2);

            var service = new ClientService(_fakeRepositories.Client);

            var filtered = await service.SearchClientAsync("Acme", null, new PaginationRequest());
            Assert.Equal(1, filtered.TotalCount);
            var result = Assert.Single(filtered.Items);
            Assert.Equal(c2.Id, result.Id);
        }

        [Fact]
        public async Task UpdateClientAsync_Should_Update_When_Valid()
        {
            var client = new Client(ClientType.Individual, "John", "1234567890123");
            await _fakeRepositories.Client.AddClientAsync(client);

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

            var dto = await service.UpdateClientAsync(client.Id, update);

            Assert.Equal(client.Id, dto.Id);
            Assert.Equal(update.Name, dto.Name);
            Assert.Equal(update.Email, dto.Email);
            Assert.Equal(ClientType.Company, dto.ClientType);
        }

        [Fact]
        public async Task UpdateClientAsync_Should_Throw_When_Client_Not_Found()
        {
            var service = new ClientService(_fakeRepositories.Client);

            var update = new UpdateClientRequest
            {
                ClientType = ClientType.Individual,
                Name = "X",
                IdentificationNumber = "1234567890123"
            };

            var ex = await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateClientAsync(Guid.NewGuid(), update));
            Assert.Equal("Client was not found.", ex.Message);
        }

        [Fact]
        public async Task UpdateClientAsync_Should_Throw_When_Identification_Changed()
        {
            var client = new Client(ClientType.Individual, "John", "1234567890123");
            await _fakeRepositories.Client.AddClientAsync(client);

            var service = new ClientService(_fakeRepositories.Client);

            var update = new UpdateClientRequest
            {
                ClientType = ClientType.Individual,
                Name = "John",
                IdentificationNumber = "9999999999999"
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateClientAsync(client.Id, update));
            Assert.Equal("The client identification number cannot be changed.", ex.Message);
        }

        [Fact]
        public async Task UpdateClientAsync_Should_Throw_When_Identification_Exists_For_Other()
        {
            var client1 = new Client(ClientType.Individual, "A", "1234567890123");
            var client2 = new Client(ClientType.Individual, "B", "9999999999999");
            await _fakeRepositories.Client.AddClientAsync(client1);
            await _fakeRepositories.Client.AddClientAsync(client2);

            var service = new ClientService(_fakeRepositories.Client);

            var update = new UpdateClientRequest
            {
                ClientType = ClientType.Individual,
                Name = "A",
                IdentificationNumber = client2.IdentificationNumber // attempt to set to other client's id
            };

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateClientAsync(client1.Id, update));
            Assert.Equal("The client identification number cannot be changed.", ex.Message);
        }
    }
}
