using Insurance.Domain.Entities;
using Insurance.Domain.Enums;

namespace Insurance.UnitTest.Domain.Entities;

public sealed class BrokerTest
{
    private readonly Broker _broker;

    public BrokerTest()
    {
        _broker = new Broker("BR-001", "Broker One", "broker@example.com", "123");
    }

    [Fact]
    public void New_Broker_Should_Default_To_Active_One()
    {
        // Act
        var status = _broker.Status;

        // Assert
        Assert.Equal(BrokerStatus.Active, status);
        Assert.Equal(1, (int)status);
    }

    [Fact]
    public void Constructor_Should_Reject_Empty_Required_Values()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Broker("", "Broker", "broker@example.com", "123"));
        Assert.Throws<ArgumentException>(() => new Broker("BR-001", "", "broker@example.com", "123"));
        Assert.Throws<ArgumentException>(() => new Broker("BR-001", "Broker", "", "123"));
        Assert.Throws<ArgumentException>(() => new Broker("BR-001", "Broker", "broker@example.com", ""));
    }

    [Fact]
    public void Deactivate_And_Activate_Should_Change_Status()
    {
        // Act
        _broker.Deactivate();

        // Assert
        Assert.Equal(BrokerStatus.Inactive, _broker.Status);
        Assert.Equal(0, (int)_broker.Status);

        // Act
        _broker.Activate();

        // Assert
        Assert.Equal(BrokerStatus.Active, _broker.Status);
        Assert.Equal(1, (int)_broker.Status);
    }

    [Fact]
    public void Update_Should_Trim_Values_And_Change_Commission()
    {
        // Act
        _broker.Update(" BR-002 ", " Updated ", " updated@example.com ", " 456 ", 25m);

        // Assert
        Assert.Equal("BR-002", _broker.BrokerCode);
        Assert.Equal("Updated", _broker.Name);
        Assert.Equal("updated@example.com", _broker.Email);
        Assert.Equal("456", _broker.Phone);
        Assert.Equal(25m, _broker.CommissionPercentage);
    }

    [Fact]
    public void Update_Should_Reject_Invalid_Commission()
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => _broker.Update(
            "BR-001", "Broker One", "broker@example.com", "123", 101));
    }

    [Fact]
    public void Constructor_Should_Reject_Undefined_Status()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => new Broker(
            "BR-001", "Broker One", "broker@example.com", "123", (BrokerStatus)99));
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void Constructor_Should_Reject_Out_Of_Range_Commission(decimal commission)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new Broker(
            "BR-001", "Broker One", "broker@example.com", "123",
            commissionPercentage: commission));
    }
}
