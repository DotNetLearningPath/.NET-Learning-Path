using Insurance.Application.DTO.Common;
using Insurance.Domain.Entities;

namespace Insurance.Application.Abstractions;

public interface IBrokerRepository
{
    Task AddBrokerAsync(Broker broker, CancellationToken cancellationToken);

    Task<bool> ExistsBrokerByCodeAsync(
        string brokerCode,
        CancellationToken cancellationToken,
        Guid? excludedBrokerId = null);

    Task<bool> ExistsBrokerByCodeAsync(
        string brokerCode,
        Guid? excludedBrokerId = null,
        CancellationToken cancellationToken = default);

    Task<Broker?> GetBrokerByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<Broker?> GetBrokerByCodeAsync(
        string brokerCode,
        CancellationToken cancellationToken);

    Task<PagedResult<Broker>> ListBrokersAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken);

    Task UpdateBrokerAsync(Broker broker, CancellationToken cancellationToken);
}
