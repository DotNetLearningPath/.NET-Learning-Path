using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.Services;
using InsuranceApp.Domain.Entities;
using InsuranceApp.UnitTests.Common;
using Moq;

namespace InsuranceApp.UnitTests.Application.Services;

public sealed class PolicyReferenceServiceTests
{
    private readonly Mock<IClientRepository> _clientRepositoryMock;
    private readonly Mock<IBuildingRepository> _buildingRepositoryMock;
    private readonly Mock<IBrokerRepository> _brokerRepositoryMock;
    private readonly Mock<ICurrencyRepository> _currencyRepositoryMock;
    private readonly PolicyReferenceService _service;

    public PolicyReferenceServiceTests()
    {
        _clientRepositoryMock = new Mock<IClientRepository>();
        _buildingRepositoryMock = new Mock<IBuildingRepository>();
        _brokerRepositoryMock = new Mock<IBrokerRepository>();
        _currencyRepositoryMock = new Mock<ICurrencyRepository>();

        _service = new PolicyReferenceService(
            _clientRepositoryMock.Object,
            _buildingRepositoryMock.Object,
            _brokerRepositoryMock.Object,
            _currencyRepositoryMock.Object);
    }

    #region Validate Policy References Tests

    [Fact]
    public async Task ValidatePolicyReferencesAsync_ValidReferences_ReturnsSuccess()
    {
        // Arrange
        var policy = TestData.CreateDraftPolicy1();
        var policyDto = TestData.CreatePolicyDto(policy);
        var references = TestData.CreatePolicyReferences(policyDto);

        SetupValidReferences(references);

        // Act
        var result = await _service.ValidatePolicyReferencesAsync(policy.ClientId, policy.BuildingId, policy.BrokerId, policy.CurrencyId, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(policy.ClientId, result.Value.Client.ClientId);
        Assert.Equal(policy.BuildingId, result.Value.Building.BuildingId);
        Assert.Equal(policy.BrokerId, result.Value.Broker.BrokerId);
        Assert.Equal(policy.CurrencyId, result.Value.Currency.CurrencyId);
    }

    [Fact]
    public async Task ValidatePolicyReferencesAsync_ClientNotFound_ReturnsError()
    {
        // Arrange
        var policy = TestData.CreateDraftPolicy1();

        _clientRepositoryMock.Setup(x => x.GetClientByIdAsync(policy.ClientId, It.IsAny<CancellationToken>())).ReturnsAsync((Client?)null);

        // Act
        var result = await _service.ValidatePolicyReferencesAsync(policy.ClientId, policy.BuildingId, policy.BrokerId, policy.CurrencyId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(PolicyErrors.ClientNotFound.Type, result.Error.Type);
        Assert.Equal(PolicyErrors.ClientNotFound.Code, result.Error.Code);

        _buildingRepositoryMock.Verify(x => x.GetBuildingByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidatePolicyReferencesAsync_BuildingNotFound_ReturnsError()
    {
        // Arrange
        var policy = TestData.CreateDraftPolicy1();
        var policyDto = TestData.CreatePolicyDto(policy);
        var references = TestData.CreatePolicyReferences(policyDto);

        _clientRepositoryMock.Setup(x => x.GetClientByIdAsync(policy.ClientId, It.IsAny<CancellationToken>())).ReturnsAsync(references.Client);
        _buildingRepositoryMock.Setup(x => x.GetBuildingByIdAsync(policy.BuildingId, It.IsAny<CancellationToken>())).ReturnsAsync((Building?)null);

        // Act
        var result = await _service.ValidatePolicyReferencesAsync(policy.ClientId, policy.BuildingId, policy.BrokerId, policy.CurrencyId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(PolicyErrors.BuildingNotFound.Type, result.Error.Type);
        Assert.Equal(PolicyErrors.BuildingNotFound.Code, result.Error.Code);

        _brokerRepositoryMock.Verify(x => x.GetBrokerByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidatePolicyReferencesAsync_BuildingDoesNotBelongToClient_ReturnsError()
    {
        // Arrange
        var policy = TestData.CreateDraftPolicy1();
        var policyDto = TestData.CreatePolicyDto(policy);
        var references = TestData.CreatePolicyReferences(policyDto);
        references.Building.ClientId = Guid.NewGuid();

        SetupValidReferences(references);

        // Act
        var result = await _service.ValidatePolicyReferencesAsync(policy.ClientId, policy.BuildingId, policy.BrokerId, policy.CurrencyId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(PolicyErrors.BuildingDoesNotBelongToClient.Type, result.Error.Type);
        Assert.Equal(PolicyErrors.BuildingDoesNotBelongToClient.Code, result.Error.Code);

        _brokerRepositoryMock.Verify(x => x.GetBrokerByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidatePolicyReferencesAsync_BrokerNotFound_ReturnsError()
    {
        // Arrange
        var policy = TestData.CreateDraftPolicy1();
        var policyDto = TestData.CreatePolicyDto(policy);
        var references = TestData.CreatePolicyReferences(policyDto);

        SetupValidReferences(references);
        _brokerRepositoryMock.Setup(x => x.GetBrokerByIdAsync(policy.BrokerId, It.IsAny<CancellationToken>())).ReturnsAsync((Broker?)null);

        // Act
        var result = await _service.ValidatePolicyReferencesAsync(policy.ClientId, policy.BuildingId, policy.BrokerId, policy.CurrencyId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(PolicyErrors.BrokerNotFound.Type, result.Error.Type);
        Assert.Equal(PolicyErrors.BrokerNotFound.Code, result.Error.Code);

        _currencyRepositoryMock.Verify(x => x.GetCurrencyByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidatePolicyReferencesAsync_InactiveBroker_ReturnsError()
    {
        // Arrange
        var policy = TestData.CreateDraftPolicy1();
        var policyDto = TestData.CreatePolicyDto(policy);
        var references = TestData.CreatePolicyReferences(policyDto);
        references.Broker.IsActive = false;

        SetupValidReferences(references);

        // Act
        var result = await _service.ValidatePolicyReferencesAsync(policy.ClientId, policy.BuildingId, policy.BrokerId, policy.CurrencyId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(PolicyErrors.InactiveBroker.Type, result.Error.Type);
        Assert.Equal(PolicyErrors.InactiveBroker.Code, result.Error.Code);

        _currencyRepositoryMock.Verify(x => x.GetCurrencyByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ValidatePolicyReferencesAsync_CurrencyNotFound_ReturnsError()
    {
        // Arrange
        var policy = TestData.CreateDraftPolicy1();
        var policyDto = TestData.CreatePolicyDto(policy);
        var references = TestData.CreatePolicyReferences(policyDto);

        SetupValidReferences(references);
        _currencyRepositoryMock.Setup(x => x.GetCurrencyByIdAsync(policy.CurrencyId, It.IsAny<CancellationToken>())).ReturnsAsync((Currency?)null);

        // Act
        var result = await _service.ValidatePolicyReferencesAsync(policy.ClientId, policy.BuildingId, policy.BrokerId, policy.CurrencyId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(PolicyErrors.CurrencyNotFound.Type, result.Error.Type);
        Assert.Equal(PolicyErrors.CurrencyNotFound.Code, result.Error.Code);
    }

    [Fact]
    public async Task ValidatePolicyReferencesAsync_InactiveCurrency_ReturnsError()
    {
        // Arrange
        var policy = TestData.CreateDraftPolicy1();
        var policyDto = TestData.CreatePolicyDto(policy);
        var references = TestData.CreatePolicyReferences(policyDto);
        references.Currency.IsActive = false;

        SetupValidReferences(references);

        // Act
        var result = await _service.ValidatePolicyReferencesAsync(policy.ClientId, policy.BuildingId, policy.BrokerId, policy.CurrencyId, CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
        Assert.Equal(PolicyErrors.InactiveCurrency.Type, result.Error.Type);
        Assert.Equal(PolicyErrors.InactiveCurrency.Code, result.Error.Code);
    }

    #endregion

    #region Helpers

    private void SetupValidReferences(InsuranceApp.Application.Models.Services.PolicyReferences references)
    {
        _clientRepositoryMock.Setup(x => x.GetClientByIdAsync(references.Client.ClientId, It.IsAny<CancellationToken>())).ReturnsAsync(references.Client);
        _buildingRepositoryMock.Setup(x => x.GetBuildingByIdAsync(references.Building.BuildingId, It.IsAny<CancellationToken>())).ReturnsAsync(references.Building);
        _brokerRepositoryMock.Setup(x => x.GetBrokerByIdAsync(references.Broker.BrokerId, It.IsAny<CancellationToken>())).ReturnsAsync(references.Broker);
        _currencyRepositoryMock.Setup(x => x.GetCurrencyByIdAsync(references.Currency.CurrencyId, It.IsAny<CancellationToken>())).ReturnsAsync(references.Currency);
    }

    #endregion
}