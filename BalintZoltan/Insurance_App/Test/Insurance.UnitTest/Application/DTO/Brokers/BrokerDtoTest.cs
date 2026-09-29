using Application.DTO.Brokers;
using Domain.Enums;

namespace Insurance.UnitTest.Application.DTO.Brokers;

public sealed class BrokerDtoTest
{
    [Fact]
    public void BrokerDto_Should_Expose_All_Broker_Fields()
    {
        var id = Guid.NewGuid();
        var dto = new BrokerDto
        {
            Id = id, BrokerCode = "BR-001", Name = "Broker", Email = "broker@example.com",
            Phone = "123", Status = BrokerStatus.Active, CommissionPercentage = 15.5m
        };

        Assert.Equal(id, dto.Id);
        Assert.Equal("BR-001", dto.BrokerCode);
        Assert.Equal("Broker", dto.Name);
        Assert.Equal("broker@example.com", dto.Email);
        Assert.Equal("123", dto.Phone);
        Assert.Equal(BrokerStatus.Active, dto.Status);
        Assert.Equal(15.5m, dto.CommissionPercentage);
    }

    [Fact]
    public void CreateBrokerRequest_Should_Have_Expected_Defaults()
    {
        var request = new CreateBrokerRequest();

        Assert.Equal(string.Empty, request.BrokerCode);
        Assert.Equal(string.Empty, request.Name);
        Assert.Equal(string.Empty, request.Email);
        Assert.Equal(string.Empty, request.Phone);
        Assert.Null(request.CommissionPercentage);
    }

    [Fact]
    public void UpdateBrokerRequest_Should_Store_All_Values()
    {
        var request = new UpdateBrokerRequest
        {
            BrokerCode = "BR-002", Name = "Updated", Email = "updated@example.com",
            Phone = "456", CommissionPercentage = 10m
        };

        Assert.Equal("BR-002", request.BrokerCode);
        Assert.Equal("Updated", request.Name);
        Assert.Equal("updated@example.com", request.Email);
        Assert.Equal("456", request.Phone);
        Assert.Equal(10m, request.CommissionPercentage);
    }
}
