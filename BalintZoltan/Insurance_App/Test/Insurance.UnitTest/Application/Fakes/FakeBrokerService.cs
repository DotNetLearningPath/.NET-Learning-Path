using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Brokers;
using Insurance.Application.DTO.Common;

namespace Insurance.UnitTest.Application.Fakes;

public sealed class FakeBrokerService : IBrokerService
{
    public BrokerDto? BrokerToReturn { get; set; }
    public PagedResult<BrokerDto> BrokersToReturn { get; set; } = new PagedResult<BrokerDto>();
    public BrokerDto? LastBrokerResult { get; private set; }

    public Task<BrokerDto> CreateBrokerAsync(
        CreateBrokerRequest request,
        CancellationToken cancellationToken)
    {
        BrokerDto result;

        if (BrokerToReturn is not null)
        {
            result = BrokerToReturn;
        }
        else
        {
            result = CreateDto();
        }

        LastBrokerResult = result;
        return Task.FromResult(result);
    }

    public Task<BrokerDto?> GetBrokerByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(BrokerToReturn);
    }

    public Task<PagedResult<BrokerDto>> ListBrokersAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(BrokersToReturn);
    }

    public Task<BrokerDto> UpdateBrokerAsync(
        Guid id,
        UpdateBrokerRequest request,
        CancellationToken cancellationToken)
    {
        return CreateResultTask();
    }

    public Task<BrokerDto> ActivateBrokerAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return CreateResultTask();
    }

    public Task<BrokerDto> DeactivateBrokerAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return CreateResultTask();
    }

    private Task<BrokerDto> CreateResultTask()
    {
        BrokerDto result;

        if (BrokerToReturn is not null)
        {
            result = BrokerToReturn;
        }
        else
        {
            result = CreateDto();
        }

        LastBrokerResult = result;
        return Task.FromResult(result);
    }

    private static BrokerDto CreateDto()
    {
        return new BrokerDto
        {
            Id = Guid.NewGuid(),
            BrokerCode = "BR-001",
            Name = "Broker",
            Email = "broker@example.com",
            Phone = "123"
        };
    }
}
