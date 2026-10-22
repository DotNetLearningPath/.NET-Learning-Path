using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Application.DTO.Policies;
using Insurance.Application.DTO.Premiums;
using Insurance.Application.Exceptions;
using Insurance.Application.Services;
using Insurance.Domain.Entities;
using Insurance.Domain.Enums;
using Insurance.UnitTest.Application.Fakes;

namespace Insurance.UnitTest.Application.Services;

public sealed class PolicyServiceTest
{
    private readonly FakePolicyRepository _policies = new();
    private readonly FakeClientRepository _clients = new();
    private readonly FakeBuildingRepository _buildings = new();
    private readonly FakeBrokerRepository _brokers = new();
    private readonly FakeCurrencyRepository _currencies = new();
    private readonly FakePremiumCalculationService _premium = new();
    private readonly PolicyService _service;

    public PolicyServiceTest()
    {
        _service = new PolicyService(
            _policies,
            _clients,
            _buildings,
            _brokers,
            _currencies,
            _premium);
    }

    [Fact]
    public async Task CreatePolicyAsync_Should_Create_Draft_And_Map_Premium()
    {
        // Arrange
        var data = SeedValidDependencies();
        var request = CreateRequest(data);
        _premium.Result = 125m;

        // Act
        var result = await _service.CreatePolicyAsync(request, CancellationToken.None);

        // Assert
        Assert.Equal(PolicyStatus.Draft, result.Status);
        Assert.Equal(request.BasePremium, result.BasePremium);
        Assert.Equal(125m, result.FinalPremium);
        Assert.Equal(data.Client.Id, result.ClientId);
        Assert.Equal(data.Building.Id, result.BuildingId);
        Assert.Equal(data.Broker.Id, result.BrokerId);
        Assert.Equal(data.Currency.Id, result.CurrencyId);
        Assert.Single(_policies.AddedPolicies);
        Assert.Equal(data.Building.CityId, _premium.LastContext?.CityId);
        Assert.Equal(data.Building.Type, _premium.LastContext?.BuildingType);
    }

    [Fact]
    public async Task CreatePolicyAsync_Should_Throw_When_Request_Is_Null()
    {
        // Arrange

        // Act
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.CreatePolicyAsync(null!, CancellationToken.None));

