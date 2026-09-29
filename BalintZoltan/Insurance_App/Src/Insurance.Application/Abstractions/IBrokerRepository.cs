using Application.DTO.Common;
using Insurance.Domain.Entities;

namespace Insurance.Application.Abstractions;

public interface IBrokerRepository
{
    Task AddBrokerAsync(Broker broker, CancellationToken cancellationToken = default);

    Task<bool> ExistsBrokerByCodeAsync(
        string brokerCode,
        Guid? excludedBrokerId = null,
        CancellationToken cancellationToken = default);

    Task<Broker?> GetBrokerByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Broker?> GetBrokerByCodeAsync(
        string brokerCode,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Broker>> ListBrokersAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);

    Task UpdateBrokerAsync(Broker broker, CancellationToken cancellationToken = default);
}
