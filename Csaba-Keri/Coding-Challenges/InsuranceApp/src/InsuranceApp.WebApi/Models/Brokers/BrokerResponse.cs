namespace InsuranceApp.WebApi.Models.Brokers;

public record BrokerResponse(
    Guid Id,
    string Code,
    string Name,
    string Email,
    string Phone,
    BrokerStatusDto Status
);
