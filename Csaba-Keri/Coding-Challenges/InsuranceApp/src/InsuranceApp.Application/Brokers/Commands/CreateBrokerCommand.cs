using InsuranceApp.Domain.Brokers;

namespace InsuranceApp.Application.Brokers.Commands;

public record CreateBrokerCommand(
    string Code,
    string Name,
    string Email,
    string Phone,
    BrokerStatus Status
) : IBrokerDetailsCommand;
