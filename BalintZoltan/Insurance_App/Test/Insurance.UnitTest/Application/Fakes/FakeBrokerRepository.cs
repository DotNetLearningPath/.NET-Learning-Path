using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Common;
using Insurance.Domain.Entities;

namespace Insurance.UnitTest.Application.Fakes;

public sealed class FakeBrokerRepository : IBrokerRepository
{
    public Dictionary<Guid, Broker> Storage { get; } = new();

    public Task AddBrokerAsync(Broker broker, CancellationToken cancellationToken)
    {
        Store(broker);
        return Task.CompletedTask;
    }

    public Task<Broker?> GetBrokerByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        Storage.TryGetValue(id, out var broker);
        return Task.FromResult(broker);
    }

    public Task<Broker?> GetBrokerByCodeAsync(string brokerCode, CancellationToken cancellationToken) =>
        Task.FromResult(Storage.Values.FirstOrDefault(broker => broker.BrokerCode == brokerCode));

    public Task<PagedResult<Broker>> ListBrokersAsync(PaginationRequest pagination, CancellationToken cancellationToken)
    {
        var pageNumber = Math.Max(pagination.PageNumber, 1);
        var pageSize = Math.Min(Math.Max(pagination.PageSize, 1), 100);
        var brokers = Storage.Values.OrderBy(broker => broker.Name).ThenBy(broker => broker.Id).ToList();

        return Task.FromResult(new PagedResult<Broker>
        {
            Items = brokers.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList(),
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = brokers.Count
        });
    }

    public Task UpdateBrokerAsync(Broker broker, CancellationToken cancellationToken)
    {
        Store(broker);
        return Task.CompletedTask;
    }

    private void Store(Broker broker)
    {
        bool duplicateCodeExists = Storage.Values.Any(existingBroker =>
            existingBroker.BrokerCode == broker.BrokerCode
            && existingBroker.Id != broker.Id);

        if (duplicateCodeExists)
        {
            throw new InvalidOperationException(
                "A broker with this code already exists.");
        }

        Storage[broker.Id] = broker;
    }
}
