namespace InsuranceApp.Application.DTOs.Broker;

public sealed record BrokerDto(
    Guid BrokerId,
    string BrokerCode,
    string Name,
    string? Email,
    string? Phone,
    decimal? CommissionPercentage,
    bool IsActive
);
