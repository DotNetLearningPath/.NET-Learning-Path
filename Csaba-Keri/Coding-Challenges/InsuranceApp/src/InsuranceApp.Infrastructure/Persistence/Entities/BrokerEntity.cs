using InsuranceApp.Domain.Brokers;

namespace InsuranceApp.Infrastructure.Persistence.Entities;

public class BrokerEntity(
    Guid id,
    string code,
    string name,
    string email,
    string phone,
    BrokerStatus status
)
{
    public Guid Id { get; private set; } = id;
    public string Code { get; private set; } = code;
    public string Name { get; private set; } = name;
    public string Email { get; private set; } = email;
    public string Phone { get; private set; } = phone;
    public BrokerStatus Status { get; private set; } = status;
}
