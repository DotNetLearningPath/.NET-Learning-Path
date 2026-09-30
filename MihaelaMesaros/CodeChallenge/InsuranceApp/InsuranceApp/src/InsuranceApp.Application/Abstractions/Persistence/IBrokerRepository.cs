using InsuranceApp.Domain.Entities;

namespace InsuranceApp.Application.Abstractions.Persistence;

public interface IBrokerRepository
{
    Task<IReadOnlyList<Broker>> GetBrokersAsync(CancellationToken cancellationToken);

    Task<Broker?> GetBrokerByIdAsync(Guid brokerId, CancellationToken cancellationToken);

    Task<Broker?> GetBrokerForUpdateAsync(Guid brokerId, CancellationToken cancellationToken);

    Task<bool> BrokerCodeExistsAsync(string brokerCode, Guid? excludeBrokerId, CancellationToken cancellationToken);

    Task AddBrokerAsync(Broker broker, CancellationToken cancellationToken);

    Task SaveBrokerChangesAsync(CancellationToken cancellationToken);
}