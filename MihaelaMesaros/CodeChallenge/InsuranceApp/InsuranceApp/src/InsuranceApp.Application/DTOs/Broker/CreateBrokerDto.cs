namespace InsuranceApp.Application.DTOs.Broker;

public sealed record CreateBrokerDto(
    string BrokerCode,
    string Name,
    string? Email,
    string? Phone,
    decimal? CommissionPercentage,
    bool IsActive
);