        // Assert
    }

    [Fact]
    public async Task CreatePolicyAsync_Should_Throw_When_Client_Is_Not_Found()
    {
        // Arrange
        var request = CreateRequest();

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreatePolicyAsync(request, CancellationToken.None));

        // Assert
        Assert.Equal("Client was not found.", exception.Message);
    }

    [Fact]
    public async Task CreatePolicyAsync_Should_Throw_When_Building_Belongs_To_Another_Client()
    {
        // Arrange
        var client = CreateClient();
        _clients.Seed(client);
        var building = CreateBuilding(Guid.NewGuid());
        _buildings.Storage.Add(building);

        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreatePolicyAsync(
                CreateRequest(clientId: client.Id, buildingId: building.Id),
                CancellationToken.None));

        // Assert
        Assert.Equal("The building does not belong to the selected client.", exception.Message);
    }

    [Fact]
    public async Task CreatePolicyAsync_Should_Throw_When_Broker_Is_Inactive()
    {
        // Arrange
        var data = SeedValidDependencies(brokerStatus: BrokerStatus.Inactive);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreatePolicyAsync(CreateRequest(data), CancellationToken.None));

        // Assert
        Assert.Equal("Inactive brokers cannot create policies.", exception.Message);
        Assert.Empty(_policies.AddedPolicies);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task CreatePolicyAsync_Should_Throw_When_Base_Premium_Is_Not_Positive(decimal basePremium)
    {
        // Arrange
        var data = SeedValidDependencies();

        // Act
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _service.CreatePolicyAsync(
                CreateRequest(data, basePremium),
                CancellationToken.None));

        // Assert
        Assert.Empty(_policies.AddedPolicies);
    }

    [Fact]
    public async Task GetPolicyByIdAsync_Should_Return_Mapped_Policy()
    {
        // Arrange
        var policy = CreatePolicy();
        _policies.Item = policy;

        // Act
        var result = await _service.GetPolicyByIdAsync(policy.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(policy.Id, result.Id);
        Assert.Equal(policy.PolicyNumber, result.PolicyNumber);
        Assert.Equal(policy.CreatedAt, result.CreatedAt);
        Assert.Equal(policy.UpdatedAt, result.UpdatedAt);
    }

    [Fact]
    public async Task GetPolicyByIdAsync_Should_Return_Null_When_Policy_Is_Not_Found()
    {
        // Arrange

        // Act
        var result = await _service.GetPolicyByIdAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task SearchPolicyAsync_Should_Return_Paged_Mapped_Policies()
    {
        // Arrange
        var policy = CreatePolicy();
        _policies.SearchResult = new PagedResult<Policy>
        {
            Items = [policy],
            PageNumber = 2,
            PageSize = 1,
            TotalCount = 3
        };

        // Act
        var result = await _service.SearchPolicyAsync(
            "POL",
            policy.ClientId,
            policy.BrokerId,
            PolicyStatus.Active,
            policy.StartDate,
            policy.EndDate,
            new PaginationRequest { PageNumber = 2, PageSize = 1 },
            CancellationToken.None);

        // Assert
        var mapped = Assert.Single(result.Items);
        Assert.Equal(policy.Id, mapped.Id);
        Assert.Equal(2, result.PageNumber);
        Assert.Equal(1, result.PageSize);
        Assert.Equal(3, result.TotalCount);
    }

    [Fact]
    public async Task SearchPolicyAsync_Should_Throw_When_Date_Range_Is_Invalid()
    {
        // Arrange
        var from = new DateTime(2027, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.SearchPolicyAsync(
                null,
                null,
                null,
                null,
                from,
                to,
                new PaginationRequest(),
                CancellationToken.None));
    }

    [Fact]
    public async Task ActivatePolicyAsync_Should_Activate_And_Update_Policy()
    {
        // Arrange
        var policy = CreatePolicy(PolicyStatus.Draft, DateTime.UtcNow.Date.AddDays(1));
        _policies.Item = policy;

        // Act
        var result = await _service.ActivatePolicyAsync(policy.Id, CancellationToken.None);

        // Assert
        Assert.Equal(PolicyStatus.Active, result.Status);
        Assert.Same(policy, _policies.UpdatedPolicy);
    }

    [Fact]
    public async Task ActivatePolicyAsync_Should_Throw_When_Policy_Is_Not_Found()
    {
        // Arrange

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.ActivatePolicyAsync(Guid.NewGuid(), CancellationToken.None));

        // Assert
        Assert.Equal("Policy was not found.", exception.Message);
    }

    [Fact]
    public async Task CancelPolicyAsync_Should_Cancel_And_Update_Policy()
    {
        // Arrange
        var policy = CreatePolicy();
        _policies.Item = policy;
        var cancellationDate = new DateTime(2027, 6, 15, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = await _service.CancelPolicyAsync(
            policy.Id,
            new CancelPolicyRequest
            {
                Reason = "Customer request",
                CancellationDate = cancellationDate
            },
            CancellationToken.None);

        // Assert
        Assert.Equal(PolicyStatus.Cancelled, result.Status);
        Assert.Equal("Customer request", result.CancellationReason);
        Assert.Equal(cancellationDate, result.CancellationDate);
        Assert.Same(policy, _policies.UpdatedPolicy);
    }

    [Fact]
    public async Task CancelPolicyAsync_Should_Throw_When_Request_Is_Null()
    {
        // Arrange

        // Act
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            _service.CancelPolicyAsync(Guid.NewGuid(), null!, CancellationToken.None));

        // Assert
    }

    [Fact]
    public async Task CancelPolicyAsync_Should_Throw_When_Policy_Is_Not_Found()
    {
        // Arrange
        var request = new CancelPolicyRequest
        {
            Reason = "Customer request",
            CancellationDate = DateTime.UtcNow
        };

        // Act
        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CancelPolicyAsync(
                Guid.NewGuid(),
                request,
                CancellationToken.None));

        // Assert
        Assert.Equal("Policy was not found.", exception.Message);
    }

    private (Client Client, Building Building, Broker Broker, Currency Currency) SeedValidDependencies(
        BrokerStatus brokerStatus = BrokerStatus.Active)
    {
        var client = CreateClient();
        var building = CreateBuilding(client.Id);
        var broker = CreateBroker(brokerStatus);
        var currency = new Currency("RON", "Romanian Leu", 1m);

        _clients.Seed(client);
        _buildings.Storage.Add(building);
        _brokers.Storage[broker.Id] = broker;
        _currencies.Storage.Add(currency);

        return (client, building, broker, currency);
    }

    private static CreatePolicyRequest CreateRequest(
        (Client Client, Building Building, Broker Broker, Currency Currency) data,
        decimal basePremium = 100m) =>
        CreateRequest(data.Client.Id, data.Building.Id, data.Broker.Id, data.Currency.Id, basePremium);

    private static CreatePolicyRequest CreateRequest(
        Guid? clientId = null,
        Guid? buildingId = null,
        Guid? brokerId = null,
        Guid? currencyId = null,
        decimal basePremium = 100m) => new()
        {
            ClientId = clientId ?? Guid.NewGuid(),
            BuildingId = buildingId ?? Guid.NewGuid(),
            BrokerId = brokerId ?? Guid.NewGuid(),
            CurrencyId = currencyId ?? Guid.NewGuid(),
            StartDate = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            BasePremium = basePremium
        };

    private static Client CreateClient() =>
        new(ClientType.Individual, "Test Client", "1791210963990");

    private static Building CreateBuilding(Guid clientId) =>
        new(
            clientId,
            Guid.NewGuid(),
            "Test Street",
            "1",
            2000,
            BuildingType.Residential,
            2,
            100m,
            100000m);

    private static Broker CreateBroker(BrokerStatus status = BrokerStatus.Active) =>
        new("BR-001", "Test Broker", "broker@example.com", "123", status);

    private static Policy CreatePolicy(
        PolicyStatus status = PolicyStatus.Active,
        DateTime? startDate = null)
    {
        var effectiveStartDate = startDate ??
            new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        return new Policy(
            "POL-001",
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            effectiveStartDate,
            effectiveStartDate.AddYears(1),
            100m,
            120m,
            status);
    }

}
