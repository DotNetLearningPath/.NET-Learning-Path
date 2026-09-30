using Insurance.Application.DTO.Brokers;
using Insurance.Application.DTO.Common;

namespace Insurance.Application.Abstractions;

public interface IBrokerService
{
    Task<BrokerDto> CreateBrokerAsync(CreateBrokerRequest request, CancellationToken cancellationToken);
    Task<BrokerDto?> GetBrokerByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<PagedResult<BrokerDto>> ListBrokersAsync(PaginationRequest pagination, CancellationToken cancellationToken);
    Task<BrokerDto> UpdateBrokerAsync(Guid id, UpdateBrokerRequest request, CancellationToken cancellationToken);
    Task<BrokerDto> ActivateBrokerAsync(Guid id, CancellationToken cancellationToken);
    Task<BrokerDto> DeactivateBrokerAsync(Guid id, CancellationToken cancellationToken);
}
