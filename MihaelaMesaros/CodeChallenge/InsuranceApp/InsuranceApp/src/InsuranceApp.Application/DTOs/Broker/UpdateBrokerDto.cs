namespace InsuranceApp.Application.DTOs.Broker;

public sealed record UpdateBrokerDto(
    string BrokerCode,
    string Name,
    string? Email,
    string? Phone,
    decimal? CommissionPercentage
);
