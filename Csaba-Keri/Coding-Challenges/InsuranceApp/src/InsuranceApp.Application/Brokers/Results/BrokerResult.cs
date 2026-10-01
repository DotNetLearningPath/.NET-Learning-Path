using InsuranceApp.Domain.Brokers;

namespace InsuranceApp.Application.Brokers.Results;

public record BrokerResult(
    Guid Id,
    string Code,
    string Name,
    string Email,
    string Phone,
    BrokerStatus Status
);
