using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Client;
using InsuranceApp.Application.Exceptions;
using InsuranceApp.Application.Services;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;
using InsuranceApp.UnitTests.Common;
using Microsoft.Extensions.Logging;
using Moq;

namespace InsuranceApp.UnitTests.Application.Services;

public sealed class ClientServiceTests
{
    private readonly Mock<IClientRepository> _repositoryMock;
    private readonly Mock<ILogger<ClientService>> _loggerMock;
    private readonly ClientService _service;

    public ClientServiceTests()
    {
        _repositoryMock = new Mock<IClientRepository>();
        _loggerMock = new Mock<ILogger<ClientService>>();

        _service = new ClientService(
            _repositoryMock.Object,
            _loggerMock.Object);
    }


    #region Read Client Tests

    [Fact]
    public async Task GetClientByIdAsync_ExistingClient_ReturnsSuccess()
    {
        // Arrange
        var client = CreateClientEntity();

        _repositoryMock
            .Setup(x => x.GetClientByIdAsync(
                client.ClientId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        // Act
        var result = await _service.GetClientByIdAsync(
            client.ClientId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(client.ClientId, result.Value.ClientId);
        Assert.Equal(client.ClientType, result.Value.ClientType);
        Assert.Equal(client.Name, result.Value.Name);
        Assert.Equal(
            client.IdentificationNumber,
            result.Value.IdentificationNumber);
        Assert.Equal(client.Email, result.Value.Email);
    }

    [Fact]
    public async Task GetClientByIdAsync_NonExistingClient_ReturnsNotFound()
    {
        // Arrange
        const int clientId = TestConstants.NonExistingId;

        _repositoryMock
            .Setup(x => x.GetClientByIdAsync(
                clientId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        // Act
        var result = await _service.GetClientByIdAsync(
            clientId,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetClientByIdAsync_InvalidClientId_ReturnsValidationError(int clientId)
    {
        // Act
        var result = await _service.GetClientByIdAsync(
            clientId,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);

        _repositoryMock.Verify(
            x => x.GetClientByIdAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    #endregion


    #region Search Clients Tests

    [Fact]
    public async Task SearchClientsAsync_ValidSearch_ReturnsPagedResult()
    {
        // Arrange
        var search = new ClientSearchDto(
            "John",
            null,
            1,
            20);

        var clients = new List<Client>
        {
            CreateClientEntity(1),

            new()
            {
                ClientId = 2,
                ClientType = ClientType.Individual,
                Name = "John Smith",
                IdentificationNumber = "2990101223344",
                Email = "john.smith@test.com",
                CreatedAt = DateTime.UtcNow
            }
        };

        _repositoryMock
            .Setup(x => x.SearchClientAsync(
                "John",
                null,
                1,
                20,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((clients, 2));

        // Act
        var result = await _service.SearchClientsAsync(
            search,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(2, result.Value.Items.Count);
        Assert.Equal(2, result.Value.TotalCount);
        Assert.Equal(1, result.Value.PageNumber);
        Assert.Equal(20, result.Value.PageSize);

        Assert.Equal("John Doe", result.Value.Items[0].Name);
        Assert.Equal("John Smith", result.Value.Items[1].Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SearchClientsAsync_InvalidPageNumber_ReturnsValidationError(int pageNumber)
    {
        // Arrange
        var search = new ClientSearchDto(
            null,
            null,
            pageNumber,
            20);

        // Act
        var result = await _service.SearchClientsAsync(
            search,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);

        _repositoryMock.Verify(
            x => x.SearchClientAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1001)]
    public async Task SearchClientsAsync_InvalidPageSize_ReturnsValidationError(int pageSize)
    {
        // Arrange
        var search = new ClientSearchDto(
            null,
            null,
            1,
            pageSize);

        // Act
        var result = await _service.SearchClientsAsync(
            search,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);

        _repositoryMock.Verify(
            x => x.SearchClientAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SearchClientsAsync_TrimsSearchParameters()
    {
        // Arrange
        var search = new ClientSearchDto(
            "  John  ",
            "  1980101223344  ",
            1,
            20);

        _repositoryMock
            .Setup(x => x.SearchClientAsync(
                "John",
                "1980101223344",
                1,
                20,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Client>(), 0));

        // Act
        var result = await _service.SearchClientsAsync(
            search,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _repositoryMock.Verify(
            x => x.SearchClientAsync(
                "John",
                "1980101223344",
                1,
                20,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    #endregion


    #region Create Client Tests

    [Fact]
    public async Task CreateClientAsync_ValidClient_ReturnsSuccess()
    {
        // Arrange
        var dto = CreateValidClientDto();

        _repositoryMock
            .Setup(x => x.ClientIdentificationNumberExistsAsync(
                dto.IdentificationNumber,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(dto.ClientType, result.Value.ClientType);
        Assert.Equal(dto.Name, result.Value.Name);
        Assert.Equal(
            dto.IdentificationNumber,
            result.Value.IdentificationNumber);
        Assert.Equal(dto.Email, result.Value.Email);
        Assert.Equal(dto.Phone, result.Value.Phone);
        Assert.Equal(dto.Address, result.Value.Address);

        _repositoryMock.Verify(
            x => x.AddClientAsync(
                It.Is<Client>(client =>
                    client.Name == dto.Name &&
                    client.IdentificationNumber == dto.IdentificationNumber &&
                    client.ClientType == dto.ClientType),
                It.IsAny<CancellationToken>()),
            Times.Once);

    }

    [Fact]
    public async Task CreateClientAsync_MissingName_ReturnsValidationError()
    {
        // Arrange
        var dto = CreateValidClientDto() with
        {
            Name = ""
        };

        // Act
        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);

        _repositoryMock.Verify(
            x => x.AddClientAsync(
                It.IsAny<Client>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateClientAsync_MissingIdentificationNumber_ReturnsValidationError()
    {
        // Arrange
        var dto = CreateValidClientDto() with
        {
            IdentificationNumber = ""
        };

        // Act
        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);

        _repositoryMock.Verify(
            x => x.AddClientAsync(
                It.IsAny<Client>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateClientAsync_InvalidClientType_ReturnsValidationError()
    {
        // Arrange
        var dto = CreateValidClientDto() with
        {
            ClientType = (ClientType)TestConstants.NonExistingId
        };

        // Act
        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);

        _repositoryMock.Verify(
            x => x.AddClientAsync(
                It.IsAny<Client>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateClientAsync_InvalidEmail_ReturnsValidationError()
    {
        // Arrange
        var dto = CreateValidClientDto() with
        {
            Email = "invalid-email"
        };

        // Act
        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);

        _repositoryMock.Verify(
            x => x.AddClientAsync(
                It.IsAny<Client>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateClientAsync_DuplicateIdentificationNumber_ReturnsConflict()
    {
        // Arrange
        var dto = CreateValidClientDto();

        _repositoryMock
            .Setup(x => x.ClientIdentificationNumberExistsAsync(
                dto.IdentificationNumber,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);

        _repositoryMock.Verify(
            x => x.AddClientAsync(
                It.IsAny<Client>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

    }

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    public async Task CreateClientAsync_NameTooShort_ReturnsValidationError(
    string name)
    {
        // Arrange
        var dto = CreateValidClientDto() with
        {
            Name = name
        };

        // Act
        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(ClientErrors.InvalidNameLength.Code, result.Error.Code);

        _repositoryMock.Verify(
            x => x.AddClientAsync(
                It.IsAny<Client>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateClientAsync_NameTooLong_ReturnsValidationError()
    {
        // Arrange
        var dto = CreateValidClientDto() with
        {
            Name = new string('A', 201)
        };

        // Act
        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            ClientErrors.InvalidNameLength.Code,
            result.Error!.Code);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("12")]
    public async Task CreateClientAsync_IdentificationNumberTooShort_ReturnsValidationError(
    string identificationNumber)
    {
        // Arrange
        var dto = CreateValidClientDto() with
        {
            IdentificationNumber = identificationNumber
        };

        // Act
        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            ClientErrors.InvalidIdentificationNumberLength.Code,
            result.Error!.Code);

        _repositoryMock.Verify(
            x => x.ClientIdentificationNumberExistsAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateClientAsync_IdentificationNumberTooLong_ReturnsValidationError()
    {
        // Arrange
        var dto = CreateValidClientDto() with
        {
            IdentificationNumber = new string('1', 51)
        };

        // Act
        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            ClientErrors.InvalidIdentificationNumberLength.Code,
            result.Error!.Code);
    }

    [Fact]
    public async Task CreateClientAsync_EmailTooLong_ReturnsValidationError()
    {
        // Arrange
        var dto = CreateValidClientDto() with
        {
            Email = $"{new string('a', 195)}@test.com"
        };

        // Act
        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            ClientErrors.InvalidEmail.Code,
            result.Error!.Code);
    }

    [Fact]
    public async Task CreateClientAsync_PhoneTooLong_ReturnsValidationError()
    {
        // Arrange
        var dto = CreateValidClientDto() with
        {
            Phone = new string('1', 51)
        };

        // Act
        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            ClientErrors.InvalidPhoneLength.Code,
            result.Error!.Code);
    }

    [Fact]
    public async Task CreateClientAsync_AddressTooLong_ReturnsValidationError()
    {
        // Arrange
        var dto = CreateValidClientDto() with
        {
            Address = new string('A', 301)
        };

        // Act
        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            ClientErrors.InvalidAddressLength.Code,
            result.Error!.Code);
    }

    [Fact]
    public async Task CreateClientAsync_ValidClient_TrimsInputValues()
    {
        // Arrange
        var dto = CreateValidClientDto() with
        {
            Name = "  John Doe  ",
            IdentificationNumber = "  1980101223344  ",
            Email = "  john@test.com  ",
            Phone = "  0712345678  ",
            Address = "  Cluj-Napoca  "
        };

        _repositoryMock
            .Setup(x => x.ClientIdentificationNumberExistsAsync(
                "1980101223344",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        Assert.Equal("John Doe", result.Value!.Name);
        Assert.Equal(
            "1980101223344",
            result.Value.IdentificationNumber);
        Assert.Equal("john@test.com", result.Value.Email);
        Assert.Equal("0712345678", result.Value.Phone);
        Assert.Equal("Cluj-Napoca", result.Value.Address);
    }

    [Fact]
    public async Task CreateClientAsync_DuplicateOnInsertConcurrency_ReturnsConflict()
    {
        // Arrange
        var dto = CreateValidClientDto();

        // Pre-check does not find a duplicate.
        _repositoryMock
            .Setup(x => x.ClientIdentificationNumberExistsAsync(
                dto.IdentificationNumber,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Simulates another request inserting the same IdentificationNumber
        // between the pre-check and the database insert.
        _repositoryMock
            .Setup(x => x.AddClientAsync(
                It.IsAny<Client>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new DuplicateEntityException(nameof(Client)));

        // Act
        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(
            ClientErrors.DuplicateIdentificationNumber.Code,
            result.Error.Code);

        _repositoryMock.Verify(
            x => x.AddClientAsync(
                It.IsAny<Client>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    #endregion


    #region Update Client Tests
    [Fact]
    public async Task UpdateClientAsync_ValidClient_ReturnsUpdatedClient()
    {
        // Arrange
        var client = CreateClientEntity();

        var originalIdentificationNumber =
            client.IdentificationNumber;

        var dto = new UpdateClientDto(
            "John Updated",
            "john.updated@test.com",
            "0700123456",
            "Bucharest");

        _repositoryMock
            .Setup(x => x.GetClientForUpdateAsync(
                client.ClientId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(client);

        // Act
        var result = await _service.UpdateClientAsync(
            client.ClientId,
            dto,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal("John Updated", result.Value.Name);
        Assert.Equal("john.updated@test.com", result.Value.Email);
        Assert.Equal("0700123456", result.Value.Phone);
        Assert.Equal("Bucharest", result.Value.Address);

        // IdentificationNumber must not be changed during update.
        Assert.Equal(
            originalIdentificationNumber,
            result.Value.IdentificationNumber);

        Assert.NotNull(client.ModifiedAt);

        _repositoryMock.Verify(
            x => x.SaveClientChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateClientAsync_NonExistingClient_ReturnsNotFound()
    {
        // Arrange
        const int clientId = TestConstants.NonExistingId;

        var dto = CreateValidUpdateClientDto();

        _repositoryMock
            .Setup(x => x.GetClientForUpdateAsync(
                clientId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        // Act
        var result = await _service.UpdateClientAsync(
            clientId,
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);

        _repositoryMock.Verify(
            x => x.SaveClientChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task UpdateClientAsync_InvalidClientId_ReturnsValidationError(
        int clientId)
    {
        // Arrange
        var dto = CreateValidUpdateClientDto();

        // Act
        var result = await _service.UpdateClientAsync(
            clientId,
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);

        _repositoryMock.Verify(
            x => x.GetClientForUpdateAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateClientAsync_MissingName_ReturnsValidationError()
    {
        // Arrange
        var dto = CreateValidUpdateClientDto() with
        {
            Name = ""
        };

        // Act
        var result = await _service.UpdateClientAsync(
            1,
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);

        _repositoryMock.Verify(
            x => x.GetClientForUpdateAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateClientAsync_InvalidEmail_ReturnsValidationError()
    {
        // Arrange
        var dto = CreateValidUpdateClientDto() with
        {
            Email = "invalid-email"
        };

        // Act
        var result = await _service.UpdateClientAsync(
            1,
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);

        _repositoryMock.Verify(
            x => x.GetClientForUpdateAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    public async Task UpdateClientAsync_NameTooShort_ReturnsValidationError(
    string name)
    {
        // Arrange
        var dto = CreateValidUpdateClientDto() with
        {
            Name = name
        };

        // Act
        var result = await _service.UpdateClientAsync(
            1,
            dto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            ClientErrors.InvalidNameLength.Code,
            result.Error!.Code);

        _repositoryMock.Verify(
            x => x.GetClientForUpdateAsync(
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateClientAsync_NameTooLong_ReturnsValidationError()
    {
        var dto = CreateValidUpdateClientDto() with
        {
            Name = new string('A', 201)
        };

        var result = await _service.UpdateClientAsync(
            1,
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ClientErrors.InvalidNameLength.Code,
            result.Error!.Code);
    }

    [Fact]
    public async Task UpdateClientAsync_EmailTooLong_ReturnsValidationError()
    {
        var dto = CreateValidUpdateClientDto() with
        {
            Email = $"{new string('a', 195)}@test.com"
        };

        var result = await _service.UpdateClientAsync(
            1,
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ClientErrors.InvalidEmail.Code,
            result.Error!.Code);
    }

    [Fact]
    public async Task UpdateClientAsync_PhoneTooLong_ReturnsValidationError()
    {
        var dto = CreateValidUpdateClientDto() with
        {
            Phone = new string('1', 51)
        };

        var result = await _service.UpdateClientAsync(
            1,
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ClientErrors.InvalidPhoneLength.Code,
            result.Error!.Code);
    }

    [Fact]
    public async Task UpdateClientAsync_AddressTooLong_ReturnsValidationError()
    {
        var dto = CreateValidUpdateClientDto() with
        {
            Address = new string('A', 301)
        };

        var result = await _service.UpdateClientAsync(
            1,
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ClientErrors.InvalidAddressLength.Code,
            result.Error!.Code);
    }
    #endregion


    #region Helpers

    private static CreateClientDto CreateValidClientDto()
    {
        return new CreateClientDto(
            ClientType.Individual,
            "John Doe",
            "1980101223344",
            "john@test.com",
            "0712345678",
            "Cluj-Napoca");
    }

    private static UpdateClientDto CreateValidUpdateClientDto()
    {
        return new UpdateClientDto(
            "John Updated",
            "john.updated@test.com",
            "0700123456",
            "Bucharest");
    }

    private static Client CreateClientEntity(int clientId = 1)
    {
        return new Client
        {
            ClientId = clientId,
            ClientType = ClientType.Individual,
            Name = "John Doe",
            IdentificationNumber = "1980101223344",
            Email = "john@test.com",
            Phone = "0712345678",
            Address = "Cluj-Napoca",
            CreatedAt = DateTime.UtcNow
        };
    }
    #endregion

}