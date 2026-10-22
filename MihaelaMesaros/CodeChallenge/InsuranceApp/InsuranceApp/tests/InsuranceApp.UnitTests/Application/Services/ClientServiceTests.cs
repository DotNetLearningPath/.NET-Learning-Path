using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Client;
using InsuranceApp.Application.Exceptions;
using InsuranceApp.Application.Services;
using InsuranceApp.Domain.Constants;
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

        _service = new ClientService(_repositoryMock.Object, _loggerMock.Object);
    }

    #region Read Client Tests

    [Fact]
    public async Task GetClientByIdAsync_ExistingClient_ReturnsSuccess()
    {
        // Arrange
        var client = TestData.CreateClient1();

        _repositoryMock.Setup(x => x.GetClientByIdAsync(client.ClientId, It.IsAny<CancellationToken>())).ReturnsAsync(client);

        // Act
        var result = await _service.GetClientByIdAsync(client.ClientId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(client.ClientId, result.Value.ClientId);
        Assert.Equal(client.ClientType, result.Value.ClientType);
        Assert.Equal(client.Name, result.Value.Name);
        Assert.Equal(client.IdentificationNumber, result.Value.IdentificationNumber);
        Assert.Equal(client.Email, result.Value.Email);
        Assert.Equal(client.Phone, result.Value.Phone);
        Assert.Equal(client.Address, result.Value.Address);
    }

    [Fact]
    public async Task GetClientByIdAsync_NonExistingClient_ReturnsNotFound()
    {
        // Arrange
        var clientId = TestData.NonExistingId;

        _repositoryMock.Setup(x => x.GetClientByIdAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync((Client?)null);

        // Act
        var result = await _service.GetClientByIdAsync(clientId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(ClientErrors.NotFound(clientId).Code, result.Error.Code);
    }

    [Fact]
    public async Task GetClientByIdAsync_EmptyClientId_ReturnsValidationError()
    {
        // Arrange
        var clientId = Guid.Empty;

        // Act
        var result = await _service.GetClientByIdAsync(clientId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(ClientErrors.InvalidClientId.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.GetClientByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region Search Clients Tests

    [Fact]
    public async Task SearchClientsAsync_ValidSearch_ReturnsPagedResult()
    {
        // Arrange
        var client1 = TestData.CreateClient1();
        var client2 = TestData.CreateClient2();
        var clients = new List<Client> { client1, client2 };
        var search = new ClientSearchDto("John", null, 1, 20);

        _repositoryMock.Setup(x => x.SearchClientAsync("John", null, search.PageNumber, search.PageSize, It.IsAny<CancellationToken>())).ReturnsAsync((clients, clients.Count));

        // Act
        var result = await _service.SearchClientsAsync(search, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(clients.Count, result.Value.Items.Count);
        Assert.Equal(clients.Count, result.Value.TotalCount);
        Assert.Equal(search.PageNumber, result.Value.PageNumber);
        Assert.Equal(search.PageSize, result.Value.PageSize);
        Assert.Equal(client1.Name, result.Value.Items[0].Name);
        Assert.Equal(client2.Name, result.Value.Items[1].Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task SearchClientsAsync_InvalidPageNumber_ReturnsValidationError(int pageNumber)
    {
        // Arrange
        var search = new ClientSearchDto(null, null, pageNumber, 20);

        // Act
        var result = await _service.SearchClientsAsync(search, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(ClientErrors.InvalidPageNumber.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SearchClientAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1001)]
    public async Task SearchClientsAsync_InvalidPageSize_ReturnsValidationError(int pageSize)
    {
        // Arrange
        var search = new ClientSearchDto(null, null, 1, pageSize);

        // Act
        var result = await _service.SearchClientsAsync(search, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(ClientErrors.InvalidPageSize.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SearchClientAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SearchClientsAsync_TrimsSearchParameters()
    {
        // Arrange
        var client = TestData.CreateClient1();
        var searchName = client.Name.Split(' ')[0];

        var search = new ClientSearchDto(
            $"  {searchName}  ",
            $"  {client.IdentificationNumber}  ",
            1,
            20);

        _repositoryMock.Setup(x => x.SearchClientAsync(searchName, client.IdentificationNumber, search.PageNumber, search.PageSize, It.IsAny<CancellationToken>())).ReturnsAsync((new List<Client>(), 0));

        // Act
        var result = await _service.SearchClientsAsync(search, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);

        _repositoryMock.Verify(x => x.SearchClientAsync(searchName, client.IdentificationNumber, search.PageNumber, search.PageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Create Client Tests

    [Fact]
    public async Task CreateClientAsync_ValidClient_ReturnsSuccess()
    {
        // Arrange
        var clientDto = TestData.CreateClientDto();

        SetupIdentificationNumberDoesNotExist(clientDto);

        // Act
        var result = await _service.CreateClientAsync(clientDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(clientDto.ClientType, result.Value.ClientType);
        Assert.Equal(clientDto.Name, result.Value.Name);
        Assert.Equal(clientDto.IdentificationNumber, result.Value.IdentificationNumber);
        Assert.Equal(clientDto.Email, result.Value.Email);
        Assert.Equal(clientDto.Phone, result.Value.Phone);
        Assert.Equal(clientDto.Address, result.Value.Address);

        _repositoryMock.Verify(x => x.AddClientAsync(
            It.Is<Client>(client =>
                client.Name == clientDto.Name &&
                client.IdentificationNumber == clientDto.IdentificationNumber &&
                client.ClientType == clientDto.ClientType),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateClientAsync_MissingName_ReturnsValidationError()
    {
        // Arrange
        var clientDto = TestData.CreateClientDto() with { Name = "" };

        // Act & Assert
        await AssertInvalidCreateClientAsync(clientDto, ClientErrors.NameRequired);
    }

    [Fact]
    public async Task CreateClientAsync_MissingIdentificationNumber_ReturnsValidationError()
    {
        // Arrange
        var clientDto = TestData.CreateClientDto() with { IdentificationNumber = "" };

        // Act & Assert
        await AssertInvalidCreateClientAsync(clientDto, ClientErrors.IdentificationNumberRequired);
    }

    [Fact]
    public async Task CreateClientAsync_InvalidClientType_ReturnsValidationError()
    {
        // Arrange
        var clientDto = TestData.CreateClientDto() with { ClientType = (ClientType)999 };

        // Act & Assert
        await AssertInvalidCreateClientAsync(clientDto, ClientErrors.InvalidClientType);
    }

    [Fact]
    public async Task CreateClientAsync_InvalidEmail_ReturnsValidationError()
    {
        // Arrange
        var clientDto = TestData.CreateClientDto() with { Email = "invalid-email" };

        // Act & Assert
        await AssertInvalidCreateClientAsync(clientDto, ClientErrors.InvalidEmail);
    }

    [Fact]
    public async Task CreateClientAsync_DuplicateIdentificationNumber_ReturnsConflict()
    {
        // Arrange
        var clientDto = TestData.CreateClientDto();

        _repositoryMock.Setup(x => x.ClientIdentificationNumberExistsAsync(clientDto.IdentificationNumber, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _service.CreateClientAsync(clientDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(ClientErrors.DuplicateIdentificationNumber.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.AddClientAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    public async Task CreateClientAsync_NameTooShort_ReturnsValidationError(string name)
    {
        // Arrange
        var clientDto = TestData.CreateClientDto() with { Name = name };

        // Act & Assert
        await AssertInvalidCreateClientAsync(clientDto, ClientErrors.InvalidNameLength);
    }

    [Fact]
    public async Task CreateClientAsync_NameTooLong_ReturnsValidationError()
    {
        // Arrange
        var clientDto = TestData.CreateClientDto() with { Name = new string('A', ClientConstraints.NameMaxLength + 1) };

        // Act & Assert
        await AssertInvalidCreateClientAsync(clientDto, ClientErrors.InvalidNameLength);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("12")]
    public async Task CreateClientAsync_IdentificationNumberTooShort_ReturnsValidationError(string identificationNumber)
    {
        // Arrange
        var clientDto = TestData.CreateClientDto() with { IdentificationNumber = identificationNumber };

        // Act & Assert
        await AssertInvalidCreateClientAsync(clientDto, ClientErrors.InvalidIdentificationNumberLength);

        _repositoryMock.Verify(x => x.ClientIdentificationNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateClientAsync_IdentificationNumberTooLong_ReturnsValidationError()
    {
        // Arrange
        var clientDto = TestData.CreateClientDto() with { IdentificationNumber = new string('1', ClientConstraints.IdentificationNumberMaxLength + 1) };

        // Act & Assert
        await AssertInvalidCreateClientAsync(clientDto, ClientErrors.InvalidIdentificationNumberLength);

        _repositoryMock.Verify(x => x.ClientIdentificationNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateClientAsync_EmailTooLong_ReturnsValidationError()
    {
        // Arrange
        var clientDto = TestData.CreateClientDto();
        var email = $"{new string('a', ClientConstraints.EmailMaxLength - TestData.EmailDomain.Length + 1)}{TestData.EmailDomain}";
        var invalidClientDto = clientDto with { Email = email };

        // Act & Assert
        await AssertInvalidCreateClientAsync(invalidClientDto, ClientErrors.InvalidEmail);
    }

    [Fact]
    public async Task CreateClientAsync_PhoneTooLong_ReturnsValidationError()
    {
        // Arrange
        var clientDto = TestData.CreateClientDto() with { Phone = new string('1', ClientConstraints.PhoneMaxLength + 1) };

        // Act & Assert
        await AssertInvalidCreateClientAsync(clientDto, ClientErrors.InvalidPhoneLength);
    }

    [Fact]
    public async Task CreateClientAsync_AddressTooLong_ReturnsValidationError()
    {
        // Arrange
        var clientDto = TestData.CreateClientDto() with { Address = new string('A', ClientConstraints.AddressMaxLength + 1) };

        // Act & Assert
        await AssertInvalidCreateClientAsync(clientDto, ClientErrors.InvalidAddressLength);
    }

    [Fact]
    public async Task CreateClientAsync_ValidClient_TrimsInputValues()
    {
        // Arrange
        var clientDto = TestData.CreateClientDto();

        var clientDtoWithSpaces = clientDto with
        {
            Name = $"  {clientDto.Name}  ",
            IdentificationNumber = $"  {clientDto.IdentificationNumber}  ",
            Email = $"  {clientDto.Email}  ",
            Phone = $"  {clientDto.Phone}  ",
            Address = $"  {clientDto.Address}  "
        };

        SetupIdentificationNumberDoesNotExist(clientDto);

        // Act
        var result = await _service.CreateClientAsync(clientDtoWithSpaces, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(clientDto.Name, result.Value.Name);
        Assert.Equal(clientDto.IdentificationNumber, result.Value.IdentificationNumber);
        Assert.Equal(clientDto.Email, result.Value.Email);
        Assert.Equal(clientDto.Phone, result.Value.Phone);
        Assert.Equal(clientDto.Address, result.Value.Address);
    }

    [Fact]
    public async Task CreateClientAsync_DuplicateOnInsertConcurrency_ReturnsConflict()
    {
        // Arrange
        var clientDto = TestData.CreateClientDto();

        SetupIdentificationNumberDoesNotExist(clientDto);

        _repositoryMock.Setup(x => x.AddClientAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>())).ThrowsAsync(new DuplicateEntityException(nameof(Client)));

        // Act
        var result = await _service.CreateClientAsync(clientDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
        Assert.Equal(ClientErrors.DuplicateIdentificationNumber.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.AddClientAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region Update Client Tests

    [Fact]
    public async Task UpdateClientAsync_ValidClient_ReturnsUpdatedClient()
    {
        // Arrange
        var client = TestData.CreateClient1();
        var clientDto = TestData.UpdateClientDto();
        var originalIdentificationNumber = client.IdentificationNumber;

        SetupExistingClientForUpdate(client);

        // Act
        var result = await _service.UpdateClientAsync(client.ClientId, clientDto, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(clientDto.Name, result.Value.Name);
        Assert.Equal(clientDto.Email, result.Value.Email);
        Assert.Equal(clientDto.Phone, result.Value.Phone);
        Assert.Equal(clientDto.Address, result.Value.Address);
        Assert.Equal(originalIdentificationNumber, result.Value.IdentificationNumber);
        Assert.NotNull(client.ModifiedAt);

        _repositoryMock.Verify(x => x.SaveClientChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateClientAsync_NonExistingClient_ReturnsNotFound()
    {
        // Arrange
        var clientId = TestData.NonExistingId;
        var clientDto = TestData.UpdateClientDto();

        _repositoryMock.Setup(x => x.GetClientForUpdateAsync(clientId, It.IsAny<CancellationToken>())).ReturnsAsync((Client?)null);

        // Act
        var result = await _service.UpdateClientAsync(clientId, clientDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
        Assert.Equal(ClientErrors.NotFound(clientId).Code, result.Error.Code);

        _repositoryMock.Verify(x => x.SaveClientChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateClientAsync_EmptyClientId_ReturnsValidationError()
    {
        // Arrange
        var clientId = Guid.Empty;
        var clientDto = TestData.UpdateClientDto();

        // Act
        var result = await _service.UpdateClientAsync(clientId, clientDto, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(ClientErrors.InvalidClientId.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.GetClientForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateClientAsync_MissingName_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClient1();
        var clientDto = TestData.UpdateClientDto() with { Name = "" };

        // Act & Assert
        await AssertInvalidUpdateClientAsync(client, clientDto, ClientErrors.NameRequired);
    }

    [Fact]
    public async Task UpdateClientAsync_InvalidEmail_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClient1();
        var clientDto = TestData.UpdateClientDto() with { Email = "invalid-email" };

        // Act & Assert
        await AssertInvalidUpdateClientAsync(client, clientDto, ClientErrors.InvalidEmail);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    public async Task UpdateClientAsync_NameTooShort_ReturnsValidationError(string name)
    {
        // Arrange
        var client = TestData.CreateClient1();
        var clientDto = TestData.UpdateClientDto() with { Name = name };

        // Act & Assert
        await AssertInvalidUpdateClientAsync(client, clientDto, ClientErrors.InvalidNameLength);
    }

    [Fact]
    public async Task UpdateClientAsync_NameTooLong_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClient1();
        var clientDto = TestData.UpdateClientDto() with { Name = new string('A', ClientConstraints.NameMaxLength + 1) };

        // Act & Assert
        await AssertInvalidUpdateClientAsync(client, clientDto, ClientErrors.InvalidNameLength);
    }

    [Fact]
    public async Task UpdateClientAsync_EmailTooLong_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClient1();
        var updateClientDto = TestData.UpdateClientDto();
        var email = $"{new string('a', ClientConstraints.EmailMaxLength - TestData.EmailDomain.Length + 1)}{TestData.EmailDomain}";
        var clientDto = updateClientDto with { Email = email };

        // Act & Assert
        await AssertInvalidUpdateClientAsync(client, clientDto, ClientErrors.InvalidEmail);
    }

    [Fact]
    public async Task UpdateClientAsync_PhoneTooLong_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClient1();
        var clientDto = TestData.UpdateClientDto() with { Phone = new string('1', ClientConstraints.PhoneMaxLength + 1) };

        // Act & Assert
        await AssertInvalidUpdateClientAsync(client, clientDto, ClientErrors.InvalidPhoneLength);
    }

    [Fact]
    public async Task UpdateClientAsync_AddressTooLong_ReturnsValidationError()
    {
        // Arrange
        var client = TestData.CreateClient1();
        var clientDto = TestData.UpdateClientDto() with { Address = new string('A', ClientConstraints.AddressMaxLength + 1) };

        // Act & Assert
        await AssertInvalidUpdateClientAsync(client, clientDto, ClientErrors.InvalidAddressLength);
    }

    #endregion


    #region Helpers

    private void SetupIdentificationNumberDoesNotExist(CreateClientDto clientDto)
    {
        _repositoryMock.Setup(x => x.ClientIdentificationNumberExistsAsync(clientDto.IdentificationNumber, It.IsAny<CancellationToken>())).ReturnsAsync(false);
    }

    private void SetupExistingClientForUpdate(Client client)
    {
        _repositoryMock.Setup(x => x.GetClientForUpdateAsync(client.ClientId, It.IsAny<CancellationToken>())).ReturnsAsync(client);
    }

    private async Task AssertInvalidCreateClientAsync(CreateClientDto clientDto, Error expectedError)
    {
        var result = await _service.CreateClientAsync(clientDto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(expectedError.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.ClientIdentificationNumberExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.AddClientAsync(It.IsAny<Client>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private async Task AssertInvalidUpdateClientAsync(Client client, UpdateClientDto clientDto, Error expectedError)
    {
        var result = await _service.UpdateClientAsync(client.ClientId, clientDto, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(ErrorType.Validation, result.Error.Type);
        Assert.Equal(expectedError.Code, result.Error.Code);

        _repositoryMock.Verify(x => x.GetClientForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _repositoryMock.Verify(x => x.SaveClientChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion
}
