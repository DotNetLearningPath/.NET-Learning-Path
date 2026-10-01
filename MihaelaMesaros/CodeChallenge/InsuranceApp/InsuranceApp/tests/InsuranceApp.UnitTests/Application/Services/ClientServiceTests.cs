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

    private readonly Guid _clientId;
    private readonly CreateClientDto _validCreateDto;
    private readonly UpdateClientDto _validUpdateDto;
    private readonly Client _existingClient;

    public ClientServiceTests()
    {
        _repositoryMock = new Mock<IClientRepository>();
        _loggerMock = new Mock<ILogger<ClientService>>();

        _service = new ClientService(
            _repositoryMock.Object,
            _loggerMock.Object);

        _clientId = Guid.NewGuid();

        _validCreateDto = new CreateClientDto(
            ClientType.Individual,
            "John Doe",
            "1980101223344",
            "john@test.com",
            "0712345678",
            "Cluj-Napoca");

        _validUpdateDto = new UpdateClientDto(
            "John Updated",
            "john.updated@test.com",
            "0700123456",
            "Bucharest");

        _existingClient = new Client
        {
            ClientId = _clientId,
            ClientType = ClientType.Individual,
            Name = "John Doe",
            IdentificationNumber = "1980101223344",
            Email = "john@test.com",
            Phone = "0712345678",
            Address = "Cluj-Napoca",
            CreatedAt = DateTime.UtcNow
        };
    }

    #region Read Client Tests

    [Fact]
    public async Task GetClientByIdAsync_ExistingClient_ReturnsSuccess()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.GetClientByIdAsync(
                _clientId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingClient);

        // Act
        var result = await _service.GetClientByIdAsync(
            _clientId,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(_clientId, result.Value.ClientId);
        Assert.Equal(_existingClient.ClientType, result.Value.ClientType);
        Assert.Equal(_existingClient.Name, result.Value.Name);
        Assert.Equal(
            _existingClient.IdentificationNumber,
            result.Value.IdentificationNumber);
        Assert.Equal(_existingClient.Email, result.Value.Email);
    }

    [Fact]
    public async Task GetClientByIdAsync_NonExistingClient_ReturnsNotFound()
    {
        // Arrange
        var clientId = TestConstants.NonExistingId;

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
        Assert.Equal(ClientErrors.NotFound(clientId).Code, result.Error.Code);
    }

    [Fact]
    public async Task GetClientByIdAsync_EmptyClientId_ReturnsValidationError()
    {
        // Act
        var result = await _service.GetClientByIdAsync(
            Guid.Empty,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(ClientErrors.InvalidClientId.Code, result.Error.Code);

        _repositoryMock.Verify(
            x => x.GetClientByIdAsync(
                It.IsAny<Guid>(),
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

        var secondClient = new Client
        {
            ClientId = Guid.NewGuid(),
            ClientType = ClientType.Individual,
            Name = "John Smith",
            IdentificationNumber = "2990101223344",
            Email = "john.smith@test.com",
            CreatedAt = DateTime.UtcNow
        };

        var clients = new List<Client>
        {
            _existingClient,
            secondClient
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
    public async Task SearchClientsAsync_InvalidPageNumber_ReturnsValidationError(
        int pageNumber)
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
    public async Task SearchClientsAsync_InvalidPageSize_ReturnsValidationError(
        int pageSize)
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
        SetupIdentificationNumberDoesNotExist();

        // Act
        var result = await _service.CreateClientAsync(
            _validCreateDto,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(_validCreateDto.ClientType, result.Value.ClientType);
        Assert.Equal(_validCreateDto.Name, result.Value.Name);
        Assert.Equal(
            _validCreateDto.IdentificationNumber,
            result.Value.IdentificationNumber);
        Assert.Equal(_validCreateDto.Email, result.Value.Email);
        Assert.Equal(_validCreateDto.Phone, result.Value.Phone);
        Assert.Equal(_validCreateDto.Address, result.Value.Address);

        _repositoryMock.Verify(
            x => x.AddClientAsync(
                It.Is<Client>(client =>
                    client.Name == _validCreateDto.Name &&
                    client.IdentificationNumber ==
                        _validCreateDto.IdentificationNumber &&
                    client.ClientType == _validCreateDto.ClientType),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateClientAsync_MissingName_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            Name = ""
        };

        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);

        VerifyAddClientNeverCalled();
    }

    [Fact]
    public async Task CreateClientAsync_MissingIdentificationNumber_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            IdentificationNumber = ""
        };

        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);

        VerifyAddClientNeverCalled();
    }

    [Fact]
    public async Task CreateClientAsync_InvalidClientType_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            ClientType = (ClientType)999
        };

        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);

        VerifyAddClientNeverCalled();
    }

    [Fact]
    public async Task CreateClientAsync_InvalidEmail_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            Email = "invalid-email"
        };

        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);

        VerifyAddClientNeverCalled();
    }

    [Fact]
    public async Task CreateClientAsync_DuplicateIdentificationNumber_ReturnsConflict()
    {
        // Arrange
        _repositoryMock
            .Setup(x => x.ClientIdentificationNumberExistsAsync(
                _validCreateDto.IdentificationNumber,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateClientAsync(
            _validCreateDto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);

        VerifyAddClientNeverCalled();
    }

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    public async Task CreateClientAsync_NameTooShort_ReturnsValidationError(
        string name)
    {
        var dto = _validCreateDto with
        {
            Name = name
        };

        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(ClientErrors.InvalidNameLength.Code, result.Error.Code);

        VerifyAddClientNeverCalled();
    }

    [Fact]
    public async Task CreateClientAsync_NameTooLong_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            Name = new string('A', 201)
        };

        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

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
        var dto = _validCreateDto with
        {
            IdentificationNumber = identificationNumber
        };

        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

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
        var dto = _validCreateDto with
        {
            IdentificationNumber = new string('1', 51)
        };

        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ClientErrors.InvalidIdentificationNumberLength.Code,
            result.Error!.Code);
    }

    [Fact]
    public async Task CreateClientAsync_EmailTooLong_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            Email = $"{new string('a', 195)}@test.com"
        };

        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ClientErrors.InvalidEmail.Code,
            result.Error!.Code);
    }

    [Fact]
    public async Task CreateClientAsync_PhoneTooLong_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            Phone = new string('1', 51)
        };

        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ClientErrors.InvalidPhoneLength.Code,
            result.Error!.Code);
    }

    [Fact]
    public async Task CreateClientAsync_AddressTooLong_ReturnsValidationError()
    {
        var dto = _validCreateDto with
        {
            Address = new string('A', 301)
        };

        var result = await _service.CreateClientAsync(
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ClientErrors.InvalidAddressLength.Code,
            result.Error!.Code);
    }

    [Fact]
    public async Task CreateClientAsync_ValidClient_TrimsInputValues()
    {
        // Arrange
        var dto = _validCreateDto with
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
        SetupIdentificationNumberDoesNotExist();

        _repositoryMock
            .Setup(x => x.AddClientAsync(
                It.IsAny<Client>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new DuplicateEntityException(nameof(Client)));

        // Act
        var result = await _service.CreateClientAsync(
            _validCreateDto,
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
        var originalIdentificationNumber =
            _existingClient.IdentificationNumber;

        SetupExistingClientForUpdate();

        // Act
        var result = await _service.UpdateClientAsync(
            _clientId,
            _validUpdateDto,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);

        Assert.Equal(_validUpdateDto.Name, result.Value.Name);
        Assert.Equal(_validUpdateDto.Email, result.Value.Email);
        Assert.Equal(_validUpdateDto.Phone, result.Value.Phone);
        Assert.Equal(_validUpdateDto.Address, result.Value.Address);

        Assert.Equal(
            originalIdentificationNumber,
            result.Value.IdentificationNumber);

        Assert.NotNull(_existingClient.ModifiedAt);

        _repositoryMock.Verify(
            x => x.SaveClientChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateClientAsync_NonExistingClient_ReturnsNotFound()
    {
        // Arrange
        var clientId = TestConstants.NonExistingId;

        _repositoryMock
            .Setup(x => x.GetClientForUpdateAsync(
                clientId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Client?)null);

        // Act
        var result = await _service.UpdateClientAsync(
            clientId,
            _validUpdateDto,
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

    [Fact]
    public async Task UpdateClientAsync_EmptyClientId_ReturnsValidationError()
    {
        // Act
        var result = await _service.UpdateClientAsync(
            Guid.Empty,
            _validUpdateDto,
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(ClientErrors.InvalidClientId.Code, result.Error.Code);

        _repositoryMock.Verify(
            x => x.GetClientForUpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateClientAsync_MissingName_ReturnsValidationError()
    {
        var dto = _validUpdateDto with
        {
            Name = ""
        };

        await AssertInvalidUpdateAsync(dto);
    }

    [Fact]
    public async Task UpdateClientAsync_InvalidEmail_ReturnsValidationError()
    {
        var dto = _validUpdateDto with
        {
            Email = "invalid-email"
        };

        await AssertInvalidUpdateAsync(dto);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    public async Task UpdateClientAsync_NameTooShort_ReturnsValidationError(
        string name)
    {
        var dto = _validUpdateDto with
        {
            Name = name
        };

        var result = await _service.UpdateClientAsync(
            _clientId,
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ClientErrors.InvalidNameLength.Code,
            result.Error!.Code);

        _repositoryMock.Verify(
            x => x.GetClientForUpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateClientAsync_NameTooLong_ReturnsValidationError()
    {
        var dto = _validUpdateDto with
        {
            Name = new string('A', 201)
        };

        var result = await _service.UpdateClientAsync(
            _clientId,
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
        var dto = _validUpdateDto with
        {
            Email = $"{new string('a', 195)}@test.com"
        };

        var result = await _service.UpdateClientAsync(
            _clientId,
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
        var dto = _validUpdateDto with
        {
            Phone = new string('1', 51)
        };

        var result = await _service.UpdateClientAsync(
            _clientId,
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
        var dto = _validUpdateDto with
        {
            Address = new string('A', 301)
        };

        var result = await _service.UpdateClientAsync(
            _clientId,
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            ClientErrors.InvalidAddressLength.Code,
            result.Error!.Code);
    }

    #endregion

    #region Helpers

    private void SetupIdentificationNumberDoesNotExist()
    {
        _repositoryMock
            .Setup(x => x.ClientIdentificationNumberExistsAsync(
                _validCreateDto.IdentificationNumber,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
    }

    private void SetupExistingClientForUpdate()
    {
        _repositoryMock
            .Setup(x => x.GetClientForUpdateAsync(
                _clientId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_existingClient);
    }

    private void VerifyAddClientNeverCalled()
    {
        _repositoryMock.Verify(
            x => x.AddClientAsync(
                It.IsAny<Client>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private async Task AssertInvalidUpdateAsync(
        UpdateClientDto dto)
    {
        var result = await _service.UpdateClientAsync(
            _clientId,
            dto,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);

        _repositoryMock.Verify(
            x => x.GetClientForUpdateAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion
}
