using InsuranceApp.Domain.Brokers;

namespace InsuranceApp.UnitTests.Domain.Brokers;

public sealed class BrokerTests
{
    [Theory]
    [InlineData(BrokerStatus.Active)]
    [InlineData(BrokerStatus.Inactive)]
    public void Constructor_ValidValues_NormalizesFields(BrokerStatus status)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var broker = CreateBroker(
            id: id,
            code: " br-001 ",
            name: " Test Broker ",
            email: " broker@example.com ",
            phone: " 123 ",
            status: status
        );

        // Assert
        Assert.Equal(id, broker.Id);
        Assert.Equal("BR-001", broker.Code);
        Assert.Equal("Test Broker", broker.Name);
        Assert.Equal("broker@example.com", broker.Email);
        Assert.Equal("123", broker.Phone);
        Assert.Equal(status, broker.Status);
    }

    [Theory]
    [InlineData("", "Name", "valid@example.com", "123")]
    [InlineData("CODE", " ", "valid@example.com", "123")]
    [InlineData("CODE", "Name", "invalid", "123")]
    [InlineData("CODE", "Name", "Person <valid@example.com>", "123")]
    [InlineData("CODE", "Name", "valid@example.com", " ")]
    public void Constructor_InvalidDetails_Throws(string code, string name, string email, string phone)
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => CreateBroker(
                id: id,
                code: code,
                name: name,
                email: email,
                phone: phone,
                status: BrokerStatus.Active
            )
        );
    }

    [Fact]
    public void Constructor_EmptyId_Throws()
    {
        // Arrange
        var id = Guid.Empty;

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => CreateBroker(id: id)
        );
    }

    [Fact]
    public void Constructor_UndefinedStatus_Throws()
    {
        // Arrange
        var status = (BrokerStatus)999;

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(
            () => CreateBroker(status: status)
        );
    }

    [Fact]
    public void Constructor_CodeTooLong_Throws()
    {
        // Arrange
        var code = new string('A', Broker.MaxCodeLength + 1);

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => CreateBroker(code: code)
        );
    }

    [Fact]
    public void UpdateDetails_ValidValues_PreservesIdentityAndStatus()
    {
        // Arrange
        var broker = CreateBroker();
        var brokerId = broker.Id;

        // Act
        broker.UpdateDetails(
            name: " Updated ",
            email: " updated@example.com ",
            phone: " 456 "
        );

        // Assert
        Assert.Equal(brokerId, broker.Id);
        Assert.Equal("BR-001", broker.Code);
        Assert.Equal(BrokerStatus.Active, broker.Status);

        Assert.Equal("Updated", broker.Name);
        Assert.Equal("updated@example.com", broker.Email);
        Assert.Equal("456", broker.Phone);
    }

    [Fact]
    public void UpdateDetails_InvalidEmail_DoesNotPartiallyChangeState()
    {
        // Arrange
        var broker = CreateBroker();

        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => broker.UpdateDetails(
                name: "Changed",
                email: "invalid",
                phone: "456"
            )
        );

        Assert.Equal("Original", broker.Name);
        Assert.Equal("original@example.com", broker.Email);
        Assert.Equal("123", broker.Phone);
    }

    [Fact]
    public void ActivateAndDeactivate_RepeatedCalls_PreserveDetails()
    {
        // Arrange
        var broker = CreateBroker();

        // Act
        broker.Activate();
        broker.Activate();

        var activeStatus = broker.Status;

        broker.Deactivate();
        broker.Deactivate();

        // Assert
        Assert.Equal(BrokerStatus.Active, activeStatus);
        Assert.Equal(BrokerStatus.Inactive, broker.Status);

        Assert.Equal("BR-001", broker.Code);
        Assert.Equal("Original", broker.Name);
    }

    private static Broker CreateBroker(
        Guid? id = null,
        string code = "BR-001",
        string name = "Original",
        string email = "original@example.com",
        string phone = "123",
        BrokerStatus status = BrokerStatus.Active
    )
    {
        return new(
            id: id ?? Guid.NewGuid(),
            code: code,
            name: name,
            email: email,
            phone: phone,
            status: status
        );
    }
}
