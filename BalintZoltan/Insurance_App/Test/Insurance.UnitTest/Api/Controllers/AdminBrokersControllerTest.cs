using Insurance.Api.Controllers;
using Insurance.Application.DTO.Brokers;
using Insurance.Application.DTO.Common;
using Insurance.Domain.Enums;
using Insurance.UnitTest.Application.Fakes;
using Microsoft.AspNetCore.Mvc;

namespace Insurance.UnitTest.Api.Controllers;

public sealed class AdminBrokersControllerTest
{
    private readonly FakeBrokerService _service;
    private readonly AdminBrokersController _controller;

    public AdminBrokersControllerTest()
    {
        _service = new FakeBrokerService();
        _controller = new AdminBrokersController(_service);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Ok_When_Broker_Exists()
    {
        _service.BrokerToReturn = CreateDto();

        var result = await _controller.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<BrokerDto>(ok.Value);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_NotFound_When_Broker_Does_Not_Exist()
    {
        var result = await _controller.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task ListAsync_Should_Return_Ok_With_Paged_Result()
    {
        _service.BrokersToReturn = new PagedResult<BrokerDto>
        {
            Items = [CreateDto()],
            PageNumber = 1,
            PageSize = 20,
            TotalCount = 1
        };

        var result = await _controller.ListAsync(new(), CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.IsType<PagedResult<BrokerDto>>(ok.Value);
    }

    [Fact]
    public async Task CreateAsync_Should_Return_Created_At_Route()
    {
        _service.BrokerToReturn = CreateDto();

        var result = await _controller.CreateAsync(new(), CancellationToken.None);

        var created = Assert.IsType<CreatedAtRouteResult>(result.Result);
        Assert.Equal(nameof(AdminBrokersController.GetByIdAsync), created.RouteName);
        Assert.IsType<BrokerDto>(created.Value);
    }

    [Fact]
    public async Task UpdateAsync_Should_Return_Ok()
    {
        _service.BrokerToReturn = CreateDto();
        var result = await _controller.UpdateAsync(Guid.NewGuid(), new(), CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task ActivateAsync_Should_Return_Ok()
    {
        _service.BrokerToReturn = CreateDto();
        var result = await _controller.ActivateAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task DeactivateAsync_Should_Return_Ok()
    {
        _service.BrokerToReturn = CreateDto();
        var result = await _controller.DeactivateAsync(Guid.NewGuid(), CancellationToken.None);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    private static BrokerDto CreateDto() => new()
    {
        Id = Guid.NewGuid(),
        BrokerCode = "BR-001",
        Name = "Broker",
        Email = "broker@example.com",
        Phone = "123",
        Status = BrokerStatus.Active
    };
}
