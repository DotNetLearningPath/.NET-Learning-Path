using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Broker;

namespace InsuranceApp.Application.Abstractions.Services;

public interface IBrokerService
{
    Task<Result<IReadOnlyList<BrokerDto>>> GetBrokersAsync(CancellationToken cancellationToken);

    Task<Result<BrokerDto>> GetBrokerByIdAsync(Guid brokerId, CancellationToken cancellationToken);

    Task<Result<BrokerDto>> CreateBrokerAsync(CreateBrokerDto createBrokerDto, CancellationToken cancellationToken);

    Task<Result<BrokerDto>> UpdateBrokerAsync(Guid brokerId, UpdateBrokerDto updateBrokerDto, CancellationToken cancellationToken);

    Task<Result<BrokerDto>> ActivateBrokerAsync(Guid brokerId, CancellationToken cancellationToken);

    Task<Result<BrokerDto>> DeactivateBrokerAsync(Guid brokerId, CancellationToken cancellationToken);
}