using InsuranceApp.Application.Brokers.Commands;
using InsuranceApp.Application.Brokers.Results;
using InsuranceApp.Application.Common.Pagination;

namespace InsuranceApp.Application.Brokers;

public interface IBrokerService
{
    Task<BrokerResult> GetBrokerByIdAsync(Guid brokerId, CancellationToken cancellationToken);
    
    Task<PagedResult<BrokerResult>> GetBrokersAsync(PageQuery query, CancellationToken cancellationToken);
    
    Task<BrokerResult> CreateBrokerAsync(CreateBrokerCommand command, CancellationToken cancellationToken);
    
    Task<BrokerResult> UpdateBrokerAsync(UpdateBrokerCommand command, CancellationToken cancellationToken);
    
    Task<BrokerResult> ActivateBrokerAsync(Guid brokerId, CancellationToken cancellationToken);
    
    Task<BrokerResult> DeactivateBrokerAsync(Guid brokerId, CancellationToken cancellationToken);
}
