using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Domain.Brokers;

namespace InsuranceApp.Application.Brokers;

public interface IBrokerRepository
{
    Task<Broker?> GetBrokerByIdAsync(Guid brokerId, CancellationToken cancellationToken);
    
    Task<PagedResult<Broker>> GetBrokersAsync(PageQuery query, CancellationToken cancellationToken);
    
    Task<bool> BrokerExistsByCodeAsync(string code, CancellationToken cancellationToken);
    
    Task AddBrokerAsync(Broker broker, CancellationToken cancellationToken);
    
    Task UpdateBrokerAsync(Broker broker, CancellationToken cancellationToken);
}
