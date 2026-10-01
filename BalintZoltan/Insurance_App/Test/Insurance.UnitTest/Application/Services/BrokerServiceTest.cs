using Insurance.Application.DTO.Brokers;
using Insurance.Domain.Enums;
using Insurance.Application.Exceptions;
using Insurance.Application.Services;
using Insurance.Domain.Entities;
using Insurance.UnitTest.Application.Fakes;

namespace Insurance.UnitTest.Application.Services;

public sealed class BrokerServiceTest
{
    private readonly FakeBrokerRepository _repository;
    private readonly BrokerService _service;

    public BrokerServiceTest()
    {
        _repository = new FakeBrokerRepository();
        _service = new BrokerService(_repository);
    }

    [Fact]
    public async Task CreateBrokerAsync_Should_Create_Active_Broker()
    {
        // Act
        var result = await _service.CreateBrokerAsync(CreateRequest(), CancellationToken.None);

        // Assert
        Assert.Equal("BR-001", result.BrokerCode);
        Assert.Equal(BrokerStatus.Active, result.Status);
        Assert.Single(_repository.Storage);
    }

    [Fact]
    public async Task CreateBrokerAsync_Should_Trim_Text_Values()
    {
        // Act
        var result = await _service.CreateBrokerAsync(CreateRequest(
            brokerCode: " BR-001 ", name: " Broker One ",
            email: " broker@example.com ", phone: " 123456 "), CancellationToken.None);

        // Assert
        Assert.Equal("BR-001", result.BrokerCode);
        Assert.Equal("Broker One", result.Name);
        Assert.Equal("broker@example.com", result.Email);
        Assert.Equal("123456", result.Phone);
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    [InlineData("user@")]
    public async Task CreateBrokerAsync_Should_Reject_Invalid_Email(string email)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateBrokerAsync(CreateRequest(email: email), CancellationToken.None));
    }

    [Theory]
    [InlineData("Broker code is required.", "", "Broker One", "broker@example.com", "123")]
    [InlineData("Broker name is required.", "BR-001", "", "broker@example.com", "123")]
    [InlineData("Broker phone is required.", "BR-001", "Broker One", "broker@example.com", "")]
    public async Task CreateBrokerAsync_Should_Reject_Missing_Required_Values(
        string expectedMessage, string brokerCode, string name, string email, string phone)
    {
        // Act
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateBrokerAsync(CreateRequest(brokerCode, name, email, phone), CancellationToken.None));

        // Assert
        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public async Task CreateBrokerAsync_Should_Reject_Duplicate_Code()
    {
        // Arrange
        await _repository.AddBrokerAsync(CreateBroker("BR-001", "Existing"), CancellationToken.None);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateBrokerAsync(CreateRequest(), CancellationToken.None));

        // Assert
        Assert.Equal("A broker with this code already exists.", exception.Message);
    }

    [Fact]
    public async Task UpdateBrokerAsync_Should_Update_Broker()
    {
        // Arrange
        var broker = await AddBrokerAsync();

        // Act
        var result = await _service.UpdateBrokerAsync(broker.Id, new UpdateBrokerRequest
        {
            BrokerCode = "BR-002",
            Name = "Updated Broker",
            Email = "new@example.com",
            Phone = "456",
            CommissionPercentage = 20
        }, CancellationToken.None);

        // Assert
        Assert.Equal("BR-002", result.BrokerCode);
        Assert.Equal("Updated Broker", result.Name);
        Assert.Equal("new@example.com", result.Email);
        Assert.Equal(20m, result.CommissionPercentage);
    }

    [Fact]
    public async Task UpdateBrokerAsync_Should_Reject_Duplicate_Code()
    {
        // Arrange
        await AddBrokerAsync("BR-001", "First");
        var second = await AddBrokerAsync("BR-002", "Second");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.UpdateBrokerAsync(
            second.Id, new UpdateBrokerRequest
            {
                BrokerCode = "BR-001",
                Name = "Second",
                Email = "second@example.com",
                Phone = "456"
            }, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateBrokerAsync_Should_Throw_When_Not_Found()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateBrokerAsync(Guid.NewGuid(), new UpdateBrokerRequest
            {
                BrokerCode = "BR-001",
                Name = "Broker",
                Email = "broker@example.com",
                Phone = "123"
            }, CancellationToken.None));
    }

    [Fact]
    public async Task GetBrokerByIdAsync_Should_Return_Broker_When_Found()
    {
        // Arrange
        var broker = await AddBrokerAsync();

        // Act
        var result = await _service.GetBrokerByIdAsync(broker.Id, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(broker.Id, result.Id);
    }

    [Fact]
    public async Task GetBrokerByIdAsync_Should_Return_Null_When_Not_Found()
    {
        // Act & Assert
        Assert.Null(await _service.GetBrokerByIdAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public async Task CreateBrokerAsync_Should_Accept_Commission_Boundaries(decimal commission)
    {
        // Act
        var result = await _service.CreateBrokerAsync(CreateRequest(commissionPercentage: commission), CancellationToken.None);

        // Assert
        Assert.Equal(commission, result.CommissionPercentage);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public async Task CreateBrokerAsync_Should_Reject_Out_Of_Range_Commission(decimal commission)
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            _service.CreateBrokerAsync(CreateRequest(commissionPercentage: commission), CancellationToken.None));
    }

    [Theory]
    [InlineData(true, BrokerStatus.Active, 1)]
    [InlineData(false, BrokerStatus.Inactive, 0)]
    public async Task Status_Commands_Should_Set_Expected_Status(
        bool activate, BrokerStatus expectedStatus, int expectedNumericValue)
    {
        // Arrange
        var broker = await AddBrokerAsync(status: activate ? BrokerStatus.Inactive : BrokerStatus.Active);

        // Act
        var result = activate
            ? await _service.ActivateBrokerAsync(broker.Id, CancellationToken.None)
            : await _service.DeactivateBrokerAsync(broker.Id, CancellationToken.None);

        // Assert
        Assert.Equal(expectedStatus, result.Status);
        Assert.Equal(expectedNumericValue, (int)result.Status);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Status_Commands_Should_Throw_When_Broker_Does_Not_Exist(bool activate)
    {
        // Act
        var action = activate
            ? _service.ActivateBrokerAsync(Guid.NewGuid(), CancellationToken.None)
            : _service.DeactivateBrokerAsync(Guid.NewGuid(), CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<NotFoundException>(() => action);
    }

    [Fact]
    public async Task ListBrokersAsync_Should_Return_Mapped_Paged_Result()
    {
        // Arrange
        await AddBrokerAsync("BR-001", "Alpha");
        await AddBrokerAsync("BR-002", "Beta");

        // Act
        var result = await _service.ListBrokersAsync(new() { PageSize = 1 }, CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal("Alpha", Assert.Single(result.Items).Name);
    }

    private async Task<Broker> AddBrokerAsync(
        string brokerCode = "BR-001", string name = "Broker One", BrokerStatus status = BrokerStatus.Active)
    {
        var broker = CreateBroker(brokerCode, name, status);
        await _repository.AddBrokerAsync(broker, CancellationToken.None);
        return broker;
    }

    private static Broker CreateBroker(
        string brokerCode = "BR-001", string name = "Broker One", BrokerStatus status = BrokerStatus.Active) =>
        new(brokerCode, name, $"{brokerCode.ToLowerInvariant()}@example.com", "123456", status);

    private static CreateBrokerRequest CreateRequest(
        string brokerCode = "BR-001", string name = "Broker One",
        string email = "broker@example.com", string phone = "123456",
        decimal? commissionPercentage = 12.5m) => new()
        {
            BrokerCode = brokerCode,
            Name = name,
            Email = email,
            Phone = phone,
            CommissionPercentage = commissionPercentage
        };
}
