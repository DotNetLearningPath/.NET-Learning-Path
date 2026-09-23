using Application.DTO.Brokers;
using Application.DTO.Common;

namespace Application.Abstractions;

public interface IBrokerService
{
    Task<BrokerDto> CreateBrokerAsync(CreateBrokerRequest request, CancellationToken cancellationToken = default);
    Task<BrokerDto?> GetBrokerByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<BrokerDto>> ListBrokersAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<BrokerDto> UpdateBrokerAsync(Guid id, UpdateBrokerRequest request, CancellationToken cancellationToken = default);
    Task<BrokerDto> ActivateBrokerAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BrokerDto> DeactivateBrokerAsync(Guid id, CancellationToken cancellationToken = default);
}
