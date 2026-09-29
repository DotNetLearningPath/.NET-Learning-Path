using Application.DTO.Common;
using Domain.Entities;

namespace Application.Abstractions;

public interface IBrokerRepository
{
    Task AddBrokerAsync(Broker broker, CancellationToken cancellationToken = default);

    Task<Broker?> GetBrokerByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Broker?> GetBrokerByCodeAsync(
        string brokerCode,
        CancellationToken cancellationToken = default);

    Task<PagedResult<Broker>> ListBrokersAsync(
        PaginationRequest pagination,
        CancellationToken cancellationToken = default);
}
